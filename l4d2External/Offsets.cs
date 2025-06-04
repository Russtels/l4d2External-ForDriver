using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace l4d2External
{
    internal class Offsets
    {
        public int engineAngles = 0x04268EC;
        public int engineAnglesOffset = 0x4AAC;

        public int localplayer = 0x726BD8;
        public int entityList = 0x73A574 + 0x10;


        public int Health = 0xEC;
        public int Lifestate = 0x144;
        public int JumpFlag = 0xF0;
        public int ViewOffset = 0xF4;
        public int Origin = 0x124;
        public int TeamNum = 0xE4;

    }
}
