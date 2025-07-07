// l4d2External/Program.cs (RESTAURADO)
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

        // <<< VUELVEN LAS LISTAS SEPARADAS >>>
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
        private TriggerBot triggerBotController = null!;
        private Config config;
        private bool hasPerformedMeleeShove = false;
        private readonly int specialAimbotKey = 0x06;

        public Program()
        {
            config = ConfigManager.LoadConfig();

            try
            {
                memory = new GameMemory("left4dead2");
            }
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
            renderer = new Renderer(memory, engineModule, offsets);
            bunnyHopController = new BunnyHop(memory, offsets);
            bunnyHopController.Start();
            triggerBotController = new TriggerBot();
            guiManager = new GuiManager();
            guiManager.OnSaveConfig += () => ConfigManager.SaveConfig(config);
            guiManager.OnLoadConfig += () => config = ConfigManager.LoadConfig();
        }

        protected override void Render()
        {
            float screenWidth = ImGui.GetIO().DisplaySize.X;
            float screenHeight = ImGui.GetIO().DisplaySize.Y;
            Vector2 centerScreen = new Vector2(screenWidth / 2, screenHeight / 2);

            ImGui.Begin("l4d2 external by Russ");
            if (guiManager != null)
            {
                guiManager.DrawMenu(ref config);
            }
            ImGui.End();

            if (renderer == null) return;

            renderer.UpdateViewMatrix();

            if (config.EnableAimbot && NativeMethods.GetAsyncKeyState(specialAimbotKey) < 0)
            {
                var aimTargets = new List<Entity>();
                lock (_listLock)
                {
                    if (config.AimbotOnBosses) aimTargets.AddRange(bossInfected);
                    if (config.AimbotOnSpecials) aimTargets.AddRange(specialInfected);
                    if (config.AimbotOnCommons) aimTargets.AddRange(commonInfected);
                    if (config.AimbotOnSurvivors) aimTargets.AddRange(survivors);
                }
                if (aimTargets.Count > 0)
                {
                    var bestTarget = aimbotController.FindBestTarget(localPlayer, aimTargets, config.EnableAimbotArea, config.AimbotAreaRadius, config.FovCircleVisualRadius, renderer, screenWidth, screenHeight);
                    if (bestTarget != null)
                    {
                        aimbotController.ExecuteMouseAimbot(bestTarget, config.AimbotTargetSelection, config.AimbotSmoothness, renderer, screenWidth, screenHeight);
                    }
                }
            }

            if (localPlayer.address != IntPtr.Zero)
            {
                if (config.EnableMeleeArea)
                    areaController.DrawCircleArea(ImGui.GetBackgroundDrawList(), localPlayer.origin, renderer, screenWidth, screenHeight, config.MeleeAreaRadius, config.MeleeAreaSegments, config.MeleeAreaColor);
                if (config.EnableAimbotArea)
                    areaController.DrawCircleArea(ImGui.GetBackgroundDrawList(), localPlayer.origin, renderer, screenWidth, screenHeight, config.AimbotAreaRadius, config.AimbotAreaSegments, config.AimbotAreaColor);
                if (config.EnableAimbot && config.DrawFovCircle && !config.EnableAimbotArea)
                    renderer.DrawFovCircle(ImGui.GetBackgroundDrawList(), centerScreen, config.FovCircleVisualRadius, new Vector4(1, 1, 1, 0.5f));
            }

            if (config.EnableTriggerBot)
            {
                var allEntitiesForTrigger = new List<Entity>();
                lock (_listLock)
                {
                    allEntitiesForTrigger.AddRange(bossInfected);
                    allEntitiesForTrigger.AddRange(specialInfected);
                    allEntitiesForTrigger.AddRange(commonInfected);
                    allEntitiesForTrigger.AddRange(survivors);
                }

                // Pasamos la configuración directamente al triggerbot
                triggerBotController.IsEnabled = config.EnableTriggerBot;
                triggerBotController.TriggerRadius = config.TriggerBotRadius;
                triggerBotController.TriggerOnBosses = config.TriggerOnBosses;
                triggerBotController.TriggerOnSpecials = config.TriggerOnSpecials;
                triggerBotController.TriggerOnCommons = config.TriggerOnCommons;
                triggerBotController.TriggerOnSurvivors = config.TriggerOnSurvivors;

                triggerBotController.Update(renderer, screenWidth, screenHeight, allEntitiesForTrigger);
            }

            if (config.EnableEsp)
            {
                List<Entity> commonSnapshot, specialSnapshot, bossSnapshot, survivorSnapshot;
                lock (_listLock)
                {
                    commonSnapshot = new List<Entity>(commonInfected);
                    specialSnapshot = new List<Entity>(specialInfected);
                    bossSnapshot = new List<Entity>(bossInfected);
                    survivorSnapshot = new List<Entity>(survivors);
                }

                // <<< LLAMADA AL RENDERER RESTAURADA >>>
                renderer.RenderAll(
                    ImGui.GetBackgroundDrawList(), screenWidth, screenHeight,
                    commonSnapshot, specialSnapshot, bossSnapshot, survivorSnapshot,
                    config
                );
            }
        }

        void MainLogicLoop()
        {
            InitializeLogicModules();
            if (entityManager == null) return;

            while (true)
            {
                lock (_listLock)
                {
                    // <<< LLAMADA AL ENTITYMANAGER RESTAURADA >>>
                    entityManager.ReloadEntities(localPlayer, commonInfected, specialInfected, bossInfected, survivors, clientModule);
                }

                if (localPlayer.address != IntPtr.Zero)
                {
                    bunnyHopController.IsEnabled = config.EnableBunnyHop;
                    bunnyHopController.ClientModule = clientModule;
                    bunnyHopController.LocalPlayer = localPlayer;
                    if (config.EnableMeleeArea)
                    {
                        var meleeTargets = new List<Entity>();
                        lock (_listLock)
                        {
                            if (config.MeleeOnCommons) meleeTargets.AddRange(commonInfected);

                            var specialMeleeTargets = specialInfected.Where(s =>
                                (config.MeleeOnHunter && s.SimpleName == "Hunter") ||
                                (config.MeleeOnSmoker && s.SimpleName == "Smoker") ||
                                (config.MeleeOnBoomer && s.SimpleName == "Boomer") ||
                                (config.MeleeOnJockey && s.SimpleName == "Jockey") ||
                                (config.MeleeOnSpitter && s.SimpleName == "Spitter") ||
                                (config.MeleeOnCharger && s.SimpleName == "Charger")
                            ).ToList();
                            meleeTargets.AddRange(specialMeleeTargets);
                        }
                        bool enemyInMeleeRange = meleeTargets.Any(e => e.health > 0 && e.magnitude <= config.MeleeAreaRadius);
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