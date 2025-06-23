﻿// BunnyHop.cs (Versión Final Sincronizada)
using System;
using System.Threading;
using l4d2External;

namespace left4dead2Menu
{
    internal class BunnyHop
    {
        private readonly GameMemory memory;
        private readonly Offsets offsets;
        private readonly Random random = new Random();

        private const int JUMP_KEY = 0x05;
        private const ushort SCANCODE_SPACE = 0x39;
        private const int STANDING = 129;
        private const int DUCKING = 131;

        public BunnyHop(GameMemory memory, Offsets offsets) // Cambiado de Swed a GameMemory
        {
            this.memory = memory;
            this.offsets = offsets;
        }

        // Firma del método corregida, sin 'failureChance'
        public void Update(IntPtr localPlayerAddress, bool isEnabled)
        {
            if (!isEnabled) return;

            if (NativeMethods.GetAsyncKeyState(JUMP_KEY) < 0)
            {
                int jumpFlag = memory.ReadInt(localPlayerAddress, offsets.JumpFlag);
                bool onGround = (jumpFlag == STANDING || jumpFlag == DUCKING);

                if (onGround)
                {
                    Thread.Sleep(random.Next(5, 20));
                    SimulateJump();
                }
            }
        }

        private void SimulateJump()
        {
            NativeMethods.INPUT[] inputs = new NativeMethods.INPUT[2];

            inputs[0].type = 1;
            inputs[0].mkhi.ki.wScan = SCANCODE_SPACE;
            inputs[0].mkhi.ki.dwFlags = NativeMethods.KEYEVENTF_SCANCODE;

            inputs[1].type = 1;
            inputs[1].mkhi.ki.wScan = SCANCODE_SPACE;
            inputs[1].mkhi.ki.dwFlags = NativeMethods.KEYEVENTF_SCANCODE | NativeMethods.KEYEVENTF_KEYUP;

            NativeMethods.SendInput(2, inputs, System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }
    }
}