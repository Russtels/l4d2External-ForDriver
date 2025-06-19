// ESP.cs (Modificado)
using ImGuiNET;
using System;
using System.Numerics;

namespace left4dead2Menu
{
    internal static class ESP
    {
        public static void DrawBox(ImDrawListPtr drawList, Vector2 topLeft, Vector2 bottomRight, uint color)
        {
            drawList.AddRect(topLeft, bottomRight, color, 0, ImDrawFlags.None, 1.5f);
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

        // --- NUEVO MÉTODO PARA DIBUJAR LA CABEZA ---
        public static void DrawHeadBone(ImDrawListPtr drawList, Vector2 boxTopLeft, float boxWidth, float boxHeight, uint color)
        {
            // Círculo en la posición de la cabeza, relativo al tamaño del cuadro
            Vector2 headCenter = new Vector2(boxTopLeft.X + boxWidth / 2, boxTopLeft.Y + boxHeight * 0.1f);
            float headRadius = boxWidth * 0.15f; // Radio relativo al ancho
            drawList.AddCircle(headCenter, headRadius, color, 12, 1.5f);
        }

        // --- NUEVO MÉTODO PARA DIBUJAR EL CUERPO/TORSO ---
        public static void DrawBodyBone(ImDrawListPtr drawList, Vector2 boxTopLeft, float boxWidth, float boxHeight, uint color)
        {
            // Rectángulo en la posición del torso, relativo al tamaño del cuadro
            Vector2 bodyTopLeft = new Vector2(boxTopLeft.X + boxWidth * 0.2f, boxTopLeft.Y + boxHeight * 0.2f);
            Vector2 bodyBottomRight = new Vector2(boxTopLeft.X + boxWidth * 0.8f, boxTopLeft.Y + boxHeight * 0.6f);
            drawList.AddRect(bodyTopLeft, bodyBottomRight, color, 0, ImDrawFlags.None, 1.5f);
        }
        /// <summary>
        /// Dibuja el nombre de la entidad sobre el cuadro del ESP.
        /// </summary>
        public static void DrawName(ImDrawListPtr drawList, string? name, Vector2 boxTopLeft, float boxWidth, uint color)
        {
            // Esta comprobación ya manejaba correctamente los nulos, ahora la firma coincide.
            if (string.IsNullOrEmpty(name)) return;

            Vector2 textSize = ImGui.CalcTextSize(name);
            Vector2 textPos = new Vector2(
                boxTopLeft.X + (boxWidth / 2) - (textSize.X / 2),
                boxTopLeft.Y - textSize.Y - 2
            );
            drawList.AddText(textPos, color, name);
        }

    }
}