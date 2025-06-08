using ImGuiNET;
using System.Numerics; // Para Vector2, Vector4

namespace left4dead2Menu
{
    internal class GuiManager
    {
        public void DrawMenuControls(
            ref bool enableAimbot,
            ref float aimbotTargetZOffset,
            ref bool drawFovCircle,
            ref float fovCircleVisualRadius,
            ref float aimbotSmoothness)
        {
            // Los Begin/End de la ventana principal se manejan en Program.Render
            if (ImGui.BeginTabBar("tabs"))
            {
                if (ImGui.BeginTabItem("general"))
                {
                    ImGui.Checkbox("Aimbot", ref enableAimbot);
                    if (enableAimbot)
                    {
                        ImGui.SliderFloat("Desplazamiento Z Aimbot", ref aimbotTargetZOffset, -50.0f, 50.0f, "%.1f u");
                        ImGui.Text("Positivo: más abajo, Negativo: más arriba del punto 'abs'");
                        ImGui.Separator();
                        ImGui.Text("Configuración FOV:");
                        ImGui.Checkbox("Dibujar Círculo FOV", ref drawFovCircle);
                        ImGui.SliderFloat("Radio Círculo Visual", ref fovCircleVisualRadius, 10.0f, 300.0f, "%.0f px");
                        ImGui.Separator();
                        ImGui.Text("Configuración Suavizado:");
                        ImGui.SliderFloat("Suavizado Aimbot", ref aimbotSmoothness, 0.01f, 1.0f, "%.2f");
                    }
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("esp"))
                {
                    ImGui.Text("Opciones de ESP (a implementar)");
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }

        public void DrawFovCircle(ImDrawListPtr drawList, Vector2 centerScreen, float radius, Vector4 color)
        {
            if (radius > 0)
            {
                drawList.AddCircle(centerScreen, radius, ImGui.GetColorU32(color), 32, 1.5f);
            }
        }
    }
}