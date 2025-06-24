// l4d2External/Program.cs (Versión Final y Corregida)

using ClickableTransparentOverlay;
using ImGuiNET;
using l4d2External;
using System.Text;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;

namespace left4dead2Menu
{
    class Program : Overlay
    {
        private readonly GameMemory memory;
        private static readonly object _listLock = new object();
        private readonly Offsets offsets = new Offsets();
        private readonly Entity localPlayer = new Entity();
        private readonly List<Entity> commonInfected = new List<Entity>();
        private readonly List<Entity> specialInfected = new List<Entity>();
        private readonly List<Entity> bossInfected = new List<Entity>();
        private readonly List<Entity> survivors = new List<Entity>();

        private EntityManager entityManager = null!;
        private AimbotController aimbotController = null!;
        private GuiManager guiManager = null!;
        private Renderer renderer = null!;
        private BunnyHop bunnyHopController = null!;
        private Areas areaController = new Areas();
        private IntPtr clientModule, engineModule;

        private bool hasPerformedMeleeShove = false;

        // --- Configuración Aimbot ---
        private bool enableAimbot = true;
        private float aimbotSmoothness = 0.1f;
        private AimbotTarget aimbotTargetSelection = AimbotTarget.Head;
        private bool aimbotOnBosses = true;
        private bool aimbotOnSpecials = true;
        private bool aimbotOnCommons = false;
        private bool aimbotOnSurvivors = false;
        private bool drawFovCircle = true;
        private float fovCircleVisualRadius = 100.0f;
        private readonly int specialAimbotKey = 0x06;
        private bool enableAimbotArea = false;
        private float aimbotAreaRadius = 300.0f;
        private int aimbotAreaSegments = 40;
        private Vector4 aimbotAreaColor = new Vector4(1, 0, 1, 0.7f);

        // --- Configuración ESP ---
        private bool enableEsp = true;
        private bool espOnBosses = true;
        private bool espOnSpecials = true;
        private bool espOnCommons = true;
        private bool espOnSurvivors = true;
        private Vector4 espColorBosses = new Vector4(1, 0, 0, 1);
        private Vector4 espColorSpecials = new Vector4(1, 0.6f, 0, 1);
        private Vector4 espColorCommons = new Vector4(0.8f, 0.8f, 0.8f, 0.7f);
        private Vector4 espColorSurvivors = new Vector4(0.2f, 0.8f, 1, 1);
        private bool espDrawBones = false;
        private bool espDrawSkeleton = true;
        private Vector4 colorNombreFill = new Vector4(1, 1, 1, 1);
        private Vector4 colorNombreBorde = new Vector4(0, 0, 0, 1);
        private Vector4 colorCajaFill = new Vector4(0, 0, 0, 0.3f);
        private Vector4 colorCajaBorde = new Vector4(1, 1, 1, 1);
        private Vector4 colorEsqueletoFill = new Vector4(1, 1, 1, 1);
        private Vector4 colorEsqueletoBorde = new Vector4(0, 0, 0, 1);

        // --- Configuración Others ---
        private bool enableBunnyHop = true;
        private bool enableMeleeArea = true;
        private float meleeAreaRadius = 80.0f;
        private int meleeAreaSegments = 40;
        private Vector4 meleeAreaColor = new Vector4(0, 1, 1, 0.7f);
        private bool meleeOnCommons = true;
        private bool meleeOnHunter = true;
        private bool meleeOnSmoker = true;
        private bool meleeOnBoomer = true;
        private bool meleeOnJockey = true;
        private bool meleeOnSpitter = false;
        private bool meleeOnCharger = false;

        public Program()
        {
            try { memory = new GameMemory("left4dead2"); }
            catch (Exception ex)
            {
                Console.WriteLine($"Fallo en inicialización de Program: {ex.Message}");
                Environment.Exit(1);
            }
        }
        private void InitializeLogicModules()
        {
            clientModule = memory.client;
            engineModule = memory.engine;
            if (clientModule == IntPtr.Zero || engineModule == IntPtr.Zero) return;

            entityManager = new EntityManager(memory, offsets, Encoding.ASCII);
            aimbotController = new AimbotController();
            guiManager = new GuiManager();
            renderer = new Renderer(memory, engineModule, offsets);
            bunnyHopController = new BunnyHop(memory, offsets);
        }

        protected override void Render()
        {
            float screenWidth = ImGui.GetIO().DisplaySize.X;
            float screenHeight = ImGui.GetIO().DisplaySize.Y;
            Vector2 centerScreen = new Vector2(screenWidth / 2, screenHeight / 2);

            ImGui.Begin("l4d2 external by Russ");

            guiManager.DrawMenuControls(
                ref enableAimbot, ref aimbotSmoothness,
                ref aimbotTargetSelection,
                ref aimbotOnBosses, ref aimbotOnSpecials, ref aimbotOnCommons, ref aimbotOnSurvivors,
                ref drawFovCircle, ref fovCircleVisualRadius,
                ref enableAimbotArea, ref aimbotAreaRadius, ref aimbotAreaSegments, ref aimbotAreaColor,
                ref enableEsp, ref espOnBosses, ref espColorBosses,
                ref espOnSpecials, ref espColorSpecials,
                ref espOnCommons, ref espColorCommons,
                ref espOnSurvivors, ref espColorSurvivors,
                ref espDrawBones, ref espDrawSkeleton,
                ref colorNombreFill, ref colorNombreBorde,
                ref colorCajaFill, ref colorCajaBorde,
                ref colorEsqueletoFill, ref colorEsqueletoBorde,
                ref enableBunnyHop,
                ref enableMeleeArea, ref meleeAreaRadius, ref meleeAreaSegments, ref meleeAreaColor,
                ref meleeOnCommons,
                ref meleeOnHunter, ref meleeOnSmoker, ref meleeOnBoomer,
                ref meleeOnJockey, ref meleeOnSpitter, ref meleeOnCharger
            );
            ImGui.End();

            if (renderer != null)
            {
                renderer.UpdateViewMatrix();

                if (enableAimbot && NativeMethods.GetAsyncKeyState(specialAimbotKey) < 0)
                {
                    var aimTargets = new List<Entity>();
                    lock (_listLock)
                    {
                        if (aimbotOnBosses) aimTargets.AddRange(bossInfected);
                        if (aimbotOnSpecials) aimTargets.AddRange(specialInfected);
                        if (aimbotOnCommons) aimTargets.AddRange(commonInfected);
                        if (aimbotOnSurvivors) aimTargets.AddRange(survivors);
                    }

                    if (aimTargets.Count > 0)
                    {
                        var bestTarget = aimbotController.FindBestTarget(localPlayer, aimTargets, enableAimbotArea, aimbotAreaRadius, fovCircleVisualRadius, renderer, screenWidth, screenHeight);
                        if (bestTarget != null)
                        {
                            aimbotController.ExecuteMouseAimbot(bestTarget, aimbotTargetSelection, aimbotSmoothness, renderer, screenWidth, screenHeight);
                        }
                    }
                }

                if (enableMeleeArea && localPlayer.address != IntPtr.Zero)
                {
                    areaController.DrawCircleArea(ImGui.GetBackgroundDrawList(), localPlayer.origin, renderer, screenWidth, screenHeight, meleeAreaRadius, meleeAreaSegments, meleeAreaColor);
                }
                if (enableAimbotArea && localPlayer.address != IntPtr.Zero)
                {
                    areaController.DrawCircleArea(ImGui.GetBackgroundDrawList(), localPlayer.origin, renderer, screenWidth, screenHeight, aimbotAreaRadius, aimbotAreaSegments, aimbotAreaColor);
                }
                if (enableAimbot && drawFovCircle && !enableAimbotArea)
                {
                    renderer.DrawFovCircle(ImGui.GetBackgroundDrawList(), centerScreen, fovCircleVisualRadius, new Vector4(1, 1, 1, 0.5f));
                }
                if (enableEsp)
                {
                    List<Entity> commonSnapshot, specialSnapshot, bossSnapshot, survivorSnapshot;
                    lock (_listLock)
                    {
                        commonSnapshot = new List<Entity>(commonInfected);
                        specialSnapshot = new List<Entity>(specialInfected);
                        bossSnapshot = new List<Entity>(bossInfected);
                        survivorSnapshot = new List<Entity>(survivors);
                    }
                    renderer.RenderAll(
                        ImGui.GetBackgroundDrawList(), screenWidth, screenHeight,
                        commonSnapshot, specialSnapshot, bossSnapshot, survivorSnapshot,
                        espOnBosses, espColorBosses, espOnSpecials, espColorSpecials,
                        espOnCommons, espColorCommons, espOnSurvivors, espColorSurvivors,
                        espDrawBones, espDrawSkeleton,
                        colorNombreFill, colorNombreBorde,
                        colorCajaFill, colorCajaBorde,
                        colorEsqueletoFill, colorEsqueletoBorde
                    );
                }
            }
        }

        void MainLogicLoop()
        {
            InitializeLogicModules();
            if (entityManager == null || bunnyHopController == null) return;

            while (true)
            {
                lock (_listLock)
                {
                    entityManager.ReloadEntities(localPlayer, commonInfected, specialInfected, bossInfected, survivors, clientModule);
                }

                if (localPlayer.address != IntPtr.Zero)
                {
                    bunnyHopController.Update(localPlayer.address, enableBunnyHop);

                    if (enableMeleeArea)
                    {
                        var meleeTargets = new List<Entity>();
                        lock (_listLock)
                        {
                            if (meleeOnCommons) meleeTargets.AddRange(commonInfected);
                            foreach (var special in specialInfected)
                            {
                                switch (special.SimpleName)
                                {
                                    case "Hunter": if (meleeOnHunter) meleeTargets.Add(special); break;
                                    case "Smoker": if (meleeOnSmoker) meleeTargets.Add(special); break;
                                    case "Boomer": if (meleeOnBoomer) meleeTargets.Add(special); break;
                                    case "Jockey": if (meleeOnJockey) meleeTargets.Add(special); break;
                                    case "Spitter": if (meleeOnSpitter) meleeTargets.Add(special); break;
                                    case "Charger": if (meleeOnCharger) meleeTargets.Add(special); break;
                                }
                            }
                        }
                        bool enemyInMeleeRange = meleeTargets.Any(e => e.health > 0 && e.magnitude <= meleeAreaRadius);
                        if (enemyInMeleeRange && !hasPerformedMeleeShove)
                        {
                            NativeMethods.SimulateRightClick();
                            hasPerformedMeleeShove = true;
                        }
                        else if (!enemyInMeleeRange)
                        {
                            hasPerformedMeleeShove = false;
                        }
                    }
                }
                Thread.Sleep(5);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Program program = new Program();
                IntPtr consoleHandle = NativeMethods.GetConsoleWindow();
                NativeMethods.ShowWindow(consoleHandle, GameConstants.SW_SHOW);
                Thread mainLogicThread = new Thread(program.MainLogicLoop) { IsBackground = true };
                mainLogicThread.Start();
                program.Start().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n--- ERROR CRÍTICO ---\n");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("\nPresiona cualquier tecla para salir...");
                Console.ReadKey();
            }
        }
    }
}