using ClickableTransparentOverlay;
using ImGuiNET;
using l4d2External; // Asegúrate que Entity.cs y Offsets.cs están en este namespace
using System.Runtime.InteropServices;
using System.Text;
using Swed32;
using System.Numerics;
using SharpDX.Direct3D11; // Presente en tu código original

namespace left4dead2Menu
{
    class Program : Overlay
    {
        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // GetWindowRect no es llamado estáticamente desde fuera, así que puede no ser público.
        // public static RECT GetWindowRect(IntPtr hWnd) 
        // {
        //     RECT rect = new RECT();
        //     GetWindowRect(hWnd, out rect);
        //     return rect;
        // }

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        int specialAimbotKey = 0x05;
        Encoding encoding = Encoding.ASCII;
        ImDrawListPtr drawList; // Declarado en tu código original
        Offsets offsets = new Offsets();
        Swed swed = new Swed("left4dead2");

        Entity localPlayer = new Entity();
        List<Entity> commonInfected = new List<Entity>();
        List<Entity> specialInfected = new List<Entity>();
        List<Entity> survivors = new List<Entity>();

        // Vector3 offsetVector = new Vector3(0, 0, 15); // Usaremos aimbotTargetZOffset para esto
        Vector3 offsetVectorCommon = new Vector3(0, 0, 10); // No usado activamente por el aimbot principal

        IntPtr client;
        IntPtr engine;

        // --- Variables de Configuración del Aimbot y FOV ---
        bool EnableAimbot = true;
        float aimbotTargetZOffset = 25.0f;  // Tu valor original de desplazamientoZ, ahora configurable
        bool drawFovCircle = true;
        float fovCircleVisualRadius = 100.0f;

        Vector4 fovCircleColor = new Vector4(1, 1, 1, 0.5f); // Blanco semitransparente
        float aimbotSmoothness = 0.1f;      // Suavizado del Aimbot

        // Variables para dimensiones de pantalla (actualizadas en Render)
        private float screenWidth;
        private float screenHeight;
        private Vector2 centerScreen;

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        const int SURVIVOR_TEAM = 2;
        const int INFECTED_TEAM = 3;

        protected override void Render()
        {
            // Actualizar dimensiones de la pantalla y centro
            screenWidth = ImGui.GetIO().DisplaySize.X;
            screenHeight = ImGui.GetIO().DisplaySize.Y;
            centerScreen = new Vector2(screenWidth / 2, screenHeight / 2);

            DrawMenu(); // Dibuja tu menú

            // Dibujar el círculo del FOV si está activado
            if (EnableAimbot && drawFovCircle && fovCircleVisualRadius > 0)
            {
                ImGui.GetBackgroundDrawList().AddCircle(centerScreen, fovCircleVisualRadius, ImGui.GetColorU32(fovCircleColor), 32, 1.5f);
            }

            ImGui.End(); // Cierra la ventana del menú que se abrió en DrawMenu()
        }

        void MainLogic()
        {
            client = swed.GetModuleBase("client.dll");
            engine = swed.GetModuleBase("engine.dll");

            while (true)
            {
                ReloadEntities();
                Aimbot();
                // int o = 0; // Variable no usada en tu código original
                Thread.Sleep(5);
            }
        }

        void DrawMenu()
        {
            ImGui.Begin("l4d2 external by Russ");
            if (ImGui.BeginTabBar("tabs"))
            {
                if (ImGui.BeginTabItem("general"))
                {
                    ImGui.Checkbox("Aimbot", ref EnableAimbot); // "aimbot" en minúscula como en tu original
                    if (EnableAimbot)
                    {
                        ImGui.SliderFloat("Desplazamiento Z Aimbot", ref aimbotTargetZOffset, -50.0f, 50.0f, "%.1f u");
                        ImGui.Text("Positivo: más abajo, Negativo: más arriba del punto 'abs'");
                        ImGui.Separator();
                        ImGui.Text("Configuración FOV:");
                        ImGui.Checkbox("Dibujar Círculo FOV", ref drawFovCircle);
                        ImGui.SliderFloat("Radio Círculo Visual", ref fovCircleVisualRadius, 10.0f, 300.0f, "%.0f px");
                        ImGui.Separator();
                        ImGui.Text("Configuración Suavizado:");
                        ImGui.SliderFloat("Suavizado Aimbot", ref aimbotSmoothness, 0.01f, 1.0f, "%.2f");
                    }
                }
                ImGui.EndTabItem(); // Final del TabItem "general"

                // Tu TabItem "esp" original
                if (ImGui.BeginTabItem("esp"))
                {
                    // Aquí irían tus opciones de ESP
                    ImGui.Text("Opciones de ESP (a implementar)");
                }
                ImGui.EndTabItem(); // Final del TabItem "esp"
            }
            ImGui.EndTabBar();
            // ImGui.End() se llama en el método Render()
        }

        void Aimbot()
        {
            if (!EnableAimbot || !(GetAsyncKeyState(specialAimbotKey) < 0) || specialInfected.Count == 0)
            {
                return;
            }

            Entity bestTarget = null;
            float closestAngleDistance = fovCircleVisualRadius / 2.0f;

            var engineAnglesBase = swed.ReadPointer(engine, offsets.engineAngles);
            // Lectura correcta de Pitch y Yaw actuales
            Vector3 currentViewAngles = new Vector3(
                swed.ReadFloat(engineAnglesBase, offsets.engineAnglesOffset),       // Pitch
                swed.ReadFloat(engineAnglesBase, offsets.engineAnglesOffset + 0x4), // Yaw
                0
            );

            foreach (Entity targetEntity in specialInfected)
            {
                Vector3 puntoBase = targetEntity.abs;
                // Usar el aimbotTargetZOffset configurable en lugar del desplazamientoZ hardcodeado
                Vector3 aimPosition = new Vector3(puntoBase.X, puntoBase.Y, puntoBase.Z - aimbotTargetZOffset);

                Vector3 targetAngles = CalculateAngles(localPlayer.abs, aimPosition);

                float deltaYaw = NormalizeAngle(targetAngles.Y - currentViewAngles.Y);
                float deltaPitch = NormalizeAngle(targetAngles.X - currentViewAngles.X);

                float angleDistance = (float)Math.Sqrt(deltaYaw * deltaYaw + deltaPitch * deltaPitch);

                if (angleDistance < closestAngleDistance)
                {
                    closestAngleDistance = angleDistance;
                    bestTarget = targetEntity;
                }
            }

            if (bestTarget != null)
            {
                Vector3 puntoBase = bestTarget.abs;
                Vector3 finalAimPosition = new Vector3(puntoBase.X, puntoBase.Y, puntoBase.Z - aimbotTargetZOffset);

                Vector3 targetViewAngles = CalculateAngles(localPlayer.abs, finalAimPosition);

                Vector3 smoothedAngles = new Vector3();
                smoothedAngles.X = Lerp(currentViewAngles.X, targetViewAngles.X, aimbotSmoothness);
                smoothedAngles.Y = Lerp(currentViewAngles.Y, targetViewAngles.Y, aimbotSmoothness);
                smoothedAngles.Z = 0;

                AimAt(smoothedAngles);
            }
        }

        // Tu función AimAt original (Pitch primero, Yaw segundo)
        void AimAt(Vector3 angles) // angles.X = pitch, angles.Y = yaw
        {
            var engineAnglesBase = swed.ReadPointer(engine, offsets.engineAngles);
            swed.WriteFloat(engineAnglesBase, offsets.engineAnglesOffset, angles.X); // Pitch
            swed.WriteFloat(engineAnglesBase, offsets.engineAnglesOffset + 0x4, angles.Y);  // Yaw
        }

        // Tu función CalculateMagnitude original
        float CalculateMagnitude(Vector3 from, Vector3 destination)
        {
            return (float)Math.Sqrt(Math.Pow(destination.X - from.X, 2) + Math.Pow(destination.Y - from.Y, 2) + Math.Pow(destination.Z - from.Z, 2));
        }

        // Tu función CalculateAngles original
        Vector3 CalculateAngles(Vector3 from, Vector3 destination)
        {
            float yaw;
            float pitch;

            float deltaX = destination.X - from.X;
            float deltaY = destination.Y - from.Y;
            float deltaZ = destination.Z - from.Z;

            yaw = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);

            double distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2)); // xyDistance
            pitch = -(float)(Math.Atan2(deltaZ, distance) * 180 / Math.PI);

            return new Vector3(pitch, yaw, 0);
        }

        // Tu función ReloadEntities original
        void ReloadEntities()
        {
            commonInfected.Clear();
            specialInfected.Clear();
            survivors.Clear();

            localPlayer.address = swed.ReadPointer(client, offsets.localplayer);
            if (localPlayer.address != IntPtr.Zero) // Buena práctica: verificar si el puntero es válido
            {
                UpdateEntity(localPlayer);
            }

            UpdateEntities();

            // Ordenar listas (como en tu original)
            if (commonInfected.Count > 0) commonInfected = commonInfected.OrderBy(x => x.magnitude).ToList();
            if (specialInfected.Count > 0) specialInfected = specialInfected.OrderBy(x => x.magnitude).ToList();
        }

        // Tu función UpdateEntities original (con la lógica de lifeState != 2 para 'continue')
        void UpdateEntities()
        {
            for (int i = 0; i < 900; i++)
            {
                Entity entity = new Entity();
                entity.address = swed.ReadPointer(client + offsets.entityList, i * 0x10);

                if (entity.address == IntPtr.Zero || entity.address == localPlayer.address) // Evitar procesar el jugador local como un enemigo
                {
                    continue;
                }

                UpdateEntity(entity); // Llama a tu UpdateEntity

                // Tu lógica de filtrado: si lifeState NO ES 2, salta la entidad.
                // Esto significa que solo entidades con lifeState == 2 son consideradas más adelante.
                if (entity.lifeState != 2)
                {
                    continue;
                }

                // El resto de tu lógica para añadir a listas (solo se ejecutará si lifeState ES 2)
                if (entity.teamNum == SURVIVOR_TEAM && entity.health > 0)
                {
                    survivors.Add(entity);
                }
                else if (entity.teamNum == INFECTED_TEAM && entity.health > 0)
                {
                    if (entity.modelName != null && entity.modelName.Contains("inf")) // Añadida comprobación de nulidad para entity.name
                    {
                        commonInfected.Add(entity);
                    }
                    else if (entity.modelName != null) // Si no es "inf" y el nombre no es nulo
                    {
                        // La comprobación if (entity.health > 0) aquí es redundante con la del 'else if' exterior
                        specialInfected.Add(entity);
                    }
                }
            }
        }

        // Tu función UpdateEntity original (solo procesa detalles si lifeState ES 2)
        void UpdateEntity(Entity entity)
        {
            entity.lifeState = swed.ReadInt(entity.address, offsets.Lifestate);

            // Tu lógica: si lifeState NO ES 2, retorna.
            // Solo se leen más datos si lifeState ES 2.
            if (entity.lifeState != 2)
                return;

            // Las siguientes lecturas solo ocurren si lifeState == 2
            entity.origin = swed.ReadVec(entity.address, offsets.Origin);
            entity.viewOffset = swed.ReadVec(entity.address, offsets.ViewOffset);
            entity.abs = Vector3.Add(entity.origin, entity.viewOffset);

            entity.health = swed.ReadInt(entity.address, offsets.Health);
            entity.teamNum = swed.ReadInt(entity.address, offsets.TeamNum);
            entity.jumpflag = swed.ReadInt(entity.address, offsets.JumpFlag); // jumpflag no se usa actualmente
            entity.magnitude = CalculateMagnitude(entity.origin, localPlayer.origin);

            var entityStringPointer = swed.ReadPointer(entity.address, 0x10); // Offset 0x10 para nombre
            if (entityStringPointer != IntPtr.Zero) // Buena práctica: verificar puntero nulo
            {
                try
                {
                    entity.modelName = encoding.GetString(swed.ReadBytes(entityStringPointer, 10));
                }
                catch // Capturar posible error si el puntero/offset es incorrecto
                {
                    entity.modelName = "ERR_NAME";
                }
            }
            else
            {
                entity.modelName = "NULL_NAME_PTR";
            }
        }

        // --- FUNCIONES AUXILIARES PARA AIMBOT FOV Y SUAVIZADO ---
        float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        float Lerp(float start, float end, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount)); // Clamp amount entre 0 y 1
            return start + amount * NormalizeAngle(end - start);
        }

        static void Main(string[] args)
        {
            Program program = new Program();

            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE); // Oculta la consola

            Thread MainLogicThread = new Thread(program.MainLogic)
            {
                IsBackground = true
            };
            MainLogicThread.Start();

            program.Start().Wait(); // Inicia el overlay (bloqueante)
        }
    }
}