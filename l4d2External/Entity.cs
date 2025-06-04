using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace l4d2External
{
    internal class Entity
    {
        
        public IntPtr address { get; set; }

        public string name { get; set; }
        public int health { get; set; }
        public int teamNum { get; set; }
        public int lifeState { get; set; }
        public int jumpflag { get; set; } 
        public Vector3 origin { get; set; }
        public Vector3 viewOffset { get; set; }
        public Vector3 abs { get; set; }
        public float magnitude { get; set; }


        
    }

}
