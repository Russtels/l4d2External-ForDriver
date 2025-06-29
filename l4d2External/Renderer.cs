// l4d2External/Renderer.cs (RESTAURADO)
using ImGuiNET;
using System.Numerics;
using System.Collections.Generic;
using l4d2External;
using System;
using System.Linq;
using System.Runtime.InteropServices;

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
                        viewMatrix = MemoryMarshal.Read<Matrix4x4>(matrixBytes);
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

        // <<< MÉTODO RESTAURADO PARA USAR LISTAS SEPARADAS >>>
        public void RenderAll(
            ImDrawListPtr drawList, float screenWidth, float screenHeight,
            List<Entity> common, List<Entity> special, List<Entity> bosses, List<Entity> survivors,
            Config cfg)
        {
            if (cfg.EspOnCommons) RenderESPForEntities(drawList, common, screenWidth, screenHeight, cfg);
            if (cfg.EspOnSpecials) RenderESPForEntities(drawList, special, screenWidth, screenHeight, cfg);
            if (cfg.EspOnBosses) RenderESPForEntities(drawList, bosses, screenWidth, screenHeight, cfg);
            if (cfg.EspOnSurvivors) RenderESPForEntities(drawList, survivors, screenWidth, screenHeight, cfg);
        }

        private void RenderESPForEntities(ImDrawListPtr drawList, List<Entity> entities, float screenWidth, float screenHeight, Config cfg)
        {
            uint cBoxFill = ImGui.GetColorU32(cfg.ColorEspBoxFill);
            uint cBoxBorder = ImGui.GetColorU32(cfg.ColorEspBoxBorder);
            uint cSkeletonFill = ImGui.GetColorU32(cfg.ColorEspSkeletonFill);
            uint cSkeletonBorder = ImGui.GetColorU32(cfg.ColorEspSkeletonBorder);
            uint cNameFill = ImGui.GetColorU32(cfg.ColorEspNameFill);
            uint cNameBorder = ImGui.GetColorU32(cfg.ColorEspNameBorder);
            uint cHeadFill = ImGui.GetColorU32(cfg.ColorEspHeadFill);
            uint cHeadBorder = ImGui.GetColorU32(cfg.ColorEspHeadBorder);

            foreach (var entity in entities)
            {
                if (entity?.SimpleName == null) continue;

                Vector2 topLeft = Vector2.Zero, bottomRight = Vector2.Zero;
                bool isBoxOnScreen = false;

                if (ESP.IsSkeletonComplete(entity))
                {
                    float minX = float.MaxValue, minY = float.MaxValue;
                    float maxX = float.MinValue, maxY = float.MinValue;

                    if (ESP.ActiveBoneSets.TryGetValue(entity.SimpleName, out var boneSet))
                    {
                        foreach (int boneIndex in boneSet)
                        {
                            if (WorldToScreen(entity.BonePositions![boneIndex], out Vector2 screenPos, screenWidth, screenHeight))
                            {
                                isBoxOnScreen = true;
                                minX = Math.Min(minX, screenPos.X);
                                minY = Math.Min(minY, screenPos.Y);
                                maxX = Math.Max(maxX, screenPos.X);
                                maxY = Math.Max(maxY, screenPos.Y);
                            }
                        }
                    }

                    if (isBoxOnScreen)
                    {
                        topLeft = new Vector2(minX - 5, minY - 5);
                        bottomRight = new Vector2(maxX + 5, maxY + 5);
                    }
                }

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

                if (isBoxOnScreen)
                {
                    float boxWidth = bottomRight.X - topLeft.X;
                    float boxHeight = bottomRight.Y - topLeft.Y;

                    if (boxWidth <= 0 || boxHeight <= 0 || boxWidth > screenWidth || boxHeight > screenHeight) continue;

                    ESP.DrawBox(drawList, topLeft, bottomRight, cBoxFill, cBoxBorder);
                    ESP.DrawHealthBar(drawList, topLeft, bottomRight, entity.health, entity.maxHealth, cfg);
                    ESP.DrawName(drawList, entity.SimpleName, topLeft, boxWidth, cNameFill, cNameBorder);

                    if (ESP.IsSkeletonComplete(entity))
                    {
                        if (cfg.EspDrawSkeleton)
                            ESP.DrawSkeleton(drawList, entity, this, screenWidth, screenHeight, cSkeletonFill, cSkeletonBorder, 300f);

                        if (cfg.EspDrawHeadBox)
                            ESP.DrawHeadCircle(drawList, entity, this, screenWidth, screenHeight, cHeadFill, cHeadBorder, boxWidth);
                    }
                }
            }
        }
    }
}