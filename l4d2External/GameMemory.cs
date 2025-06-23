// GameMemory.cs (Modificado)
using l4d2External;
using System;
using System.Numerics;

namespace left4dead2Menu
{
    internal class GameMemory
    {
        // Se reemplaza Swed por nuestra nueva clase Driver
        public Driver driver { get; private set; }
        public int processId { get; private set; }
        public Offsets offsets { get; private set; }
        public IntPtr client { get; private set; }
        public IntPtr engine { get; private set; }

        public GameMemory(string processName)
        {
            try
            {
                // Obtenemos el ID del proceso
                processId = ProcessUtils.GetProcessIdByName(processName);

                // Inicializamos el Driver y nos vinculamos al proceso
                driver = new Driver();
                if (!driver.AttachToProcess(processId))
                {
                    throw new Exception("Error CRÍTICO: No se pudo vincular al proceso a través del driver.");
                }
                Console.WriteLine($"Vinculado al proceso {processName} (PID: {processId}) via driver.");

                offsets = new Offsets();
                InitializeModules();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la inicialización: {ex.Message}");
                Console.WriteLine("Presiona una tecla para salir...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        private void InitializeModules()
        {
            // Usamos nuestra nueva utilidad para obtener las bases de los módulos
            client = ProcessUtils.GetModuleBaseAddress(processId, "client.dll");
            engine = ProcessUtils.GetModuleBaseAddress(processId, "engine.dll");

            if (client == IntPtr.Zero)
            {
                Console.WriteLine("Error CRÍTICO: No se pudo encontrar el módulo client.dll.");
            }
            else
            {
                Console.WriteLine($"Módulo client.dll cargado en: {client.ToString("X")}");
            }

            if (engine == IntPtr.Zero)
            {
                Console.WriteLine("Error CRÍTICO: No se pudo encontrar el módulo engine.dll.");
            }
            else
            {
                Console.WriteLine($"Módulo engine.dll cargado en: {engine.ToString("X")}");
            }
        }

        // Métodos de conveniencia para mantener la compatibilidad con el resto del código
        public T Read<T>(IntPtr address) where T : struct
        {
            return driver.ReadMemory<T>(address);
        }

        public IntPtr ReadPointer(IntPtr baseAddress, int offset)
        {
            return driver.ReadMemory<IntPtr>(baseAddress + offset);
        }

        public Vector3 ReadVec(IntPtr address, int offset)
        {
            return driver.ReadMemory<Vector3>(address + offset);
        }

        public int ReadInt(IntPtr address, int offset)
        {
            return driver.ReadMemory<int>(address + offset);
        }

        public byte[] ReadBytes(IntPtr address, int size)
        {
            return driver.ReadBytes(address, size);
        }
    }
}