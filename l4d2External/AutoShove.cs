// l4d2External/AutoShove.cs
using l4d2External;
using System.Collections.Generic;
using System.Linq;

namespace left4dead2Menu
{
    internal class AutoShove
    {
        private bool hasShoved = false;

        public void Update(List<Entity> potentialTargets, Config config)
        {
            if (!config.AutoShove_IsEnabled)
            {
                hasShoved = false;
                return;
            }

            var validTargets = potentialTargets.Where(e =>
                e.health > 0 && e.magnitude <= config.AutoShove_Radius && IsValidTargetType(e, config)
            ).ToList();

            if (validTargets.Any())
            {
                if (!hasShoved)
                {
                    NativeMethods.SimulateRightClick();
                    hasShoved = true;
                }
            }
            else
            {
                hasShoved = false;
            }
        }

        private bool IsValidTargetType(Entity entity, Config config)
        {
            if (entity.SimpleName == null) return false;
            return (config.AutoShove_OnCommons && entity.SimpleName == "Común") ||
                   (config.AutoShove_OnHunter && entity.SimpleName == "Hunter") ||
                   (config.AutoShove_OnSmoker && entity.SimpleName == "Smoker") ||
                   (config.AutoShove_OnBoomer && entity.SimpleName == "Boomer") ||
                   (config.AutoShove_OnJockey && entity.SimpleName == "Jockey") ||
                   (config.AutoShove_OnSpitter && entity.SimpleName == "Spitter") ||
                   (config.AutoShove_OnCharger && entity.SimpleName == "Charger");
        }
    }
}