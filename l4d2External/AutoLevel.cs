// l4d2External/AutoLevel.cs
using l4d2External;
using left4dead2Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace left4dead2Menu
{
    internal class AutoLevel
    {
        private readonly List<Entity> levelTargets = new List<Entity>();
        private const float ChargerLevelAngle = 17f;

        public void Update(List<Entity> commonInfected, List<Entity> specialInfected, List<Entity> bossInfected, Config config, GameMemory memory, IntPtr engineModule, Offsets offsets)
        {
            IntPtr viewAnglesAddress = memory.ReadPointer(engineModule, offsets.engineAngles);

            if (!config.EnableAutoLevel)
            {
                return;
            }

            levelTargets.Clear();

            // --- Paso 1: Se mantiene la selección de objetivos de la GUI ---
            if (config.LevelOnCommons)
            {
                levelTargets.AddRange(commonInfected);
            }
            if (config.LevelOnBosses)
            {
                levelTargets.AddRange(bossInfected);
            }

            var specialLevelTargets = specialInfected.Where(s =>
                (config.LevelOnHunter && s.SimpleName == "Hunter") ||
                (config.LevelOnSmoker && s.SimpleName == "Smoker") ||
                (config.LevelOnBoomer && s.SimpleName == "Boomer") ||
                (config.LevelOnJockey && s.SimpleName == "Jockey") ||
                (config.LevelOnSpitter && s.SimpleName == "Spitter") ||
                (config.LevelOnCharger && s.SimpleName == "Charger")
            ).ToList();
            levelTargets.AddRange(specialLevelTargets);

            // --- Paso 2: Se recorren los objetivos seleccionados para aplicar los filtros ---
            foreach (var entity in levelTargets)
            {
                // Filtro 2.1: ¿Está el objetivo en el radio y tiene huesos válidos?
                bool isInArea = entity.magnitude <= config.LevelRadius &&
                                entity.BonePositions != null &&
                                entity.BonePositions.Length > 0;

                if (isInArea)
                {
                    // --- Paso 3: FILTRO ADICIONAL ---
                    // ¿Es la entidad detectada específicamente un "Charger"?
                    if (entity.SimpleName == "Charger")
                    {
                        // Si se cumplen todas las condiciones, se ejecuta la acción y se sale.
                        NativeMethods.SimulateLeftClick();
                        memory.WriteFloat(viewAnglesAddress + offsets.engineAnglesOffset, ChargerLevelAngle);
                        return;
                    }
                    else
                    {
                        // Si no es un "Charger", se simula un clic izquierdo.
                        NativeMethods.SimulateLeftClick();
                    }
                }
            }
        }
    }
}

