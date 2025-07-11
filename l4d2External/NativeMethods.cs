﻿// NativeMethods.cs (Actualizado para SendInput)
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

        public const int VK_CONTROL = 0x11;
        public const int VK_SPACE = 0x20;

        public const int KEYEVENTF_KEYDOWN = 0x0000;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int KEYEVENTF_SCANCODE = 0x0008;

        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;


        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
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
        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
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
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type; // 0 = mouse, 1 = keyboard, 2 = hardware
            public MOUSEKEYBDHARDWAREINPUT mkhi;
        }
        public static void SimulateMouseMove(int dx, int dy)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mkhi.mi.dx = dx;
            inputs[0].mkhi.mi.dy = dy;
            inputs[0].mkhi.mi.dwFlags = MOUSEEVENTF_MOVE;
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void SimulateRightClick()
        {
            INPUT[] inputDown = new INPUT[1];
            inputDown[0].type = INPUT_MOUSE;
            inputDown[0].mkhi.mi.dwFlags = MOUSEEVENTF_RIGHTDOWN;
            SendInput(1, inputDown, Marshal.SizeOf(typeof(INPUT)));

            Thread.Sleep(50);

            INPUT[] inputUp = new INPUT[1];
            inputUp[0].type = INPUT_MOUSE;
            inputUp[0].mkhi.mi.dwFlags = MOUSEEVENTF_RIGHTUP;
            SendInput(1, inputUp, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void SimulateLeftClick()
        {
            INPUT[] inputDown = new INPUT[1];
            inputDown[0].type = INPUT_MOUSE;
            inputDown[0].mkhi.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
            SendInput(1, inputDown, Marshal.SizeOf(typeof(INPUT)));

            Thread.Sleep(25); // Una pequeña pausa es buena práctica

            INPUT[] inputUp = new INPUT[1];
            inputUp[0].type = INPUT_MOUSE;
            inputUp[0].mkhi.mi.dwFlags = MOUSEEVENTF_LEFTUP;
            SendInput(1, inputUp, Marshal.SizeOf(typeof(INPUT)));
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] pInputs, int cbSize);


        //TEST FOR SCREENSHARE BYPASS(MAYNOT WORK PROPPERTLY)

    }
}