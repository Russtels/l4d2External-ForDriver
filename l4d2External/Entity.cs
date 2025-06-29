// l4d2External/Entity.cs (MODIFICADO)
using System;
using System.Numerics;

namespace l4d2External
{
    internal class Entity
    {
        public IntPtr address { get; set; }
        public int health { get; set; }
        public int maxHealth { get; set; } // <-- NUEVA PROPIEDAD
        public int teamNum { get; set; }
        public int lifeState { get; set; }
        public int jumpflag { get; set; }
        public Vector3 origin { get; set; }
        public Vector3 viewOffset { get; set; }
        public Vector3 abs { get; set; }
        public float magnitude { get; set; }
        public string? SimpleName { get; set; }
        public string? modelName { get; set; }
        public int TeamNum { get; set; }
        public Vector3[]? BonePositions { get; set; }

        public Entity()
        {
            address = IntPtr.Zero;
            origin = Vector3.Zero;
            abs = Vector3.Zero;
            viewOffset = Vector3.Zero;
            maxHealth = 6000;// Valor por defecto
        }
    }
}