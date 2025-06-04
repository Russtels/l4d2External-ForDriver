using ClickableTransparentOverlay;
using ImGuiNET;
using l4d2External;
using System.Runtime.InteropServices;
using System.Text;
using Swed32;
using System.Numerics;
using SharpDX.Direct3D11;

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

        public static RECT GetWindowRect(IntPtr hWnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hWnd, out rect);
            return rect;
        }

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        int specialAimbotKey = 0x05; 
        Encoding encoding = Encoding.ASCII;
        ImDrawListPtr drawList;
        Offsets offsets = new Offsets();
        Swed swed = new Swed("left4dead2");

        Entity localPlayer = new Entity();
        List<Entity> commonInfected= new List<Entity>();
        List<Entity> specialInfected = new List<Entity>();
        List<Entity> survivors= new List<Entity>();


        Vector3 offsetVector = new Vector3(0, 0, 15);
        Vector3 offsetVectorCommon = new Vector3(0, 0, 10);

        IntPtr client;
        IntPtr engine;

        bool EnableAimbot = true;

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        const int SURVIVOR_TEAM = 2;
        const int INFECTED_TEAM = 3;





        protected override void Render()
        {
            DrawMenu();
            ImGui.End();
        }

        void MainLogic()
        {
            client = swed.GetModuleBase("client.dll");
            engine = swed.GetModuleBase("engine.dll");
     

             while (true)
            {
                ReloadEntities();

                Aimbot();

                int o = 0;

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
                    ImGui.Checkbox("aimbot", ref EnableAimbot);
                }
                ImGui.EndTabItem();

            }
            if(ImGui.BeginTabItem("esp"))
            {
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }

        void Aimbot()
        {
            if (GetAsyncKeyState(specialAimbotKey) < 0) // Verifica si la tecla está presionada
            {
                if (specialInfected.Count > 0 && EnableAimbot) // Verifica si hay objetivos y el aimbot está activo
                {
                    Entity targetEntity = specialInfected[0]; // Tomas el primer infectado especial
                    Vector3 puntoBase = targetEntity.abs;    // Este es tu punto de referencia actual (ojos/cabeza)

                    // --- AQUÍ ES DONDE CAMBIAS EL VALOR PARA APUNTAR MÁS ALTO O MÁS BAJO ---
                    // Define cuántas unidades quieres desplazarte verticalmente desde 'puntoBase'.
                    // Un valor POSITIVO para 'desplazamientoZ' apuntará MÁS ABAJO.
                    // Un valor NEGATIVO para 'desplazamientoZ' apuntará MÁS ARRIBA.

                    float desplazamientoZ = 20.0f; // EJEMPLO: Apuntar 20 unidades hacia abajo.
                                                   // CAMBIA ESTE NÚMERO (20.0f) SEGÚN LO QUE NECESITES:
                                                   // - Para más abajo: un número mayor (ej. 25.0f, 30.0f)
                                                   // - Para menos abajo (más cerca de abs): un número menor (ej. 10.0f, 5.0f)
                                                   // - Para apuntar directo a abs: 0.0f
                                                   // - Para apuntar más arriba: un número negativo (ej. -10.0f para 10 unidades arriba)

                    Vector3 aimPosition = new Vector3(puntoBase.X,
                                                      puntoBase.Y,
                                                      puntoBase.Z - desplazamientoZ); // Se resta porque un Z mayor suele ser "arriba" en coordenadas del mundo,
                                                                                      // así que restar a la Z del objetivo apunta más abajo en su modelo.
                                                                                      // --- FIN DE LA MODIFICACIÓN DEL PUNTO DE MIRA ---

                    var angles = CalculateAngles(localPlayer.abs, aimPosition);
                    AimAt(angles);
                }
            }
        }

        void AimAt(Vector3 angles)
        {
            var engineAnglesBase = swed.ReadPointer(engine, offsets.engineAngles);

            // Se asume que la estructura de los ángulos de visión en memoria es:
            // 1. Pitch (contenido en angles.X)
            // 2. Yaw   (contenido en angles.Y)
            // 3. Roll  (no modificado aquí, angles.Z usualmente es 0)

            // Escribir Pitch (angles.X) en la dirección base de los ángulos de visión.
            swed.WriteFloat(engineAnglesBase, offsets.engineAnglesOffset, angles.X);

            // Escribir Yaw (angles.Y) en la dirección base + 4 bytes (siguiente float).
            swed.WriteFloat(engineAnglesBase, offsets.engineAnglesOffset + 0x4, angles.Y);  
        }


        float CalculateMagnitude(Vector3 from, Vector3 destination)
        {
            return (float)Math.Sqrt(Math.Pow(destination.X - from.X, 2) + Math.Pow(destination.Y - from.Y, 2) + Math.Pow(destination.Z - from.Z, 2));
        }

        Vector3 CalculateAngles(Vector3 from, Vector3 destination)
        {
            float yaw;
            float pitch;

            float deltaX = destination.X - from.X;
            float deltaY = destination.Y - from.Y;
            float deltaZ = destination.Z - from.Z;

            yaw = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);

            double distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
            pitch = -(float)(Math.Atan2(deltaZ, distance) * 180 / Math.PI);

            return new Vector3(pitch, yaw, 0);

        }

        void ReloadEntities()
        {
            commonInfected.Clear();
            specialInfected.Clear();
            survivors.Clear();

           localPlayer.address = swed.ReadPointer(client, offsets.localplayer);
            UpdateEntity(localPlayer);
            UpdateEntities();

            commonInfected = commonInfected.OrderBy(x => x.magnitude).ToList();
            specialInfected = specialInfected.OrderBy(x => x.magnitude).ToList();



        }

        void UpdateEntities()
        {
            for (int i = 0; i < 900; i++)
            {
                Entity entity = new Entity();
                entity.address = swed.ReadPointer(client + offsets.entityList, i * 0x10);

                if ( entity.address == IntPtr.Zero)
                    {
                    continue;
                }
                UpdateEntity(entity);
                if (entity.lifeState != 2)
                {
                    continue;
                }

                if(entity.teamNum == SURVIVOR_TEAM && entity.health >0)
                { 
                    survivors.Add(entity);
                }
                else if (entity.teamNum == INFECTED_TEAM && entity.health > 0)
                { 
                    if (entity.name.Contains("inf"))
                    {
                        commonInfected.Add(entity);
                    }
                    else 
                    {
                        if (entity.health > 0)
                        {
                            specialInfected.Add(entity);
                        }
                    }
                }
            }
        }




        void UpdateEntity(Entity entity)
        {
            entity.lifeState = swed.ReadInt(entity.address, offsets.Lifestate);

            if (entity.lifeState !=2)
                return;

            entity.origin = swed.ReadVec(entity.address, offsets.Origin);
            entity.viewOffset = swed.ReadVec(entity.address, offsets.ViewOffset);
            entity.abs = Vector3.Add(entity.origin, entity.viewOffset);

            entity.health = swed.ReadInt(entity.address, offsets.Health);
            entity.teamNum = swed.ReadInt(entity.address, offsets.TeamNum);
            entity.jumpflag = swed.ReadInt(entity.address, offsets.JumpFlag);
            entity.magnitude = CalculateMagnitude(entity.origin, localPlayer.origin);

            var entityStringPointer = swed.ReadPointer(entity.address, 0x10);
            entity.name = encoding.GetString(swed.ReadBytes(entityStringPointer, 10));
        }
        
        


        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start().Wait();

            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            Thread MainLogicThread = new Thread(program.MainLogic)
            {
                IsBackground = true
            };
            MainLogicThread.Start();
        }

    }
}