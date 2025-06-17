// GuiManager.cs (Modificado)
using ImGuiNET;
using System.Numerics;

namespace left4dead2Menu
{
    internal class GuiManager
    {
        public void DrawMenuControls(
            // Parámetros del Aimbot
            ref bool enableAimbot, ref float aimbotTargetZOffset, ref bool drawFovCircle,
            ref float fovCircleVisualRadius, ref float aimbotSmoothness, ref bool aimbotOnBosses,
            ref bool aimbotOnSpecials, ref bool aimbotOnCommons, ref bool aimbotOnSurvivors,

            // Parámetros del ESP
            ref bool enableEsp, ref bool espOnBosses, ref Vector4 espColorBosses,
            ref bool espOnSpecials, ref Vector4 espColorSpecials,
            ref bool espOnCommons, ref Vector4 espColorCommons,
            ref bool espOnSurvivors, ref Vector4 espColorSurvivors
            )
        {
            if (ImGui.BeginTabBar("MainTabBar"))
            {
                if (ImGui.BeginTabItem("Aimbot"))
                {
                    ImGui.Checkbox("Habilitar Aimbot", ref enableAimbot);
                    if (enableAimbot)
                    {
                        ImGui.SeparatorText("Configuración General");
                        ImGui.SliderFloat("Desplazamiento Z", ref aimbotTargetZOffset, -50.0f, 50.0f, "%.1f u");
                        ImGui.SliderFloat("Suavizado", ref aimbotSmoothness, 0.01f, 1.0f, "%.2f");

                        ImGui.SeparatorText("Objetivos");
                        ImGui.Checkbox("Jefes (Tank, Witch)", ref aimbotOnBosses);
                        ImGui.Checkbox("Especiales", ref aimbotOnSpecials);
                        ImGui.Checkbox("Comunes", ref aimbotOnCommons);
                        ImGui.Checkbox("Supervivientes", ref aimbotOnSurvivors);

                        ImGui.SeparatorText("Visualización");
                        ImGui.Checkbox("Dibujar Círculo FOV", ref drawFovCircle);
                        ImGui.SliderFloat("Radio del Círculo", ref fovCircleVisualRadius, 10.0f, 500.0f, "%.0f px");
                    }
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("ESP"))
                {
                    ImGui.Checkbox("Habilitar ESP", ref enableEsp);
                    if (enableEsp)
                    {
                        ImGui.SeparatorText("Visibilidad y Colores");

                        ImGui.Checkbox("Jefes", ref espOnBosses);
                        ImGui.SameLine();
                        ImGui.ColorEdit4("##Color Jefes", ref espColorBosses, ImGuiColorEditFlags.NoInputs);

                        ImGui.Checkbox("Especiales", ref espOnSpecials);
                        ImGui.SameLine();
                        ImGui.ColorEdit4("##Color Especiales", ref espColorSpecials, ImGuiColorEditFlags.NoInputs);

                        ImGui.Checkbox("Comunes", ref espOnCommons);
                        ImGui.SameLine();
                        ImGui.ColorEdit4("##Color Comunes", ref espColorCommons, ImGuiColorEditFlags.NoInputs);

                        ImGui.Checkbox("Supervivientes", ref espOnSurvivors);
                        ImGui.SameLine();
                        ImGui.ColorEdit4("##Color Supervivientes", ref espColorSurvivors, ImGuiColorEditFlags.NoInputs);
                    }
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }
}