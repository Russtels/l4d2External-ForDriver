// AimbotController.cs (Corregido)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Swed32;
using l4d2External;

namespace left4dead2Menu
{
    internal class AimbotController
    {
        // El constructor ahora no toma argumentos, ya que no escribe en memoria.
        public AimbotController()
        {
        }

        public Entity? FindBestTarget(Entity localPlayer, List<Entity> potentialTargets, AimbotTarget targetSelection, bool isAimbotAreaEnabled, float aimbotAreaRadius, float fovVisualRadius, Renderer renderer, float screenWidth, float screenHeight)
        {
            var liveTargets = potentialTargets.Where(t => t.health > 0).ToList();
            if (liveTargets.Count == 0) return null;

            List<Entity> validTargets;
            if (isAimbotAreaEnabled)
            {
                validTargets = liveTargets.Where(t => t.magnitude <= aimbotAreaRadius).ToList();
            }
            else
            {
                Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
                validTargets = liveTargets.Where(t =>
                {
                    Vector3 aimPos = GetTargetPosition(t, targetSelection);
                    if (renderer.WorldToScreen(aimPos, out Vector2 screenPos, screenWidth, screenHeight))
                    {
                        return Vector2.Distance(screenCenter, screenPos) <= fovVisualRadius;
                    }
                    return false;
                }).ToList();
            }

            if (validTargets.Count > 0)
            {
                return validTargets.OrderBy(t => t.magnitude).First();
            }

            return null;
        }

        public void ExecuteMouseAimbot(Entity target, Entity localPlayer, AimbotTarget targetSelection, float smoothness, Renderer renderer, float screenWidth, float screenHeight)
        {
            Vector3 aimPos = GetTargetPosition(target, targetSelection);
            if (renderer.WorldToScreen(aimPos, out Vector2 targetScreenPos, screenWidth, screenHeight))
            {
                Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
                float deltaX = targetScreenPos.X - screenCenter.X;
                float deltaY = targetScreenPos.Y - screenCenter.Y;

                float moveX = deltaX * smoothness;
                float moveY = deltaY * smoothness;

                NativeMethods.SimulateMouseMove((int)moveX, (int)moveY);
            }
        }

        private Vector3 GetTargetPosition(Entity target, AimbotTarget targetSelection)
        {
            if (targetSelection == AimbotTarget.Chest)
            {
                return new Vector3(target.origin.X, target.origin.Y, target.origin.Z + (target.viewOffset.Z * 0.7f));
            }
            return target.abs;
        }
    }
}