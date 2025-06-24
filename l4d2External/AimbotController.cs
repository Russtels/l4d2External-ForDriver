// l4d2External/AimbotController.cs (Actualizado para apuntar a huesos específicos)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using l4d2External;

namespace left4dead2Menu
{
    internal class AimbotController
    {
        public AimbotController() { }

        public Entity? FindBestTarget(Entity localPlayer, List<Entity> potentialTargets, bool isAimbotAreaEnabled, float aimbotAreaRadius, float fovVisualRadius, Renderer renderer, float screenWidth, float screenHeight)
        {
            var liveTargets = potentialTargets.Where(t => t.lifeState > 0 || t.lifeState <100).ToList();
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
                    // Usamos la cabeza para el check de FOV
                    Vector3? aimPos = GetTargetPosition(t, AimbotTarget.Head);
                    if (aimPos.HasValue && renderer.WorldToScreen(aimPos.Value, out Vector2 screenPos, screenWidth, screenHeight))
                    {
                        return Vector2.Distance(screenCenter, screenPos) <= fovVisualRadius;
                    }
                    return false;
                }).ToList();
            }

            return validTargets.Count > 0 ? validTargets.OrderBy(t => t.magnitude).First() : null;
        }

        public void ExecuteMouseAimbot(Entity target, AimbotTarget targetSelection, float smoothness, Renderer renderer, float screenWidth, float screenHeight)
        {
            Vector3? aimPos = GetTargetPosition(target, targetSelection);
            if (aimPos.HasValue && renderer.WorldToScreen(aimPos.Value, out Vector2 targetScreenPos, screenWidth, screenHeight))
            {
                Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
                float deltaX = targetScreenPos.X - screenCenter.X;
                float deltaY = targetScreenPos.Y - screenCenter.Y;

                float moveX = deltaX * smoothness;
                float moveY = deltaY * smoothness;

                NativeMethods.SimulateMouseMove((int)moveX, (int)moveY);
            }
        }

        private Vector3? GetTargetPosition(Entity target, AimbotTarget targetSelection)
        {
            if (target.BonePositions == null) return null;

            int boneIndex = -1;
            if (targetSelection == AimbotTarget.Head)
            {
                boneIndex = ESP.GetHeadBoneIndex(target.SimpleName);
            }
            else // AimbotTarget.Chest
            {
                boneIndex = ESP.GetChestBoneIndex(target.SimpleName);
            }

            if (boneIndex != -1 && boneIndex < target.BonePositions.Length)
            {
                return target.BonePositions[boneIndex];
            }

            return null; // Devuelve nulo si no se pudo encontrar el hueso
        }
    }
}