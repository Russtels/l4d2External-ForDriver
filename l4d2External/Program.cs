using ClickableTransparentOverlay;
using ImGuiNET;
using l4d2External; // Para Entity, Offsets
using System.Text;
using Swed32;
using System.Numerics;
using SharpDX.Direct3D11; // Presente en tu código original, aunque no se use explícitamente aquí

namespace left4dead2Menu
{
    class Program : Overlay
    {
        // --- Instancias y Variables Principales ---
        private readonly Encoding encoding = Encoding.ASCII;
        private readonly Offsets offsets = new Offsets();
        private readonly Swed swed = new Swed("left4dead2"); // Nombre del proceso

        private readonly Entity localPlayer = new Entity();
        private readonly List<Entity> commonInfected = new List<Entity>();
        private readonly List<Entity> specialInfected = new List<Entity>();
        private readonly List<Entity> survivors = new List<Entity>();

        private IntPtr clientModule;
        private IntPtr engineModule;

        // --- Gestores de Lógica ---
        private EntityManager entityManager;
        private AimbotController aimbotController;
        private GuiManager guiManager;

        // --- Variables de Configuración (Accesibles por GuiManager) ---
        private bool enableAimbot = true;
        private float aimbotTargetZOffset = 25.0f;
        private bool drawFovCircle = true;
        private float fovCircleVisualRadius = 100.0f;
        private Vector4 fovCircleColor = new Vector4(1, 1, 1, 0.5f); // Blanco semitransparente
        private float aimbotSmoothness = 0.1f;
        private readonly int specialAimbotKey = 0x05; // VK_XBUTTON2 (Botón lateral del ratón)

        // Variables para dimensiones de pantalla
        private float screenWidth;
        private float screenHeight;
        private Vector2 centerScreen;

        public Program() // Constructor
        {
            // Inicializar módulos y gestores aquí o en un método aparte si se prefiere
            // Esto se llamará antes de que el overlay comience su bucle
        }

        private void InitializeLogicModules()
        {
            clientModule = swed.GetModuleBase("client.dll");
            engineModule = swed.GetModuleBase("engine.dll");

            if (clientModule == IntPtr.Zero || engineModule == IntPtr.Zero)
            {
                Console.WriteLine("Error: No se pudieron obtener los módulos base. Asegúrate que el juego está corriendo.");
                // Considera cerrar la aplicación o manejar este error de forma adecuada.
                // Environment.Exit(1); // Ejemplo
                return;
            }

            entityManager = new EntityManager(swed, offsets, encoding);
            aimbotController = new AimbotController(swed, engineModule, offsets);
            guiManager = new GuiManager();
        }

        protected override void Render()
        {
            // Actualizar dimensiones de la pantalla y centro
            screenWidth = ImGui.GetIO().DisplaySize.X;
            screenHeight = ImGui.GetIO().DisplaySize.Y;
            centerScreen = new Vector2(screenWidth / 2, screenHeight / 2);

            ImGui.Begin("l4d2 external by Russ"); // Nombre de la ventana del menú

            if (guiManager != null) // Asegurarse que está inicializado
            {
                guiManager.DrawMenuControls(
                   ref enableAimbot,
                   ref aimbotTargetZOffset,
                   ref drawFovCircle,
                   ref fovCircleVisualRadius,
                   ref aimbotSmoothness);
            }
            else
            {
                ImGui.Text("Error: GuiManager no inicializado.");
            }

            ImGui.End(); // Cierra la ventana del menú

            // Dibujar el círculo del FOV si está activado
            if (guiManager != null && enableAimbot && drawFovCircle && fovCircleVisualRadius > 0)
            {
                guiManager.DrawFovCircle(ImGui.GetBackgroundDrawList(), centerScreen, fovCircleVisualRadius, fovCircleColor);
            }
        }

        void MainLogicLoop() // Renombrado de MainLogic para claridad
        {
            InitializeLogicModules(); // Mover inicialización aquí para que se ejecute en el Thread

            // Verificar si la inicialización falló
            if (clientModule == IntPtr.Zero || engineModule == IntPtr.Zero || entityManager == null || aimbotController == null || guiManager == null)
            {
                Console.WriteLine("Deteniendo MainLogicLoop debido a un error de inicialización.");
                return; // No continuar si hay errores
            }

            while (true)
            {
                entityManager.ReloadEntities(localPlayer, commonInfected, specialInfected, survivors, clientModule);

                if (enableAimbot && NativeMethods.GetAsyncKeyState(specialAimbotKey) < 0) // Tecla presionada
                {
                    // Asegurarse que localPlayer es válido antes de pasarlo al aimbot
                    if (localPlayer.address != IntPtr.Zero)
                    {
                        aimbotController.PerformAimbotActions(localPlayer, specialInfected, fovCircleVisualRadius, aimbotTargetZOffset, aimbotSmoothness);
                    }
                }
                Thread.Sleep(5); // Pequeña pausa para no sobrecargar la CPU
            }
        }

        static void Main(string[] args)
        {
            Program program = new Program();

            IntPtr consoleHandle = NativeMethods.GetConsoleWindow();
            NativeMethods.ShowWindow(consoleHandle, GameConstants.SW_HIDE); // Oculta la consola

            Thread mainLogicThread = new Thread(program.MainLogicLoop)
            {
                IsBackground = true // El hilo terminará cuando la aplicación principal cierre
            };
            mainLogicThread.Start();

            program.Start().Wait(); // Inicia el overlay (bloqueante hasta que cierre)
        }
    }
}