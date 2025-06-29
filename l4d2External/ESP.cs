// l4d2External/ESP.cs (Modificado para Skeletons)

using ImGuiNET;
using l4d2External;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace left4dead2Menu
{
    internal static class ESP
    {
        // <<< NUEVO: DICCIONARIO DE DEFINICIONES DE ESQUELETOS >>>
        // Aquí es donde mapearás las conexiones de huesos para cada tipo de infectado.
        // La clave es el SimpleName de la entidad, el valor es un array de pares [hueso1, hueso2].
        public static readonly Dictionary<string, int[,]> SkeletonDefinitions = new Dictionary<string, int[,]>
        {
            // Rellena estos arrays siguiendo las instrucciones del "Paso 2"
            { "Superviviente", new int[,]
                {
                    // Cabeza y Cuello
                    { 61, 14 },
                    { 14, 13 },

                    // Torso (desde el cuello hasta la pelvis)
                    { 13, 4 }, { 4, 3 },{ 3 , 2 }, { 2, 1},{ 1 , 0 },

                    // Brazo Derecho
                    { 13, 8 }, { 8, 9 }, { 9, 76 }, { 76, 11 },

                    // Brazo Izquierdo
                    { 13, 5 }, { 5, 6 }, { 6, 73 }, { 73, 72 },

                    // Pierna Izquierda (conectada a la pelvis, hueso 0)
                    { 0, 12 }, { 12, 18 }, { 18, 31 }, { 31, 32 },

                    // Pierna Derecha (conectada a la pelvis, hueso 0)
                    { 0, 16 }, { 16, 17 }, { 17, 19 }, { 19, 33 },
                }
            },
            { "Común", new int[,]
                {
                    // Cabeza y Cuello
                    
                    { 14, 13 },

                    // Torso (desde el cuello hasta la pelvis)
                    { 13, 12 }, { 12, 11 }, { 11, 10 }, { 10, 9 }, { 9, 0 },

                    // Brazo Derecho
                    { 13, 22 }, { 22, 23 }, { 23, 24 }, { 24, 25 },

                    // Brazo Izquierdo
                    { 13, 15 }, { 15, 16 }, { 16, 17 }, { 17, 18 },

                    // Pierna Izquierda (conectada a la pelvis, hueso 0)
                    { 0, 1 }, { 1, 2 }, { 2, 3 },

                    // Pierna Derecha (conectada a la pelvis, hueso 0)
                    { 0, 5 }, { 5, 6 }, { 6, 7 },
                }
            },
            { "Witch", new int[,]
                {
                    // Cabeza y Cuello
                    { 66, 14 },
                    { 14, 13 },

                    // Torso (desde el cuello hasta la pelvis)
                    { 13, 12 }, { 12, 11 }, { 11, 10 }, { 10, 9 }, { 9, 0 },

                    // Brazo Derecho
                    { 13, 34 }, { 34, 35 }, { 35, 36 }, { 36, 37 },

                    // Brazo Izquierdo
                    { 13, 15 }, { 15, 16 }, { 16, 17 }, { 17, 18 },

                    // Pierna Izquierda (conectada a la pelvis, hueso 0)
                    { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 4 },

                    // Pierna Derecha (conectada a la pelvis, hueso 0)
                    { 0, 5 }, { 5, 6 }, { 6, 7 }, { 7, 8 },
                }
            },
            { "Tank", new int[,]
                {
                    // Cabeza y Cuello
                    { 14, 13 },
                    

                    // Torso (desde el cuello hasta la pelvis)
                    { 13, 12 }, { 12, 11 }, { 11, 10 }, { 10, 9 }, { 9, 0 },

                    // Brazo Derecho
                    { 13, 34 }, { 34, 35 }, { 35, 36 }, 

                    // Brazo Izquierdo
                    { 13, 15 }, { 15, 16 }, { 16, 17 },

                    // Pierna Izquierda (conectada a la pelvis, hueso 0)
                    { 0, 1 }, { 1, 2 }, { 2, 3 }, 

                    // Pierna Derecha (conectada a la pelvis, hueso 0)
                    { 0, 5 }, { 5, 6 }, { 6, 7 }, 
                }
            },
            { "Hunter", new int[,]
                {
                    // Cabeza y Cuello
                    
                    { 14, 13 },

                    // Torso (desde el cuello hasta la pelvis)
                    { 13, 12 }, { 12, 11 }, { 11, 10 }, { 10, 9 }, { 9, 0 },

                    // Brazo Derecho
                    { 13, 34}, { 34, 35 }, { 35, 36 }, 

                    // Brazo Izquierdo
                    { 13, 15 }, { 15, 16 }, { 16, 17 },

                    // Pierna Izquierda (conectada a la pelvis, hueso 0)
                    { 0, 1 }, { 1, 2 }, {2,3 },

                    // Pierna Derecha (conectada a la pelvis, hueso 0)
                    { 0, 5}, { 5,6},{ 6, 7 },
                }
            },
            { "Smoker", new int[,]
                {
                    // Cabeza y Cuello
                    
                    { 13, 12 },

                    // Torso (desde el cuello hasta la pelvis)
                    { 13, 12 }, { 12, 11 }, { 11, 10 }, { 10, 9 }, { 9, 0 },

                    // Brazo Derecho
                    { 13, 19}, { 19,20 }, { 20, 21 },

                    // Brazo Izquierdo
                    { 13, 15}, { 15,16 }, { 16, 17 },

                    // Pierna Izquierda (conectada a la pelvis, hueso 0)
                    { 0, 1 }, { 1, 2 }, { 2 , 3 },

                    // Pierna Derecha (conectada a la pelvis, hueso 0)
                    { 0, 5}, { 5,6},{ 6, 7 },
                }
            },
            { "Jockey", new int[,]
                {
                    // Cabeza y Cuello
                    { 7, 6},
                    { 6, 5 },

                    // Torso (desde el cuello hasta la pelvis)
                    { 5, 4}, { 4, 3 }, { 3, 2 }, { 2, 1 }, { 1, 0 },

                    // Brazo Derecho
                    { 3, 27 }, { 27, 28 }, { 28, 29 },{ 29 , 30 }, 

                    // Brazo Izquierdo
                    { 3, 8 }, { 8, 9}, { 9, 10 },{ 10 , 11 }, 

                    // Pierna Izquierda (conectada a la pelvis, hueso 0)
                    { 0, 46 }, { 46, 47 }, { 47, 48}, { 48, 49},

                    // Pierna Derecha (conectada a la pelvis, hueso 0)
                    { 0, 50 }, { 50, 51 }, { 51, 52 }, { 52, 53 },
                }
            },
            { "Boomer", new int[,]
                {
                    // Cabeza y Cuello
                    
                    { 14, 13 },

                    // Torso (desde el cuello hasta la pelvis)
                    { 13, 12 }, { 12, 11 }, { 11, 10 }, { 10, 9 }, { 9, 0 },

                    // Brazo Derecho
                    { 13, 19}, { 19, 20 }, { 20, 21 }, { 21, 22},

                    // Brazo Izquierdo
                    { 13, 15 }, { 15, 16 }, { 16, 17 }, { 17, 18},

                    // Pierna Izquierda (conectada a la pelvis, hueso 0)
                    { 0, 1 }, { 1, 2 }, {2,3 },{ 3, 4 },

                    // Pierna Derecha (conectada a la pelvis, hueso 0)
                    { 0, 5}, { 5,6},{ 6, 7 }, { 7, 8 },
                }
            },
            { "Spitter", new int[,]
                {
                    // Cabeza y Cuello
                    { 7, 6},
                    { 6, 13 },

                    // Torso (desde el cuello hasta la pelvis)
                    { 13, 4}, { 4, 3 }, { 3, 2 }, { 2, 1 }, { 1, 0 },

                    // Brazo Derecho
                    { 13, 17 }, { 17, 18 }, { 18, 19 }, 

                    // Brazo Izquierdo
                    { 13, 36 }, { 36, 37 }, { 37, 38 },

                    // Pierna Izquierda (conectada a la pelvis, hueso 0)
                    { 0, 57 }, { 57, 58 }, { 58, 59}, { 59, 60},

                    // Pierna Derecha (conectada a la pelvis, hueso 0)
                    { 0, 62 }, { 62, 63 }, { 63, 64 }, { 64, 65 },
                }
            },
            { "Charger", new int[,]
                {
                    // Cabeza y Cuello
                    { 16, 15},
                    {15, 4 },

                    // Torso (desde el cuello hasta la pelvis)
                    { 4, 3 }, { 3, 2 }, { 2, 1 }, { 1, 0 },

                    // Brazo Derecho
                    { 4, 6 }, { 6, 7}, { 7, 8}, 

                    // Brazo Izquierdo
                    { 4,18},{18,19},{19,20},

                    // Pierna Izquierda (conectada a la pelvis, hueso 0)
                    { 0, 9}, { 9, 10}, { 10, 11}, { 11, 36},

                    // Pierna Derecha (conectada a la pelvis, hueso 0),
                    { 0, 12 }, { 12, 13}, { 13, 14}, { 14, 37},
                }
            }
        };

        public static readonly Dictionary<string, HashSet<int>> ActiveBoneSets;

        public static bool IsSkeletonComplete(Entity entity)
        {
            if (entity?.BonePositions == null || entity.SimpleName == null || !ActiveBoneSets.TryGetValue(entity.SimpleName, out var requiredBones))
            {
                return false;
            }

            foreach (int boneIndex in requiredBones)
            {
                if (boneIndex >= entity.BonePositions.Length || entity.BonePositions[boneIndex] == Vector3.Zero)
                {
                    return false;
                }
            }
            return true;
        }

        static ESP()
        {
            ActiveBoneSets = new Dictionary<string, HashSet<int>>();
            foreach (var skeletonDef in SkeletonDefinitions)
            {
                var boneSet = new HashSet<int>();
                var connections = skeletonDef.Value;
                for (int i = 0; i < connections.GetLength(0); i++)
                {
                    boneSet.Add(connections[i, 0]);
                    boneSet.Add(connections[i, 1]);
                }
                ActiveBoneSets.Add(skeletonDef.Key, boneSet);
            }
        }

        public static int GetHeadBoneIndex(string simpleName)
        {
            if (simpleName != null && SkeletonDefinitions.TryGetValue(simpleName, out var connections) && connections.Length > 0)
            {
                return connections[0, 0];
            }
            return -1;
        }

        public static int GetChestBoneIndex(string simpleName)
        {
            if (simpleName != null && SkeletonDefinitions.TryGetValue(simpleName, out var connections) && connections.Length > 0)
            {
                return connections[1, 1];
            }
            return -1;
        }

        public static void DrawSkeleton(ImDrawListPtr drawList, Entity entity, Renderer renderer, float screenWidth, float screenHeight, uint innerColor, uint borderColor, float maxBoneLength)
        {
            if (entity?.BonePositions == null || entity.SimpleName == null || !SkeletonDefinitions.TryGetValue(entity.SimpleName, out var connections) || connections.Length == 0) return;

            for (int i = 0; i < connections.GetLength(0); i++)
            {
                int boneIndex1 = connections[i, 0];
                int boneIndex2 = connections[i, 1];

                if (boneIndex1 >= entity.BonePositions.Length || boneIndex2 >= entity.BonePositions.Length) continue;
                if (entity.BonePositions[boneIndex1] == Vector3.Zero || entity.BonePositions[boneIndex2] == Vector3.Zero) continue;

                if (renderer.WorldToScreen(entity.BonePositions[boneIndex1], out Vector2 screenPos1, screenWidth, screenHeight) &&
                    renderer.WorldToScreen(entity.BonePositions[boneIndex2], out Vector2 screenPos2, screenWidth, screenHeight))
                {
                    if (Vector2.Distance(screenPos1, screenPos2) < maxBoneLength)
                    {
                        drawList.AddLine(screenPos1, screenPos2, borderColor, 3.0f);
                        drawList.AddLine(screenPos1, screenPos2, innerColor, 1.5f);
                    }
                }
            }
        }

        public static void DrawBox(ImDrawListPtr drawList, Vector2 topLeft, Vector2 bottomRight, uint innerColor, uint borderColor)
        {
            drawList.AddRect(topLeft, bottomRight, borderColor, 0, ImDrawFlags.None, 3.0f);
            drawList.AddRect(topLeft + new Vector2(1.5f, 1.5f), bottomRight - new Vector2(1.5f, 1.5f), innerColor, 0, ImDrawFlags.None, 1.5f);
        }

        public static void DrawHealthBar(ImDrawListPtr drawList, Vector2 boxTopLeft, Vector2 boxBottomRight, int currentHealth, int maxHealth, Config cfg)
        {
            if (maxHealth <= 0) return;

            currentHealth = Math.Max(0, Math.Min(currentHealth, maxHealth));
            float healthPercentage = (float)currentHealth / maxHealth;

            // Puedes ajustar este valor para hacer la barra más gruesa o delgada
            float barWidth = 5f;
            float barSpacing = 4f; // Espacio entre la caja del ESP y la barra de vida

            // --- LÓGICA DE DIBUJO CORREGIDA ---

            // Definimos las esquinas del área de la barra de vida. 
            // p1 es la esquina superior izquierda, p2 es la inferior derecha.
            Vector2 bg_p1 = new Vector2(boxTopLeft.X - barWidth - barSpacing, boxTopLeft.Y);
            Vector2 bg_p2 = new Vector2(boxTopLeft.X - barSpacing, boxBottomRight.Y);

            // Dibuja el fondo negro/semitransparente de la barra
            drawList.AddRectFilled(bg_p1, bg_p2, ImGui.GetColorU32(cfg.ColorHealthBarBackground));

            // Calculamos la altura que debe tener la barra de vida actual
            float totalBarHeight = bg_p2.Y - bg_p1.Y;
            float healthBarHeight = totalBarHeight * healthPercentage;

            // Definimos las esquinas de la barra de vida actual (la que tiene color)
            // Crece desde abajo (bg_p2.Y) hacia arriba.
            Vector2 health_p1 = new Vector2(bg_p1.X, bg_p2.Y - healthBarHeight);
            Vector2 health_p2 = bg_p2;

            // Interpola el color entre el de vida vacía y vida llena
            Vector4 healthColorVec = Vector4.Lerp(cfg.ColorHealthBarEmpty, cfg.ColorHealthBarFull, healthPercentage);
            uint colorHealth = ImGui.GetColorU32(healthColorVec);

            // Dibuja la barra de vida actual sobre el fondo
            if (healthBarHeight > 0)
            {
                drawList.AddRectFilled(health_p1, health_p2, colorHealth);
            }

            // Dibuja un borde negro alrededor de toda la barra para que se vea más definida
            drawList.AddRect(bg_p1, bg_p2, ImGui.GetColorU32(new Vector4(0, 0, 0, 1f)));

            // El texto de porcentaje se mantiene igual
            string healthText = $"{(healthPercentage * 100):F0} %";
            Vector2 textSize = ImGui.CalcTextSize(healthText);
            Vector2 textPos = new Vector2(
                boxTopLeft.X + ((boxBottomRight.X - boxTopLeft.X) / 2) - (textSize.X / 2),
                boxBottomRight.Y + 2
            );
            drawList.AddText(textPos + new Vector2(1, 1), ImGui.GetColorU32(new Vector4(0, 0, 0, 1f)), healthText);
            drawList.AddText(textPos, ImGui.GetColorU32(new Vector4(1, 1, 1, 1f)), healthText);
        }

        public static void DrawHeadCircle(ImDrawListPtr drawList, Entity entity, Renderer renderer, float screenWidth, float screenHeight, uint innerColor, uint borderColor, float boxWidth)
        {
            if (entity?.SimpleName == null || entity.BonePositions == null) return;

            int headBoneIndex = GetHeadBoneIndex(entity.SimpleName);
            if (headBoneIndex != -1 && headBoneIndex < entity.BonePositions.Length)
            {
                if (renderer.WorldToScreen(entity.BonePositions[headBoneIndex], out Vector2 headPos2D, screenWidth, screenHeight))
                {
                    float radius = Math.Max(4, Math.Min(15, boxWidth / 8));
                    drawList.AddCircle(headPos2D, radius, borderColor, 12, 3.0f);
                    drawList.AddCircleFilled(headPos2D, radius, innerColor);
                }
            }
        }

        public static void DrawName(ImDrawListPtr drawList, string? name, Vector2 boxTopLeft, float boxWidth, uint innerColor, uint borderColor)
        {
            string displayName = string.IsNullOrEmpty(name) ? "Unknown" : name;
            Vector2 textSize = ImGui.CalcTextSize(displayName);
            Vector2 textPos = new Vector2(
                boxTopLeft.X + (boxWidth / 2) - (textSize.X / 2),
                boxTopLeft.Y - textSize.Y - 2
            );

            drawList.AddText(textPos + new Vector2(1, 1), borderColor, displayName);
            drawList.AddText(textPos + new Vector2(-1, -1), borderColor, displayName);
            drawList.AddText(textPos + new Vector2(1, -1), borderColor, displayName);
            drawList.AddText(textPos + new Vector2(-1, 1), borderColor, displayName);
            drawList.AddText(textPos, innerColor, displayName);
        }
    }
}

    
