// l4d2External/AutoLevel.cs
using l4d2External;
using System.Collections.Generic;
using System.Linq;

namespace left4dead2Menu
{
    // Handles the auto-leveling logic for targeting and attacking entities automatically.
    internal class AutoLevel
    {
        /// <summary>
        /// Updates the auto-level logic, finds the best target, and executes the aimbot and click if enabled.
        /// </summary>
        /// <param name="localPlayer">The local player entity.</param>
        /// <param name="potentialTargets">List of potential target entities.</param>
        /// <param name="aimbotController">Aimbot controller instance.</param>
        /// <param name="renderer">Renderer instance for drawing.</param>
        /// <param name="screenWidth">Screen width in pixels.</param>
        /// <param name="screenHeight">Screen height in pixels.</param>
        /// <param name="config">Configuration settings.</param>
        public void Update(
            Entity localPlayer,
            List<Entity> potentialTargets,
            AimbotController aimbotController,
            Renderer renderer,
            float screenWidth,
            float screenHeight,
            Config config)
        {
            if (!config.AutoLevel_IsEnabled || localPlayer.address == System.IntPtr.Zero) return;

            var bestTarget = potentialTargets
                .Where(e => e.health > 0 && e.magnitude <= config.AutoLevel_Radius && IsValidTargetType(e, config))
                .OrderBy(e => e.magnitude)
                .FirstOrDefault();

            if (bestTarget != null)
            {
                aimbotController.ExecuteMouseAimbot(bestTarget, AimbotTarget.Head, 1.0f, renderer, screenWidth, screenHeight);
                NativeMethods.SimulateLeftClick();
            }
        }

        private bool IsValidTargetType(Entity entity, Config config)
        {
            if (entity.SimpleName == null) return false;
            return (config.AutoLevel_OnCommons && entity.SimpleName == "Común");
        }
    }
}