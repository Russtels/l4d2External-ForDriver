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

        private void RenderESPForEntities(ImDrawListPtr drawList, List<Entity> entities, Vector4 typeColor, float screenWidth, float screenHeight, int maxHealth, bool drawBones, bool drawSkeleton,
    Vector4 vNombreFill, Vector4 vNombreBorde, Vector4 vCajaFill, Vector4 vCajaBorde, Vector4 vEsqueletoFill, Vector4 vEsqueletoBorde)
        {
            if (entities == null) return;
            uint cNombreFill = ImGui.GetColorU32(vNombreFill);
            uint cNombreBorde = ImGui.GetColorU32(vNombreBorde);
            uint cCajaFill = ImGui.GetColorU32(vCajaFill);
            uint cCajaBorde = ImGui.GetColorU32(vCajaBorde);
            uint cEsqueletoFill = ImGui.GetColorU32(vEsqueletoFill);
            uint cEsqueletoBorde = ImGui.GetColorU32(vEsqueletoBorde);
            uint cType = ImGui.GetColorU32(typeColor);

            foreach (var entity in entities)
            {
                if (entity?.BonePositions == null || string.IsNullOrEmpty(entity.SimpleName)) continue;

                // --- OPTIMIZACIÓN: Usar el conjunto de huesos pre-calculado ---
                if (!ESP.ActiveBoneSets.TryGetValue(entity.SimpleName, out var activeBoneIndices))
                {
                    continue; // No hay esqueleto definido para esta entidad
                }

                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;
                bool isAnyBoneOnScreen = false;

                foreach (int boneIndex in activeBoneIndices)
                {
                    if (boneIndex >= entity.BonePositions.Length) continue;
                    Vector3 bonePos = entity.BonePositions[boneIndex];
                    if (bonePos == Vector3.Zero) continue;

                    if (WorldToScreen(bonePos, out Vector2 screenPos, screenWidth, screenHeight))
                    {
                        isAnyBoneOnScreen = true;
                        minX = Math.Min(minX, screenPos.X);
                        minY = Math.Min(minY, screenPos.Y);
                        maxX = Math.Max(maxX, screenPos.X);
                        maxY = Math.Max(maxY, screenPos.Y);
                    }
                }

                if (!isAnyBoneOnScreen) continue;

                Vector2 topLeft = new Vector2(minX - 5, minY - 5);
                Vector2 bottomRight = new Vector2(maxX + 5, maxY + 5);
                float width = bottomRight.X - topLeft.X;

                if (drawSkeleton)
                {
                    ESP.DrawSkeleton(drawList, entity, this, screenWidth, screenHeight, cEsqueletoFill, cEsqueletoBorde);
                }

                int headBoneIndex = ESP.GetHeadBoneIndex(entity.SimpleName);
                if (headBoneIndex != -1 && headBoneIndex < entity.BonePositions.Length)
                {
                    Vector3 headPos3D = entity.BonePositions[headBoneIndex];
                    if (WorldToScreen(headPos3D, out Vector2 headPos2D, screenWidth, screenHeight))
                    {
                        float radius = Math.Max(4, Math.Min(15, width / 12)); // Limita el radio entre 4 y 15
                        drawList.AddCircleFilled(headPos2D, radius, cEsqueletoFill);
                        drawList.AddCircle(headPos2D, radius, cEsqueletoBorde, 12, 2.0f);
                    }
                }

                ESP.DrawBox(drawList, topLeft, bottomRight, cCajaFill, cCajaBorde);
                ESP.DrawHealthBar(drawList, topLeft, bottomRight, entity.health, maxHealth);
                ESP.DrawName(drawList, entity.SimpleName, topLeft, width, cNombreFill, cNombreBorde);

                if (drawBones)
                {
                    RenderBonesForEntity(drawList, entity, cType, screenWidth, screenHeight);
                }
            }
        }
    }
}