// l4d2External/Renderer.cs (Corregido con Bounding Box Preciso)

using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;
using l4d2External;
using System;
using System.Linq;

namespace left4dead2Menu
{
    internal class Renderer
    {
        private readonly GameMemory memory;
        private readonly IntPtr engine;
        private readonly Offsets offsets;
        private Matrix4x4 viewMatrix;

        public Renderer(GameMemory memory, IntPtr engine, Offsets offsets)
        {
            this.memory = memory;
            this.engine = engine;
            this.offsets = offsets;
            this.viewMatrix = new Matrix4x4();
        }

        public void UpdateViewMatrix()
        {
            try
            {
                var viewMatrixBase = memory.ReadPointer(engine, offsets.ViewMatrix);
                if (viewMatrixBase != IntPtr.Zero)
                {
                    var matrixAddress = viewMatrixBase + offsets.ViewMatrixOffset;
                    byte[] matrixBytes = memory.ReadBytes(matrixAddress, 64);
                    if (matrixBytes != null && matrixBytes.Length == 64)
                    {
                        float[] matrixFloats = new float[16];
                        Buffer.BlockCopy(matrixBytes, 0, matrixFloats, 0, 64);
                        viewMatrix = new Matrix4x4(
                            matrixFloats[0], matrixFloats[1], matrixFloats[2], matrixFloats[3],
                            matrixFloats[4], matrixFloats[5], matrixFloats[6], matrixFloats[7],
                            matrixFloats[8], matrixFloats[9], matrixFloats[10], matrixFloats[11],
                            matrixFloats[12], matrixFloats[13], matrixFloats[14], matrixFloats[15]
                        );
                    }
                }
            }
            catch { viewMatrix = new Matrix4x4(); }
        }

        public bool WorldToScreen(Vector3 worldPos, out Vector2 screenPos, float screenWidth, float screenHeight)
        {
            screenPos = Vector2.Zero;
            Matrix4x4 transposedMatrix = Matrix4x4.Transpose(viewMatrix);
            Vector4 clipCoords = Vector4.Transform(worldPos, transposedMatrix);

            if (clipCoords.W < 0.1f) return false;

            Vector3 ndc = new Vector3(clipCoords.X / clipCoords.W, clipCoords.Y / clipCoords.W, clipCoords.Z / clipCoords.W);

            screenPos.X = (ndc.X + 1.0f) * 0.5f * screenWidth;
            screenPos.Y = (1.0f - ndc.Y) * 0.5f * screenHeight;

            return true;
        }

        public void DrawFovCircle(ImDrawListPtr drawList, Vector2 centerScreen, float radius, Vector4 color)
        {
            if (radius > 0)
            {
                drawList.AddCircle(centerScreen, radius, ImGui.GetColorU32(color), 32, 1.5f);
            }
        }

        public void RenderAll(
            ImDrawListPtr drawList, float screenWidth, float screenHeight,
            List<Entity> common, List<Entity> special, List<Entity> bosses, List<Entity> survivors,
            bool espOnBosses, Vector4 espColorBosses, bool espOnSpecials, Vector4 espColorSpecials,
            bool espOnCommons, Vector4 espColorCommons, bool espOnSurvivors, Vector4 espColorSurvivors,
            bool espDrawBones, bool espDrawSkeleton,
            // Colores
            Vector4 colorNombreFill, Vector4 colorNombreBorde,
            Vector4 colorCajaFill, Vector4 colorCajaBorde,
            Vector4 colorEsqueletoFill, Vector4 colorEsqueletoBorde)
        {
            if (espOnCommons) RenderESPForEntities(drawList, common, espColorCommons, screenWidth, screenHeight, 50, espDrawBones, espDrawSkeleton, colorNombreFill, colorNombreBorde, colorCajaFill, colorCajaBorde, colorEsqueletoFill, colorEsqueletoBorde);
            if (espOnSpecials) RenderESPForEntities(drawList, special, espColorSpecials, screenWidth, screenHeight, 300, espDrawBones, espDrawSkeleton, colorNombreFill, colorNombreBorde, colorCajaFill, colorCajaBorde, colorEsqueletoFill, colorEsqueletoBorde);
            if (espOnBosses) RenderESPForEntities(drawList, bosses, espColorBosses, screenWidth, screenHeight, 6000, espDrawBones, espDrawSkeleton, colorNombreFill, colorNombreBorde, colorCajaFill, colorCajaBorde, colorEsqueletoFill, colorEsqueletoBorde);
            if (espOnSurvivors) RenderESPForEntities(drawList, survivors, espColorSurvivors, screenWidth, screenHeight, 100, espDrawBones, espDrawSkeleton, colorNombreFill, colorNombreBorde, colorCajaFill, colorCajaBorde, colorEsqueletoFill, colorEsqueletoBorde);
        }

        private void RenderBonesForEntity(ImDrawListPtr drawList, Entity entity, uint color, float screenWidth, float screenHeight)
        {
            if (entity.BonePositions == null) return;

            for (int i = 0; i < entity.BonePositions.Length; i++)
            {
                if (entity.BonePositions[i] == Vector3.Zero) continue;

                if (WorldToScreen(entity.BonePositions[i], out Vector2 screenPos, screenWidth, screenHeight))
                {
                    drawList.AddText(screenPos, color, i.ToString());
                }
            }
        }

        // l4d2External/Renderer.cs

        // Reemplaza este método completo
        // l4d2External/Renderer.cs

        // Reemplaza este método completo
        private void RenderESPForEntities(ImDrawListPtr drawList, List<Entity> entities, Vector4 typeColor, float screenWidth, float screenHeight, int maxHealth, bool drawBones, bool drawSkeleton,
            Vector4 vNombreFill, Vector4 vNombreBorde, Vector4 vCajaFill, Vector4 vCajaBorde, Vector4 vEsqueletoFill, Vector4 vEsqueletoBorde)
        {
            if (entities == null) return;

            // Convertir colores a formato U32 una sola vez
            uint cNombreFill = ImGui.GetColorU32(vNombreFill);
            uint cNombreBorde = ImGui.GetColorU32(vNombreBorde);
            uint cCajaFill = ImGui.GetColorU32(vCajaFill);
            uint cCajaBorde = ImGui.GetColorU32(vCajaBorde);
            uint cEsqueletoFill = ImGui.GetColorU32(vEsqueletoFill);
            uint cEsqueletoBorde = ImGui.GetColorU32(vEsqueletoBorde);
            uint cType = ImGui.GetColorU32(typeColor);

            foreach (var entity in entities)
            {
                if (entity == null) continue;

                Vector2 topLeft = Vector2.Zero, bottomRight = Vector2.Zero;
                bool isBoxOnScreen = false;

                // --- PASO 1: Calcular el Bounding Box (Caja contenedora) ---
                // Se intenta primero con los huesos por ser más preciso.
                if (entity.BonePositions != null && ESP.ActiveBoneSets.ContainsKey(entity.SimpleName))
                {
                    float minX = float.MaxValue, minY = float.MaxValue;
                    float maxX = float.MinValue, maxY = float.MinValue;

                    foreach (int boneIndex in ESP.ActiveBoneSets[entity.SimpleName])
                    {
                        if (boneIndex >= entity.BonePositions.Length || entity.BonePositions[boneIndex] == Vector3.Zero) continue;
                        if (WorldToScreen(entity.BonePositions[boneIndex], out Vector2 screenPos, screenWidth, screenHeight))
                        {
                            isBoxOnScreen = true; // Al menos un hueso es visible
                            minX = Math.Min(minX, screenPos.X);
                            minY = Math.Min(minY, screenPos.Y);
                            maxX = Math.Max(maxX, screenPos.X);
                            maxY = Math.Max(maxY, screenPos.Y);
                        }
                    }

                    if (isBoxOnScreen)
                    {
                        topLeft = new Vector2(minX - 5, minY - 5);
                        bottomRight = new Vector2(maxX + 5, maxY + 5);
                    }
                }

                // Si no se pudo crear la caja con los huesos (o la entidad no tiene), se usa el método de fallback.
                if (!isBoxOnScreen)
                {
                    if (WorldToScreen(entity.abs, out Vector2 screenHead, screenWidth, screenHeight) &&
                        WorldToScreen(entity.origin, out Vector2 screenFeet, screenWidth, screenHeight))
                    {
                        float height = Math.Abs(screenHead.Y - screenFeet.Y);
                        if (height > 2)
                        {
                            float width = height / 2.1f;
                            topLeft = new Vector2(screenFeet.X - width / 2, screenHead.Y);
                            bottomRight = new Vector2(screenFeet.X + width / 2, screenFeet.Y);
                            isBoxOnScreen = true;
                        }
                    }
                }


                // --- PASO 2: Renderizar los componentes si la caja es válida ---
                if (isBoxOnScreen)
                {
                    float width = bottomRight.X - topLeft.X;

                    // Dibuja los elementos base del ESP (Caja, Vida, Nombre)
                    ESP.DrawBox(drawList, topLeft, bottomRight, cCajaFill, cCajaBorde);
                    ESP.DrawHealthBar(drawList, topLeft, bottomRight, entity.health, maxHealth);
                    ESP.DrawName(drawList, entity.SimpleName, topLeft, width, cNombreFill, cNombreBorde);

                    // --- PASO 3: Renderizar elementos opcionales ---

                    // Dibuja el esqueleto y la cabeza SÓLO si está activado en el menú
                    if (drawSkeleton && entity.BonePositions != null && ESP.SkeletonDefinitions.ContainsKey(entity.SimpleName))
                    {
                        ESP.DrawSkeleton(drawList, entity, this, screenWidth, screenHeight, cEsqueletoFill, cEsqueletoBorde);

                        // El Headshot ESP ahora está anidado. Solo se dibuja si el esqueleto está activo.
                        int headBoneIndex = ESP.GetHeadBoneIndex(entity.SimpleName);
                        if (headBoneIndex != -1 && headBoneIndex < entity.BonePositions.Length)
                        {
                            if (WorldToScreen(entity.BonePositions[headBoneIndex], out Vector2 headPos2D, screenWidth, screenHeight))
                            {
                                float radius = Math.Max(4, Math.Min(15, width / 12));
                                drawList.AddCircleFilled(headPos2D, radius, cEsqueletoFill);
                                drawList.AddCircle(headPos2D, radius, cEsqueletoBorde, 12, 2.0f);
                            }
                        }
                    }

                    // Dibuja los índices de los huesos si está activado
                    if (drawBones)
                    {
                        RenderBonesForEntity(drawList, entity, cType, screenWidth, screenHeight);
                    }
                }
            }
        }
    }
}