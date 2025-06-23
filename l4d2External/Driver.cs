// Driver.cs
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Linq;

namespace left4dead2Menu
{
    /// <summary>
    /// Clase principal para la comunicación con el driver de kernel FunnyDriver.
    /// Reemplaza la funcionalidad de Swed32 para leer memoria.
    /// </summary>
    public class Driver : IDisposable
    {
        private IntPtr handle = IntPtr.Zero;

        #region WinAPI P/Invoke

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        #endregion

        #region Driver Structures and Codes

        // Códigos de control (IOCTL) que coinciden con los del driver en C++
        private enum DriverCodes : uint
        {
            // CORRECCIÓN: Valores IOCTL recalculados a sus literales hexadecimales correctos
            // basados en la macro CTL_CODE. Este es el cambio más importante.
            Attach = 0x221A58, // CTL_CODE(FILE_DEVICE_UNKNOWN, 0x696, METHOD_BUFFERED, FILE_SPECIAL_ACCESS)
            Read = 0x221A5C,   // CTL_CODE(FILE_DEVICE_UNKNOWN, 0x697, METHOD_BUFFERED, FILE_SPECIAL_ACCESS)
            Write = 0x221A60   // CTL_CODE(FILE_DEVICE_UNKNOWN, 0x698, METHOD_BUFFERED, FILE_SPECIAL_ACCESS)
        }

        // Estructura que coincide con la struct 'Request' en el driver
        // CORRECCIÓN: Se añade Pack = 1 para forzar la alineación de bytes y evitar
        // problemas de padding entre C# y C++.
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Request
        {
            public IntPtr process_id;
            public IntPtr target;
            public IntPtr buffer;
            public UIntPtr size;
            public UIntPtr return_size;
        }

        #endregion

        public Driver()
        {
            // Abre un handle al symbolic link creado por el driver
            handle = CreateFile(
                @"\\.\FunnyDriver",
                FileAccess.ReadWrite,
                FileShare.ReadWrite,
                IntPtr.Zero,
                FileMode.Open,
                0,
                IntPtr.Zero);

            if (handle == new IntPtr(-1))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "No se pudo crear el handle al driver. ¿Está cargado FunnyDriver.sys?");
            }
        }

        /// <summary>
        /// Vincula el driver al proceso especificado por su ID.
        /// </summary>
        public bool AttachToProcess(int pid)
        {
            var request = new Request { process_id = (IntPtr)pid };
            IntPtr requestPtr = Marshal.AllocHGlobal(Marshal.SizeOf(request));
            Marshal.StructureToPtr(request, requestPtr, false);

            bool result = DeviceIoControl(
                handle,
                (uint)DriverCodes.Attach,
                requestPtr, (uint)Marshal.SizeOf(request),
                requestPtr, (uint)Marshal.SizeOf(request),
                out _, IntPtr.Zero);

            Marshal.FreeHGlobal(requestPtr);

            if (!result)
            {
                Console.WriteLine($"Fallo al vincularse al proceso {pid}. Error: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
            }

            return result;
        }

        /// <summary>
        /// Lee un valor genérico (struct) de la memoria del proceso vinculado.
        /// </summary>
        public T ReadMemory<T>(IntPtr address) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr bufferPtr = Marshal.AllocHGlobal(size);

            var request = new Request
            {
                target = address,
                buffer = bufferPtr,
                size = (UIntPtr)size // CORRECCIÓN: Cast a UIntPtr
            };

            IntPtr requestPtr = Marshal.AllocHGlobal(Marshal.SizeOf(request));
            Marshal.StructureToPtr(request, requestPtr, false);

            if (!DeviceIoControl(handle, (uint)DriverCodes.Read, requestPtr, (uint)Marshal.SizeOf(request), requestPtr, (uint)Marshal.SizeOf(request), out _, IntPtr.Zero))
            {
                Console.WriteLine($"Error al leer memoria en {address.ToString("X")}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
            }

            T value = (T)Marshal.PtrToStructure(bufferPtr, typeof(T));

            Marshal.FreeHGlobal(bufferPtr);
            Marshal.FreeHGlobal(requestPtr);

            return value;
        }

        /// <summary>
        /// Lee un array de bytes de la memoria del proceso vinculado.
        /// </summary>
        public byte[] ReadBytes(IntPtr address, int size)
        {
            byte[] bytes = new byte[size];
            IntPtr bufferPtr = Marshal.AllocHGlobal(size);

            var request = new Request
            {
                target = address,
                buffer = bufferPtr,
                size = (UIntPtr)size // CORRECCIÓN: Cast a UIntPtr
            };

            IntPtr requestPtr = Marshal.AllocHGlobal(Marshal.SizeOf(request));
            Marshal.StructureToPtr(request, requestPtr, false);

            if (!DeviceIoControl(handle, (uint)DriverCodes.Read, requestPtr, (uint)Marshal.SizeOf(request), requestPtr, (uint)Marshal.SizeOf(request), out _, IntPtr.Zero))
            {
                Console.WriteLine($"Error al leer bytes en {address.ToString("X")}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
            }

            Marshal.Copy(bufferPtr, bytes, 0, size);

            Marshal.FreeHGlobal(bufferPtr);
            Marshal.FreeHGlobal(requestPtr);

            return bytes;
        }

        /// <summary>
        /// Escribe un valor genérico (struct) en la memoria del proceso vinculado.
        /// </summary>
        public void WriteMemory<T>(IntPtr address, T value) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr bufferPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, bufferPtr, false);

            var request = new Request
            {
                target = address,
                buffer = bufferPtr,
                size = (UIntPtr)size // CORRECCIÓN: Cast a UIntPtr
            };

            IntPtr requestPtr = Marshal.AllocHGlobal(Marshal.SizeOf(request));
            Marshal.StructureToPtr(request, requestPtr, false);

            if (!DeviceIoControl(handle, (uint)DriverCodes.Write, requestPtr, (uint)Marshal.SizeOf(request), requestPtr, (uint)Marshal.SizeOf(request), out _, IntPtr.Zero))
            {
                Console.WriteLine($"Error al escribir en memoria en {address.ToString("X")}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
            }

            Marshal.FreeHGlobal(bufferPtr);
            Marshal.FreeHGlobal(requestPtr);
        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero && handle != new IntPtr(-1))
            {
                CloseHandle(handle);
                handle = IntPtr.Zero;
            }
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Funciones de utilidad para interactuar con procesos y módulos.
    /// </summary>
    public static class ProcessUtils
    {
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MODULEENTRY32
        {
            internal uint dwSize;
            internal uint th32ModuleID;
            internal uint th32ProcessID;
            internal uint GlblcntUsage;
            internal uint ProccntUsage;
            internal IntPtr modBaseAddr;
            internal uint modBaseSize;
            internal IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string szExePath;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        public static int GetProcessIdByName(string processName)
        {
            var process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (process == null)
            {
                throw new Exception($"Proceso '{processName}' no encontrado.");
            }
            return process.Id;
        }

        public static IntPtr GetModuleBaseAddress(int processId, string moduleName)
        {
            IntPtr baseAddress = IntPtr.Zero;
            IntPtr snapShot = CreateToolhelp32Snapshot(0x8, (uint)processId); // TH32CS_SNAPMODULE

            if (snapShot == INVALID_HANDLE_VALUE) return IntPtr.Zero;

            MODULEENTRY32 me32 = new MODULEENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32)) };

            if (Module32First(snapShot, ref me32))
            {
                do
                {
                    if (me32.szModule.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                    {
                        baseAddress = me32.modBaseAddr;
                        break;
                    }
                } while (Module32Next(snapShot, ref me32));
            }

            CloseHandle(snapShot);
            return baseAddress;
        }
    }
}