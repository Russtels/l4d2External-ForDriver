using System;
using System.Collections.Generic;
using System.Numerics;
using Swed32;
using l4d2External; // Para Entity y Offsets

namespace left4dead2Menu
{
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

        public void PerformAimbotActions(
            Entity localPlayer,
            List<Entity> potentialTargets,
            float fovVisualRadius, // Usado como umbral de distancia angular en el original
            float targetZOffset,
            float smoothness)
        {
            if (localPlayer.address == IntPtr.Zero || potentialTargets.Count == 0) return;

            Entity bestTarget = null;
            // El original usaba fovCircleVisualRadius / 2.0f como umbral para la distancia angular.
            // Esto no es un FOV angular real en grados, sino un valor derivado de píxeles. Se mantiene la lógica.
            float closestAngleDistanceThreshold = fovVisualRadius / 2.0f;

            var engineAnglesPtrBase = swed.ReadPointer(engineModuleBase, offsets.engineAngles);
            Vector3 currentViewAngles = new Vector3(
                swed.ReadFloat(engineAnglesPtrBase, offsets.engineAnglesOffset),       // Pitch
                swed.ReadFloat(engineAnglesPtrBase, offsets.engineAnglesOffset + 0x4), // Yaw
                0
            );

            foreach (Entity targetEntity in potentialTargets)
            {
                if (targetEntity.address == IntPtr.Zero || targetEntity.health <= 0 || targetEntity.lifeState != GameConstants.LIFE_STATE_ALIVE)
                    continue;

                // Usar la propiedad 'abs' de la entidad que ya es origin + viewOffset
                Vector3 aimPositionBase = targetEntity.abs;
                Vector3 aimPositionWithOffset = new Vector3(aimPositionBase.X, aimPositionBase.Y, aimPositionBase.Z - targetZOffset);

                // Usar la propiedad 'abs' del jugador local que ya es su eye position
                Vector3 targetAngles = CalculateAimAngles(localPlayer.abs, aimPositionWithOffset);

                float deltaYaw = MathUtils.NormalizeAngle(targetAngles.Y - currentViewAngles.Y);
                float deltaPitch = MathUtils.NormalizeAngle(targetAngles.X - currentViewAngles.X);
                float angleDistance = (float)Math.Sqrt(deltaYaw * deltaYaw + deltaPitch * deltaPitch);

                if (angleDistance < closestAngleDistanceThreshold)
                {
                    closestAngleDistanceThreshold = angleDistance;
                    bestTarget = targetEntity;
                }
            }

            if (bestTarget != null)
            {
                Vector3 finalAimPositionBase = bestTarget.abs;
                Vector3 finalAimPositionWithOffset = new Vector3(finalAimPositionBase.X, finalAimPositionBase.Y, finalAimPositionBase.Z - targetZOffset);

                Vector3 targetViewAngles = CalculateAimAngles(localPlayer.abs, finalAimPositionWithOffset);

                Vector3 smoothedAngles = new Vector3
                {
                    X = MathUtils.Lerp(currentViewAngles.X, targetViewAngles.X, smoothness),
                    Y = MathUtils.Lerp(currentViewAngles.Y, targetViewAngles.Y, smoothness),
                    Z = 0 // Pitch y Yaw solamente
                };

                SetAimAngles(smoothedAngles, engineAnglesPtrBase);
            }
        }

        private void SetAimAngles(Vector3 angles, IntPtr engineAnglesPtrBase) // angles.X = pitch, angles.Y = yaw
        {
            swed.WriteFloat(engineAnglesPtrBase, offsets.engineAnglesOffset, angles.X);     // Pitch
            swed.WriteFloat(engineAnglesPtrBase, offsets.engineAnglesOffset + 0x4, angles.Y); // Yaw
        }

        private Vector3 CalculateAimAngles(Vector3 sourceEyePosition, Vector3 destinationPoint)
        {
            float deltaX = destinationPoint.X - sourceEyePosition.X;
            float deltaY = destinationPoint.Y - sourceEyePosition.Y;
            float deltaZ = destinationPoint.Z - sourceEyePosition.Z; // Diferencia vertical desde el ojo al punto de mira

            float yaw = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);

            double horizontalDistance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            float pitch = -(float)(Math.Atan2(deltaZ, horizontalDistance) * 180 / Math.PI);

            return new Vector3(pitch, yaw, 0);
        }
    }
}