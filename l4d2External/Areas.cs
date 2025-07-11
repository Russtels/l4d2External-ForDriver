// Areas.cs (Refactorizado con lógica de Esfera)
using ImGuiNET;
using System.Numerics;
using l4d2External;
using System;

namespace left4dead2Menu
{
    internal class Areas
    {
        /// <summary>
        /// Dibuja una esfera 3D en el espacio del juego.
        /// </summary>
        public void DrawSphereArea(ImDrawListPtr drawList, Vector3 center, Renderer renderer, float screenWidth, float screenHeight, float radius, int segments, Vector4 color)
        {
            if (center == Vector3.Zero) return;

            uint uintColor = ImGui.GetColorU32(color);
            float step = (float)(2 * Math.PI / segments);

            // Dibuja los círculos horizontales (latitud)
            for (int i = 0; i <= segments / 4; i++) // Dibuja un cuarto de los círculos para mejor rendimiento
            {
                float latitude = (float)(i * (Math.PI / (segments / 4))) - (float)(Math.PI / 2);
                Vector2? prevScreenPos = null;
                Vector2? firstScreenPos = null;

                for (int j = 0; j <= segments; j++)
                {
                    float longitude = j * step;

                    Vector3 point = new Vector3(
                        center.X + radius * (float)Math.Cos(latitude) * (float)Math.Cos(longitude),
                        center.Y + radius * (float)Math.Cos(latitude) * (float)Math.Sin(longitude),
                        center.Z + radius * (float)Math.Sin(latitude)
                    );

                    if (renderer.WorldToScreen(point, out Vector2 currentScreenPos, screenWidth, screenHeight))
                    {
                        if (prevScreenPos.HasValue)
                        {
                            drawList.AddLine(prevScreenPos.Value, currentScreenPos, uintColor, 1.0f);
                        }
                        if (!firstScreenPos.HasValue)
                        {
                            firstScreenPos = currentScreenPos;
                        }
                        prevScreenPos = currentScreenPos;
                    }
                }
                if (firstScreenPos.HasValue && prevScreenPos.HasValue)
                {
                    drawList.AddLine(prevScreenPos.Value, firstScreenPos.Value, uintColor, 1.0f);
                }
            }

            // Dibuja los círculos verticales (longitud)
            for (int i = 0; i < segments / 2; i++) // Dibuja la mitad de los círculos para mejor rendimiento
            {
                float longitude = i * step;
                Vector2? prevScreenPos = null;
                Vector2? firstScreenPos = null;

                for (int j = 0; j <= segments; j++)
                {
                    float latitude = (j * step) - (float)(Math.PI / 2);

                    Vector3 point = new Vector3(
                       center.X + radius * (float)Math.Cos(latitude) * (float)Math.Cos(longitude),
                       center.Y + radius * (float)Math.Cos(latitude) * (float)Math.Sin(longitude),
                       center.Z + radius * (float)Math.Sin(latitude)
                   );

                    if (renderer.WorldToScreen(point, out Vector2 currentScreenPos, screenWidth, screenHeight))
                    {
                        if (prevScreenPos.HasValue)
                        {
                            drawList.AddLine(prevScreenPos.Value, currentScreenPos, uintColor, 1.0f);
                        }
                        if (!firstScreenPos.HasValue)
                        {
                            firstScreenPos = currentScreenPos;
                        }
                        prevScreenPos = currentScreenPos;
                    }
                }
                if (firstScreenPos.HasValue && prevScreenPos.HasValue)
                {
                    drawList.AddLine(prevScreenPos.Value, firstScreenPos.Value, uintColor, 1.0f);
                }
            }
        }
    }
}