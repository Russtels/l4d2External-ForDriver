// l4d2External/Entity.cs

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
        public int health { get; set; }
        public int teamNum { get; set; }
        public int lifeState { get; set; }
        public int jumpflag { get; set; }
        public Vector3 origin { get; set; }
        public Vector3 viewOffset { get; set; }
        public Vector3 abs { get; set; }
        public float magnitude { get; set; }
        public string? SimpleName { get; set; }
        public string? modelName { get; set; }

        // <<< NUEVA PROPIEDAD >>>
        // Almacenará las coordenadas 3D de cada hueso.
        public Vector3[]? BonePositions { get; set; }

        public Entity()
        {
            address = IntPtr.Zero;
            origin = Vector3.Zero;
            abs = Vector3.Zero;
            viewOffset = Vector3.Zero;
        }
    }
}