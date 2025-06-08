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
        public int ModelName = 0x60; // Nota: El código original usa 0x10 para leer el nombre del modelo.
                                     // Esta variable ModelName = 0x60 no se usa para leer la propiedad `Entity.modelName`.


        // Sugerencias para añadir basadas en el código original:
        // public int MaxEntities = 900;
        // public int EntityLoopDistance = 0x10;
        // public int EntityNamePointerOffset = 0x10; // Usado en Program.cs para leer modelName
        // public int EntityNameStringLength = 10;    // Usado en Program.cs para leer modelName
    }
}