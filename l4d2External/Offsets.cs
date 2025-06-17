// Offsets.cs (Actualizado)
using System;

namespace l4d2External
{
    internal class Offsets
    {
        // --- OFFSETS DE VIEW MATRIX ACTUALIZADOS ---
        public int ViewMatrix = 0x601FDC;
        public int ViewMatrixOffset = 0x2E4;

        // Offsets del motor
        public int engineAngles = 0x4268EC;
        public int engineAnglesOffset = 0x4AAC;

        // Offsets de cliente
        public int localplayer = 0x726BD8;
        public int entityList = 0x73A574 + 0x10;

        // Offsets de entidad
        public int Health = 0xEC;
        public int Lifestate = 0x144;
        public int JumpFlag = 0xF0;
        public int ViewOffset = 0xF4;
        public int Origin = 0x124;
        public int TeamNum = 0xE4;
        public int ModelName = 0x60;
    }
}