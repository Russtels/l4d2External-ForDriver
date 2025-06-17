// ESP.cs (Nuevo)
using ImGuiNET;
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
            currentHealth = System.Math.Max(0, System.Math.Min(currentHealth, maxHealth));

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
    }
}