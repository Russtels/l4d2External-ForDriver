// l4d2External/AutoLevel.cs
using l4d2External;
using System.Collections.Generic;
using System.Linq;

namespace left4dead2Menu
{
    internal class AutoLevel
    {
        private readonly List<Entity> levelTargets = new List<Entity>();

        public void Update(List<Entity> commonInfected, List<Entity> specialInfected, List<Entity> bossInfected, Config config)
        {
            if (!config.EnableAutoLevel)
            {
                return;
            }

            levelTargets.Clear();

            // =========================================================
            // <<< LÓGICA DE FILTRADO DETALLADA IMPLEMENTADA >>>
            // =========================================================
            if (config.LevelOnCommons)
            {
                levelTargets.AddRange(commonInfected);
            }

            if (config.LevelOnBosses)
            {
                levelTargets.AddRange(bossInfected);
            }

            // Filtra los especiales uno por uno según la configuración.
            var specialLevelTargets = specialInfected.Where(s =>
                (config.LevelOnHunter && s.SimpleName == "Hunter") ||
                (config.LevelOnSmoker && s.SimpleName == "Smoker") ||
                (config.LevelOnBoomer && s.SimpleName == "Boomer") ||
                (config.LevelOnJockey && s.SimpleName == "Jockey") ||
                (config.LevelOnSpitter && s.SimpleName == "Spitter") ||
                (config.LevelOnCharger && s.SimpleName == "Charger")
            ).ToList();
            levelTargets.AddRange(specialLevelTargets);

            // Comprueba si algún objetivo válido está dentro del radio.
            bool enemyInLevelArea = levelTargets.Any(e =>
                e.magnitude <= config.LevelRadius &&
                e.BonePositions != null &&
                e.BonePositions.Length > 0
            );

            if (enemyInLevelArea)
            {
                NativeMethods.SimulateLeftClick();
            }
        }
    }
}