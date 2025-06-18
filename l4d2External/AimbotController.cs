// AimbotController.cs (Modificado)
using System;
using System.Collections.Generic;
using System.Numerics;
using Swed32;
using l4d2External;

namespace left4dead2Menu
{
    // --- ENUM PARA DEFINIR LA ZONA DE APUNTADO ---
    public enum AimbotTarget
    {
        Head,
        Chest
    }

    internal class AimbotController
    {
        private readonly Swed swed;
        private readonly IntPtr engineModuleBase;
        private readonly Offsets offsets;

        public AimbotController(Swed swed, IntPtr engineModuleBase, Offsets offsets)
        {
            this.swed = swed;
            this.engineModuleBase = engineModuleBase;
            this.offsets = offsets;
        }

        // --- NUEVO MÉTODO PARA OBTENER LA POSICIÓN DE APUNTADO ---
        private Vector3 GetTargetPosition(Entity target, AimbotTarget targetSelection)
        {
            if (targetSelection == AimbotTarget.Chest)
            {
                // Calcula una posición en el torso, usando un 70% de la altura del viewOffset.
                // Es más fiable que 'abs' para el centro de masa.
                return new Vector3(target.origin.X, target.origin.Y, target.origin.Z + (target.viewOffset.Z * 0.7f));
            }
            // Por defecto, o si se elige cabeza, se usa 'abs' (posición de los ojos).
            return target.abs;
        }

        public void PerformAimbotActions(
            Entity localPlayer, List<Entity> potentialTargets,
            float fovVisualRadius, float targetZOffset, float smoothness, AimbotTarget targetSelection,
            // Nuevos parámetros
            bool isAimbotAreaEnabled, float aimbotAreaRadius)
        {
            if (localPlayer.address == IntPtr.Zero || potentialTargets.Count == 0) return;

            var engineAnglesPtrBase = swed.ReadPointer(engineModuleBase, offsets.engineAngles);
            Vector3 currentViewAngles = new Vector3(
                swed.ReadFloat(engineAnglesPtrBase, offsets.engineAnglesOffset),
                swed.ReadFloat(engineAnglesPtrBase, offsets.engineAnglesOffset + 0x4), 0);

            // 1. FILTRAR OBJETIVOS VÁLIDOS
            List<Entity> validTargets;
            if (isAimbotAreaEnabled)
            {
                // Modo Área: filtra por distancia/radio
                validTargets = potentialTargets.Where(t => t.magnitude <= aimbotAreaRadius).ToList();
            }
            else
            {
                // Modo FoV: filtra por ángulo de visión
                validTargets = new List<Entity>();
                foreach (var target in potentialTargets)
                {
                    Vector3 aimPositionBase = GetTargetPosition(target, targetSelection);
                    Vector3 targetAngles = CalculateAimAngles(localPlayer.abs, aimPositionBase);
                    float deltaYaw = MathUtils.NormalizeAngle(targetAngles.Y - currentViewAngles.Y);
                    float deltaPitch = MathUtils.NormalizeAngle(targetAngles.X - currentViewAngles.X);
                    float angleDistance = (float)Math.Sqrt(deltaYaw * deltaYaw + deltaPitch * deltaPitch);

                    if (angleDistance < fovVisualRadius / 2.0f)
                    {
                        validTargets.Add(target);
                    }
                }
            }

            // 2. APUNTAR SI HAY OBJETIVOS VÁLIDOS
            if (validTargets.Count > 0)
            {
                // Ordenar por la distancia más cercana
                validTargets.Sort((a, b) => a.magnitude.CompareTo(b.magnitude));
                Entity bestTarget = validTargets.First();

                Vector3 finalAimPositionBase = GetTargetPosition(bestTarget, targetSelection);
                Vector3 finalAimPositionWithOffset = new Vector3(finalAimPositionBase.X, finalAimPositionBase.Y, finalAimPositionBase.Z - targetZOffset);
                Vector3 targetViewAngles = CalculateAimAngles(localPlayer.abs, finalAimPositionWithOffset);

                Vector3 smoothedAngles = new Vector3
                {
                    X = MathUtils.Lerp(currentViewAngles.X, targetViewAngles.X, smoothness),
                    Y = MathUtils.Lerp(currentViewAngles.Y, targetViewAngles.Y, smoothness),
                    Z = 0
                };
                SetAimAngles(smoothedAngles, engineAnglesPtrBase);
            }
        }

        private void SetAimAngles(Vector3 angles, IntPtr engineAnglesPtrBase)
        {
            swed.WriteFloat(engineAnglesPtrBase, offsets.engineAnglesOffset, angles.X);
            swed.WriteFloat(engineAnglesPtrBase, offsets.engineAnglesOffset + 0x4, angles.Y);
        }

        private Vector3 CalculateAimAngles(Vector3 sourceEyePosition, Vector3 destinationPoint)
        {
            float deltaX = destinationPoint.X - sourceEyePosition.X;
            float deltaY = destinationPoint.Y - sourceEyePosition.Y;
            float deltaZ = destinationPoint.Z - sourceEyePosition.Z;
            float yaw = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);
            double horizontalDistance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            float pitch = -(float)(Math.Atan2(deltaZ, horizontalDistance) * 180 / Math.PI);
            return new Vector3(pitch, yaw, 0);
        }
    }
}