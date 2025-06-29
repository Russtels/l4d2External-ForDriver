// l4d2External/BunnyHop.cs (REFACTORIZADO PARA HILO DEDICADO)
using System;
using System.Threading;
using l4d2External;

namespace left4dead2Menu
{
    internal class BunnyHop
    {
        private readonly GameMemory memory;
        private readonly Offsets offsets;

        // --- Propiedades para la comunicación entre hilos ---
        // Usamos un objeto de bloqueo para asegurar la sincronización y evitar errores.
        private readonly object _lock = new object();
        private bool _isEnabled;
        private IntPtr _clientModule;
        private Entity _localPlayer = new Entity();

        // Propiedades públicas para que el hilo principal las actualice
        public bool IsEnabled
        {
            get { lock (_lock) return _isEnabled; }
            set { lock (_lock) _isEnabled = value; }
        }
        public IntPtr ClientModule
        {
            get { lock (_lock) return _clientModule; }
            set { lock (_lock) _clientModule = value; }
        }
        public Entity LocalPlayer
        {
            get { lock (_lock) return _localPlayer; }
            set { lock (_lock) _localPlayer = value; }
        }

        // Valores para ForceJump
        private const int JUMP_STATE = 5;
        private const int NORMAL_STATE = 4;

        // Constantes para el estado del jugador
        private const int STANDING = 129;
        private const int DUCKING = 131;

        public BunnyHop(GameMemory memory, Offsets offsets)
        {
            this.memory = memory;
            this.offsets = offsets;
        }

        /// <summary>
        /// Inicia el hilo dedicado para la lógica del BunnyHop.
        /// </summary>
        public void Start()
        {
            Thread bunnyHopThread = new Thread(BunnyHopLoop)
            {
                IsBackground = true, // El hilo terminará cuando la aplicación principal se cierre
                Name = "BunnyHopThread"
            };
            bunnyHopThread.Start();
        }

        /// <summary>
        /// Bucle principal que se ejecuta en el hilo dedicado.
        /// </summary>
        private void BunnyHopLoop()
        {
            while (true)
            {
                // Copiamos los valores actuales de las propiedades para trabajar con ellos
                // de forma segura dentro del hilo.
                bool currentIsEnabled = IsEnabled;
                IntPtr currentClientModule = ClientModule;
                Entity currentLocalPlayer = LocalPlayer;

                if (!currentIsEnabled || currentClientModule == IntPtr.Zero || currentLocalPlayer.address == IntPtr.Zero)
                {
                    // Si está desactivado o no tenemos datos, esperamos un poco para no consumir CPU inútilmente.
                    Thread.Sleep(100);
                    continue; // Volvemos al inicio del bucle
                }

                // Calculamos la dirección de memoria de ForceJump
                IntPtr forceJumpAddress = currentClientModule + offsets.ForceJump;

                // Leemos el estado de salto (jumpFlag) del jugador local
                int jumpFlag = memory.ReadInt(currentLocalPlayer.address, offsets.JumpFlag);

                // Verificamos si la barra espaciadora está presionada
                bool spacePressed = NativeMethods.GetAsyncKeyState(NativeMethods.VK_SPACE) < 0;

                if (spacePressed)
                {
                    if (jumpFlag == STANDING || jumpFlag == DUCKING)
                    {
                        memory.WriteInt(forceJumpAddress, JUMP_STATE);
                    }
                    else
                    {
                        memory.WriteInt(forceJumpAddress, NORMAL_STATE);
                    }
                }
                else
                {
                    memory.WriteInt(forceJumpAddress, NORMAL_STATE);
                }

                // Pequeña pausa para evitar el uso excesivo de la CPU y hacer el hilo más eficiente.
                // Un valor bajo (1-5ms) asegura una alta capacidad de respuesta.
                Thread.Sleep(1);
            }
        }
    }
}