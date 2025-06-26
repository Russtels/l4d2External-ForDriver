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
            if (SkeletonDefinitions.TryGetValue(simpleName, out var connections) && connections.Length > 0)
            {
                return connections[0, 0]; // Devuelve el primer hueso de la primera conexión (la cabeza).
            }
            return -1; // No encontrado
        }

        public static int GetChestBoneIndex(string simpleName)
        {
            if (SkeletonDefinitions.TryGetValue(simpleName, out var connections) && connections.Length > 0)
            {
                // Devuelve el hueso del cuello/torso superior, un buen objetivo para el pecho.
                return connections[1, 1];
            }
            return -1; // No encontrado
        }

        // --- FUNCIONES DE DIBUJO ACTUALIZADAS ---

        public static void DrawSkeleton(ImDrawListPtr drawList, Entity entity, Renderer renderer, float screenWidth, float screenHeight, uint innerColor, uint borderColor)
        {
            if (entity.BonePositions == null || !SkeletonDefinitions.TryGetValue(entity.SimpleName, out var connections) || connections.Length == 0) return;

            for (int i = 0; i < connections.GetLength(0); i++)
            {
                int boneIndex1 = connections[i, 0];
                int boneIndex2 = connections[i, 1];

                if (boneIndex1 >= entity.BonePositions.Length || boneIndex2 >= entity.BonePositions.Length) continue;
                if (entity.BonePositions[boneIndex1] == Vector3.Zero || entity.BonePositions[boneIndex2] == Vector3.Zero) continue;


                if (renderer.WorldToScreen(entity.BonePositions[boneIndex1], out Vector2 screenPos1, screenWidth, screenHeight) &&
                    renderer.WorldToScreen(entity.BonePositions[boneIndex2], out Vector2 screenPos2, screenWidth, screenHeight))
                {
                    // Dibuja el borde (más grueso)
                    drawList.AddLine(screenPos1, screenPos2, borderColor, 3.0f);
                    // Dibuja el relleno (más delgado, encima)
                    drawList.AddLine(screenPos1, screenPos2, innerColor, 1.5f);
                }
            }
        }

        public static void DrawBox(ImDrawListPtr drawList, Vector2 topLeft, Vector2 bottomRight, uint innerColor, uint borderColor)
        {
            // Dibuja el borde
            drawList.AddRect(topLeft, bottomRight, borderColor, 0, ImDrawFlags.None, 3.0f);
            // Dibuja el relleno interior
            drawList.AddRect(topLeft + new Vector2(1.5f, 1.5f), bottomRight - new Vector2(1.5f, 1.5f), innerColor, 0, ImDrawFlags.None, 1.5f);
        }

        public static void DrawHealthBar(ImDrawListPtr drawList, Vector2 boxTopLeft, Vector2 boxBottomRight, int currentHealth, int maxHealth)
        {
            currentHealth = Math.Max(0, Math.Min(currentHealth, maxHealth));
            float healthPercentage = (float)currentHealth / maxHealth;
            float boxWidth = boxBottomRight.X - boxTopLeft.X;
            Vector2 healthBarStart = new Vector2(boxTopLeft.X, boxBottomRight.Y + 3);
            Vector2 healthBarEnd = new Vector2(boxTopLeft.X + (boxWidth * healthPercentage), boxBottomRight.Y + 7);
            Vector2 healthBarBgEnd = new Vector2(boxBottomRight.X, boxBottomRight.Y + 7);
            uint colorRed = ImGui.GetColorU32(new Vector4(1, 0, 0, 0.7f));
            uint colorGreen = ImGui.GetColorU32(new Vector4(0, 1, 0, 1f));
            drawList.AddRectFilled(healthBarStart, healthBarBgEnd, colorRed);
            drawList.AddRectFilled(healthBarStart, healthBarEnd, colorGreen);
        }

        // l4d2External/ESP.cs

        // Reemplaza este método
        public static void DrawName(ImDrawListPtr drawList, string? name, Vector2 boxTopLeft, float boxWidth, uint innerColor, uint borderColor)
        {
            // Si el nombre es nulo o vacío, lo reemplazamos por "Unknown".
            string displayName = string.IsNullOrEmpty(name) ? "Unknown" : name;

            Vector2 textSize = ImGui.CalcTextSize(displayName);
            Vector2 textPos = new Vector2(
                boxTopLeft.X + (boxWidth / 2) - (textSize.X / 2),
                boxTopLeft.Y - textSize.Y - 2
            );

            // Dibuja el borde/sombra del texto
            drawList.AddText(textPos + new Vector2(1, 1), borderColor, displayName);
            drawList.AddText(textPos + new Vector2(-1, -1), borderColor, displayName);
            drawList.AddText(textPos + new Vector2(1, -1), borderColor, displayName);
            drawList.AddText(textPos + new Vector2(-1, 1), borderColor, displayName);

            // Dibuja el texto principal
            drawList.AddText(textPos, innerColor, displayName);
        }
    }
}