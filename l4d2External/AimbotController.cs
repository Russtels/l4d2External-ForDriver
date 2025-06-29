// l4d2External/AimbotController.cs (MODIFICADO CON PRIORIDADES)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using l4d2External;

namespace left4dead2Menu
{
    internal class AimbotController
    {
        // Conjuntos de nombres para facilitar la categorización
        private static readonly HashSet<string> SpecialInfectedNames = new HashSet<string>
        {
            "Smoker", "Hunter", "Jockey", "Boomer", "Spitter", "Charger"
        };

        private static readonly HashSet<string> BossNames = new HashSet<string>
        {
            "Tank", "Witch"
        };

        public AimbotController() { }

        // --- MÉTODO PRINCIPAL MODIFICADO CON LA NUEVA LÓGICA DE PRIORIDAD ---
        public Entity? FindBestTarget(Entity localPlayer, List<Entity> potentialTargets, bool isAimbotAreaEnabled, float aimbotAreaRadius, float fovVisualRadius, Renderer renderer, float screenWidth, float screenHeight)
        {
            // <<< FILTRO DE LIFESTATE ORIGINAL - SIN CAMBIOS >>>
            var liveTargets = potentialTargets.Where(t => t.lifeState > 0 || t.lifeState < 100).ToList();
            if (!liveTargets.Any())
            {
                return null;
            }

            // --- Lógica de Prioridad ---

            // 1. Categorizar los objetivos vivos que pasaron el filtro inicial
            var specialTargets = liveTargets.Where(t => t.SimpleName != null && SpecialInfectedNames.Contains(t.SimpleName)).ToList();
            var bossTargets = liveTargets.Where(t => t.SimpleName != null && BossNames.Contains(t.SimpleName)).ToList();
            var commonTargets = liveTargets.Where(t => t.SimpleName == "Común").ToList();
            var survivorTargets = liveTargets.Where(t => t.SimpleName == "Superviviente").ToList();

            // 2. Aplicar la lógica de búsqueda en orden de prioridad
            // (El sistema ya se encarga de que solo las categorías activadas en la GUI estén en `potentialTargets`)

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

            // Si no se encuentra ningún objetivo válido en ninguna categoría prioritaria
            return null;
        }

        // Método de ayuda para encontrar el mejor objetivo dentro de una categoría (sin cambios)
        private Entity? FindBestTargetInCategory(List<Entity> targets, bool isAimbotAreaEnabled, float aimbotAreaRadius, float fovVisualRadius, Renderer renderer, float screenWidth, float screenHeight)
        {
            if (!targets.Any())
            {
                return null;
            }

            List<Entity> validTargets;
            if (isAimbotAreaEnabled)
            {
                validTargets = targets.Where(t => t.magnitude <= aimbotAreaRadius).ToList();
            }
            else
            {
                Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
                validTargets = targets.Where(t =>
                {
                    Vector3? aimPos = GetTargetPosition(t, AimbotTarget.Head);
                    if (aimPos.HasValue && renderer.WorldToScreen(aimPos.Value, out Vector2 screenPos, screenWidth, screenHeight))
                    {
                        return Vector2.Distance(screenCenter, screenPos) <= fovVisualRadius;
                    }
                    return false;
                }).ToList();
            }

            return validTargets.Any() ? validTargets.OrderBy(t => t.magnitude).First() : null;
        }


        // --- MÉTODOS RESTANTES (SIN CAMBIOS) ---

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