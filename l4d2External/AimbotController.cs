// l4d2External/AimbotController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using l4d2External;

namespace left4dead2Menu
{
    internal class AimbotController
    {
        private static readonly HashSet<string> SpecialInfectedNames = new HashSet<string>
        {
            "Smoker", "Hunter", "Jockey", "Boomer", "Spitter", "Charger"
        };

        private static readonly HashSet<string> BossNames = new HashSet<string>
        {
            "Tank", "Witch"
        };

        public AimbotController() { }

        public Entity? FindBestTarget(Entity localPlayer, List<Entity> potentialTargets, bool isAimbotAreaEnabled, float aimbotAreaRadius, float fovVisualRadius, Renderer renderer, float screenWidth, float screenHeight)
        {
            // <<< CORRECCIÓN APLICADA AQUÍ >>>
            // Filtramos las entidades usando la misma lógica del TriggerBot.
            // Solo se considerarán objetivos si tienen un esqueleto válido.
            var validTargets = potentialTargets.Where(t => t != null && ESP.IsSkeletonComplete(t)).ToList();

            if (!validTargets.Any())
            {
                return null;
            }

            // --- Lógica de Prioridad ---
            var specialTargets = validTargets.Where(t => t.SimpleName != null && SpecialInfectedNames.Contains(t.SimpleName)).ToList();
            var bossTargets = validTargets.Where(t => t.SimpleName != null && BossNames.Contains(t.SimpleName)).ToList();
            var commonTargets = validTargets.Where(t => t.SimpleName == "Común").ToList();
            var survivorTargets = validTargets.Where(t => t.SimpleName == "Superviviente").ToList();

            // Prioridad 1: Infectados Especiales
            Entity? bestTarget = FindBestTargetInCategory(specialTargets, isAimbotAreaEnabled, aimbotAreaRadius, fovVisualRadius, renderer, screenWidth, screenHeight);
            if (bestTarget != null) return bestTarget;

            // Prioridad 2: Jefes
            bestTarget = FindBestTargetInCategory(bossTargets, isAimbotAreaEnabled, aimbotAreaRadius, fovVisualRadius, renderer, screenWidth, screenHeight);
            if (bestTarget != null) return bestTarget;

            // Prioridad 3: Comunes
            bestTarget = FindBestTargetInCategory(commonTargets, isAimbotAreaEnabled, aimbotAreaRadius, fovVisualRadius, renderer, screenWidth, screenHeight);
            if (bestTarget != null) return bestTarget;

            // Prioridad 4: Supervivientes
            bestTarget = FindBestTargetInCategory(survivorTargets, isAimbotAreaEnabled, aimbotAreaRadius, fovVisualRadius, renderer, screenWidth, screenHeight);
            if (bestTarget != null) return bestTarget;

            return null;
        }

        private Entity? FindBestTargetInCategory(List<Entity> targets, bool isAimbotAreaEnabled, float aimbotAreaRadius, float fovVisualRadius, Renderer renderer, float screenWidth, float screenHeight)
        {
            if (!targets.Any())
            {
                return null;
            }

            List<Entity> validTargetsInFov;
            if (isAimbotAreaEnabled)
            {
                validTargetsInFov = targets.Where(t => t.magnitude <= aimbotAreaRadius).ToList();
            }
            else
            {
                Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
                validTargetsInFov = targets.Where(t =>
                {
                    Vector3? aimPos = GetTargetPosition(t, AimbotTarget.Head); // Se sigue usando la cabeza para el FOV
                    if (aimPos.HasValue && renderer.WorldToScreen(aimPos.Value, out Vector2 screenPos, screenWidth, screenHeight))
                    {
                        return Vector2.Distance(screenCenter, screenPos) <= fovVisualRadius;
                    }
                    return false;
                }).ToList();
            }

            return validTargetsInFov.Any() ? validTargetsInFov.OrderBy(t => t.magnitude).First() : null;
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
            if (target.BonePositions == null || target.SimpleName == null) return null;

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

            return null;
        }
    }
}