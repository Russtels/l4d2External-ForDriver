// l4d2External/AutoShove.cs
using l4d2External;
using System.Collections.Generic;
using System.Linq;

namespace left4dead2Menu
{
    internal class AutoShove
    {
        private readonly List<Entity> meleeTargets = new List<Entity>();
        private bool hasPerformedShove = false;

        public void Update(List<Entity> commonInfected, List<Entity> specialInfected, Config config)
        {
            if (!config.EnableAutoShove)
            {
                return;
            }

            meleeTargets.Clear();

            if (config.ShoveOnCommons)
            {
                meleeTargets.AddRange(commonInfected);
            }

            var specialMeleeTargets = specialInfected.Where(s =>
                (config.ShoveOnHunter && s.SimpleName == "Hunter") ||
                (config.ShoveOnSmoker && s.SimpleName == "Smoker") ||
                (config.ShoveOnBoomer && s.SimpleName == "Boomer") ||
                (config.ShoveOnJockey && s.SimpleName == "Jockey") ||
                (config.ShoveOnSpitter && s.SimpleName == "Spitter") ||
                (config.ShoveOnCharger && s.SimpleName == "Charger")
            ).ToList();
            meleeTargets.AddRange(specialMeleeTargets);

            // =======================================================================
            // <<< FILTRO DEL AIMBOT IMPLEMENTADO AQUÍ >>>
            // Ahora se usa ESP.IsSkeletonComplete() para una máxima consistencia.
            // =======================================================================
            bool enemyInMeleeRange = meleeTargets.Any(e =>
                e.magnitude <= config.ShoveRadius &&
                ESP.IsSkeletonComplete(e)
            );

            if (enemyInMeleeRange && !hasPerformedShove)
            {
                NativeMethods.SimulateRightClick();
                hasPerformedShove = true;
            }
            else if (!enemyInMeleeRange)
            {
                hasPerformedShove = false;
            }
        }
    }
}