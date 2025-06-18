// NativeMethods.cs (Actualizado para SendInput)
using System;
using System.Runtime.InteropServices;

namespace left4dead2Menu
{
    internal static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // --- ESTRUCTURAS Y MÉTODOS PARA SENDINPUT ---

        // Constantes para los eventos de teclado
        public const int INPUT_KEYBOARD = 1;
        public const int INPUT_MOUSE = 0;

        public const int KEYEVENTF_KEYDOWN = 0x0000;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int KEYEVENTF_SCANCODE = 0x0008;

        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;

        // Estructura para la entrada de teclado
        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Estructura para la entrada de ratón (necesaria para el layout de INPUT)
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Estructura para la entrada de hardware (necesaria para el layout de INPUT)
        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        // Unión para los diferentes tipos de entrada
        [StructLayout(LayoutKind.Explicit)]
        public struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        // Estructura principal para SendInput
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type; // 0 = mouse, 1 = keyboard, 2 = hardware
            public MOUSEKEYBDHARDWAREINPUT mkhi;
        }

        public static void SimulateRightClick()
        {
            INPUT[] inputs = new INPUT[2];

            // Presionar botón derecho
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mkhi.mi.dwFlags = MOUSEEVENTF_RIGHTDOWN;

            // Soltar botón derecho
            inputs[1].type = INPUT_MOUSE;
            inputs[1].mkhi.mi.dwFlags = MOUSEEVENTF_RIGHTUP;

            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        // Firma de la función SendInput
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] pInputs, int cbSize);
    }
}