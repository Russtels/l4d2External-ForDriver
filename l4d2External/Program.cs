// l4d2External/Program.cs
using ClickableTransparentOverlay;
using ImGuiNET;
using l4d2External;
using System.Numerics;
using System.Text;

namespace left4dead2Menu
{
    class Program : Overlay
    {
        private readonly GameMemory memory;
        private static readonly object _listLock = new object();
        private readonly Offsets offsets = new Offsets();

        // Listas base
        private readonly Entity localPlayer = new Entity();
        private readonly List<Entity> commonInfected = new List<Entity>();
        private readonly List<Entity> specialInfected = new List<Entity>();
        private readonly List<Entity> bossInfected = new List<Entity>();
        private readonly List<Entity> survivors = new List<Entity>();

        // Listas para renderizado
        private readonly List<Entity> commonInfected_render = new List<Entity>();
        private readonly List<Entity> specialInfected_render = new List<Entity>();
        private readonly List<Entity> bossInfected_render = new List<Entity>();
        private readonly List<Entity> survivors_render = new List<Entity>();
        private readonly List<Entity> aimTargets_render = new List<Entity>();
        private readonly List<Entity> meleeTargets = new List<Entity>();

        // Managers y Controladores
        private EntityManager entityManager = null!;
        private AimbotController aimbotController = null!;
        private GuiManager guiManager = null!;
        private Renderer renderer = null!;
        private BunnyHop bunnyHopController = null!;
        private Areas areaController = new Areas();
        private IntPtr clientModule, engineModule;
        private TriggerBot triggerBotController = null!;
        private Config config;
        private readonly int specialAimbotKey = 0x06;
        private AutoCelling autoCellingController = null!;
        private AutoShove autoShoveController = null!;
        private AutoLevel autoLevelController = null!;


        public Program()
        {
            config = ConfigManager.LoadConfig();
            try { memory = new GameMemory("left4dead2"); } catch (Exception ex) { Console.WriteLine($"Fallo en inicialización: {ex.Message}"); Environment.Exit(1); }
        }

        private void InitializeLogicModules()
        {
            clientModule = memory.client;
            engineModule = memory.engine;
            if (clientModule == IntPtr.Zero || engineModule == IntPtr.Zero) return;
            entityManager = new EntityManager(memory, offsets, Encoding.ASCII);
            aimbotController = new AimbotController();
            renderer = new Renderer(memory, engineModule, offsets);
            bunnyHopController = new BunnyHop(memory, offsets); // Esta línea es la que probablemente faltaba
            bunnyHopController.Start();
            triggerBotController = new TriggerBot();
            autoShoveController = new AutoShove();
            autoCellingController = new AutoCelling();
            autoLevelController = new AutoLevel();
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
            if (guiManager != null) { guiManager.DrawMenu(ref config); }
            ImGui.End();

            if (renderer == null) return;
            renderer.UpdateViewMatrix();

            lock (_listLock)
            {
                commonInfected_render.Clear(); commonInfected_render.AddRange(commonInfected);
                specialInfected_render.Clear(); specialInfected_render.AddRange(specialInfected);
                bossInfected_render.Clear(); bossInfected_render.AddRange(bossInfected);
                survivors_render.Clear(); survivors_render.AddRange(survivors);
            }

            if (config.EnableAimbot && NativeMethods.GetAsyncKeyState(specialAimbotKey) < 0)
            {
                aimTargets_render.Clear();
                if (config.AimbotOnBosses) aimTargets_render.AddRange(bossInfected_render);
                if (config.AimbotOnSpecials) aimTargets_render.AddRange(specialInfected_render);
                if (config.AimbotOnCommons) aimTargets_render.AddRange(commonInfected_render);
                if (config.AimbotOnSurvivors) aimTargets_render.AddRange(survivors_render);
                if (aimTargets_render.Any())
                {
                    var bestTarget = aimbotController.FindBestTarget(localPlayer, aimTargets_render, config.EnableAimbotArea, config.AimbotAreaRadius, config.FovCircleVisualRadius, renderer, screenWidth, screenHeight);
                    if (bestTarget != null) { aimbotController.ExecuteMouseAimbot(bestTarget, config.AimbotTargetSelection, config.AimbotSmoothness, renderer, screenWidth, screenHeight); }
                }
            }

            if (localPlayer.address != IntPtr.Zero)
            {
                if (config.EnableAutoShove) areaController.DrawCircleArea(ImGui.GetBackgroundDrawList(), localPlayer.origin, renderer, screenWidth, screenHeight, config.ShoveRadius, config.ShoveAreaSegments, config.ShoveAreaColor);
                if (config.EnableAimbotArea) areaController.DrawCircleArea(ImGui.GetBackgroundDrawList(), localPlayer.origin, renderer, screenWidth, screenHeight, config.AimbotAreaRadius, config.AimbotAreaSegments, config.AimbotAreaColor);
                if (config.EnableAimbot && config.DrawFovCircle && !config.EnableAimbotArea) renderer.DrawFovCircle(ImGui.GetBackgroundDrawList(), centerScreen, config.FovCircleVisualRadius, new Vector4(1, 1, 1, 0.5f));
            }
            if (config.EnableEsp)
            {
                renderer.RenderAll(ImGui.GetBackgroundDrawList(), screenWidth, screenHeight, commonInfected_render, specialInfected_render, bossInfected_render, survivors_render, config);
            }

            if (config.EnableTriggerBot)
            {
                var triggerTargets = new List<Entity>();
                lock (_listLock)
                {
                    triggerTargets.AddRange(commonInfected);
                    triggerTargets.AddRange(specialInfected);
                    triggerTargets.AddRange(bossInfected);
                    triggerTargets.AddRange(survivors);
                }

                // Actualizamos las propiedades del TriggerBot desde la config
                triggerBotController.IsEnabled = config.EnableTriggerBot;
                triggerBotController.TriggerRadius = config.TriggerBotRadius;
                triggerBotController.TriggerOnBosses = config.TriggerOnBosses;
                triggerBotController.TriggerOnSpecials = config.TriggerOnSpecials;
                triggerBotController.TriggerOnCommons = config.TriggerOnCommons;
                triggerBotController.TriggerOnSurvivors = config.TriggerOnSurvivors;

                triggerBotController.Update(renderer, screenWidth, screenHeight, triggerTargets);
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
                    entityManager.ReloadEntities(localPlayer, commonInfected, specialInfected, bossInfected, survivors, memory.client);
                    autoShoveController.Update(commonInfected, specialInfected, config);
                    autoLevelController.Update(commonInfected, specialInfected, bossInfected, config);
                }

                if (localPlayer.address != IntPtr.Zero)
                {
                    bunnyHopController.IsEnabled = config.EnableBunnyHop;
                    bunnyHopController.ClientModule = clientModule;
                    bunnyHopController.LocalPlayer = localPlayer;

                    if (config.EnableAutoCelling && NativeMethods.GetAsyncKeyState(config.AutoCellingKey) < 0)
                    {
                        autoCellingController.Update(memory, engineModule, offsets);
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
                Console.WriteLine($"\n--- ERROR CRÍTICO ---\n{ex}");
                Console.ReadKey();
            }
        }
    }
}