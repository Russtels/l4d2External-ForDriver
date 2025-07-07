// l4d2External/TriggerBot.cs
using l4d2External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace left4dead2Menu
{
    internal class TriggerBot
    {
        // Propiedades para la configuración desde la GUI
        public bool IsEnabled { get; set; } = false;
        public float TriggerRadius { get; set; } = 5.0f;
        public bool TriggerOnBosses { get; set; } = true;
        public bool TriggerOnSpecials { get; set; } = true;
        public bool TriggerOnCommons { get; set; } = false;
        public bool TriggerOnSurvivors { get; set; } = false;

        /// <summary>
        /// Calcula la distancia mínima desde un punto hasta un segmento de línea en 2D.
        /// </summary>
        private float DistancePointToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            float lengthSq = Vector2.DistanceSquared(lineStart, lineEnd);
            if (lengthSq == 0.0f) return Vector2.Distance(point, lineStart);

            float t = Math.Max(0, Math.Min(1, Vector2.Dot(point - lineStart, lineEnd - lineStart) / lengthSq));
            Vector2 projection = lineStart + t * (lineEnd - lineStart);
            return Vector2.Distance(point, projection);
        }

        public void Update(
            Renderer renderer,
            float screenWidth,
            float screenHeight,
            IEnumerable<Entity> allEntities)
        {
            if (!IsEnabled)
            {
                return;
            }

            Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
            bool targetFoundInFrame = false;

            var potentialTargets = allEntities.Where(e => e != null && ESP.IsSkeletonComplete(e));

            foreach (var entity in potentialTargets)
            {
                // Verifica si el tipo de entidad es un objetivo válido
                bool isValidTargetType = false;
                switch (entity.SimpleName)
                {
                    case "Tank":
                    case "Witch":
                        if (TriggerOnBosses) isValidTargetType = true;
                        break;
                    case "Hunter":
                    case "Smoker":
                    case "Boomer":
                    case "Jockey":
                    case "Spitter":
                    case "Charger":
                        if (TriggerOnSpecials) isValidTargetType = true;
                        break;
                    case "Común":
                        if (TriggerOnCommons) isValidTargetType = true;
                        break;
                    case "Superviviente":
                        if (TriggerOnSurvivors) isValidTargetType = true;
                        break;
                }

                if (!isValidTargetType) continue;

                if (entity.SimpleName == null || !ESP.SkeletonDefinitions.TryGetValue(entity.SimpleName, out var connections)) continue;
                if (entity.BonePositions == null) continue;

                // *** LÓGICA MEJORADA: Iterar sobre las CONEXIONES del esqueleto ***
                for (int i = 0; i < connections.GetLength(0); i++)
                {
                    int boneIndex1 = connections[i, 0];
                    int boneIndex2 = connections[i, 1];

                    if (boneIndex1 >= entity.BonePositions.Length || boneIndex2 >= entity.BonePositions.Length) continue;

                    Vector3 pos1_3D = entity.BonePositions[boneIndex1];
                    Vector3 pos2_3D = entity.BonePositions[boneIndex2];

                    if (pos1_3D == Vector3.Zero || pos2_3D == Vector3.Zero) continue;

                    if (renderer.WorldToScreen(pos1_3D, out Vector2 screenPos1, screenWidth, screenHeight) &&
                        renderer.WorldToScreen(pos2_3D, out Vector2 screenPos2, screenWidth, screenHeight))
                    {
                        // Calcula la distancia desde la mira al segmento de línea del hueso
                        float distance = DistancePointToLineSegment(screenCenter, screenPos1, screenPos2);

                        if (distance < TriggerRadius)
                        {
                            targetFoundInFrame = true;
                            goto EndOfCheck; // Salimos de todo al encontrar el primer objetivo
                        }
                    }
                }
            }

        EndOfCheck:
            if (targetFoundInFrame)
            {
                NativeMethods.SimulateLeftClick();
            }
        }
    }
}