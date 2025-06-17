// Program.cs (Versión Final)
using ClickableTransparentOverlay;
using ImGuiNET;
using l4d2External;
using System.Text;
using Swed32;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace left4dead2Menu
{
    class Program : Overlay
    {
        private static readonly object _listLock = new object();
        private readonly Swed swed = new Swed("left4dead2");
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
        private IntPtr clientModule, engineModule;

        // --- Configuración ---
        private bool enableAimbot = true;
        private float aimbotTargetZOffset = 25.0f;
        private float aimbotSmoothness = 0.1f;
        private bool aimbotOnBosses = true;
        private bool aimbotOnSpecials = true;
        private bool aimbotOnCommons = false;
        private bool aimbotOnSurvivors = false;
        private bool drawFovCircle = true;
        private float fovCircleVisualRadius = 100.0f;
        private Vector4 fovCircleColor = new Vector4(1, 1, 1, 0.5f);
        private readonly int specialAimbotKey = 0x05;
        private bool enableEsp = true;
        private bool espOnBosses = true;
        private bool espOnSpecials = true;
        private bool espOnCommons = true;
        private bool espOnSurvivors = true;
        private Vector4 espColorBosses = new Vector4(1, 0, 0, 1);
        private Vector4 espColorSpecials = new Vector4(1, 0.6f, 0, 1);
        private Vector4 espColorCommons = new Vector4(0.8f, 0.8f, 0.8f, 0.7f);
        private Vector4 espColorSurvivors = new Vector4(0.2f, 0.8f, 1, 1);

        private void InitializeLogicModules()
        {
            clientModule = swed.GetModuleBase("client.dll");
            engineModule = swed.GetModuleBase("engine.dll");
            if (clientModule == IntPtr.Zero || engineModule == IntPtr.Zero) return;

            entityManager = new EntityManager(swed, offsets, Encoding.ASCII);
            aimbotController = new AimbotController(swed, engineModule, offsets);
            guiManager = new GuiManager();
            renderer = new Renderer(swed, engineModule, offsets);
        }

        protected override void Render()
        {
            float screenWidth = ImGui.GetIO().DisplaySize.X;
            float screenHeight = ImGui.GetIO().DisplaySize.Y;
            Vector2 centerScreen = new Vector2(screenWidth / 2, screenHeight / 2);

            ImGui.Begin("l4d2 external by Russ");
            guiManager.DrawMenuControls(
                ref enableAimbot, ref aimbotTargetZOffset, ref drawFovCircle, ref fovCircleVisualRadius, ref aimbotSmoothness,
                ref aimbotOnBosses, ref aimbotOnSpecials, ref aimbotOnCommons, ref aimbotOnSurvivors,
                ref enableEsp, ref espOnBosses, ref espColorBosses, ref espOnSpecials, ref espColorSpecials,
                ref espOnCommons, ref espColorCommons, ref espOnSurvivors, ref espColorSurvivors
            );
            ImGui.End();

            if (renderer != null)
            {
                renderer.UpdateViewMatrix();

                if (enableAimbot && drawFovCircle)
                {
                    renderer.DrawFovCircle(ImGui.GetBackgroundDrawList(), centerScreen, fovCircleVisualRadius, fovCircleColor);
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
                        espOnCommons, espColorCommons, espOnSurvivors, espColorSurvivors
                    );
                }
            }
        }

        void MainLogicLoop()
        {
            InitializeLogicModules();
            if (entityManager == null || aimbotController == null) return;

            while (true)
            {
                lock (_listLock)
                {
                    entityManager.ReloadEntities(localPlayer, commonInfected, specialInfected, bossInfected, survivors, clientModule);
                }

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
                        aimTargets.Sort((a, b) => a.magnitude.CompareTo(b.magnitude));
                        aimbotController.PerformAimbotActions(localPlayer, aimTargets, fovCircleVisualRadius, aimbotTargetZOffset, aimbotSmoothness);
                    }
                }
                Thread.Sleep(5);
            }
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            IntPtr consoleHandle = NativeMethods.GetConsoleWindow();
            NativeMethods.ShowWindow(consoleHandle, GameConstants.SW_HIDE);
            Thread mainLogicThread = new Thread(program.MainLogicLoop) { IsBackground = true };
            mainLogicThread.Start();
            program.Start().Wait();
        }
    }
}