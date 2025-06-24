// l4d2External/GuiManager.cs (Versión Final y Corregida)

using ImGuiNET;
using System.Numerics;

namespace left4dead2Menu
{
    internal class GuiManager
    {
        public void DrawMenuControls(
            // Aimbot
            ref bool enableAimbot, ref float aimbotSmoothness,
            ref AimbotTarget aimbotTarget,
            ref bool aimbotOnBosses, ref bool aimbotOnSpecials, ref bool aimbotOnCommons, ref bool aimbotOnSurvivors,
            ref bool drawFovCircle, ref float fovCircleVisualRadius,
            ref bool enableAimbotArea, ref float aimbotAreaRadius, ref int aimbotAreaSegments, ref Vector4 aimbotAreaColor,

            // ESP
            ref bool enableEsp, ref bool espOnBosses, ref Vector4 espColorBosses,
            ref bool espOnSpecials, ref Vector4 espColorSpecials,
            ref bool espOnCommons, ref Vector4 espColorCommons,
            ref bool espOnSurvivors, ref Vector4 espColorSurvivors,
            ref bool espDrawBones, ref bool espDrawSkeleton,

            // Colores Personalizados
            ref Vector4 colorNombreFill, ref Vector4 colorNombreBorde,
            ref Vector4 colorCajaFill, ref Vector4 colorCajaBorde,
            ref Vector4 colorEsqueletoFill, ref Vector4 colorEsqueletoBorde,

            // Others
            ref bool enableBunnyHop,
            ref bool enableMeleeArea, ref float meleeAreaRadius, ref int meleeAreaSegments, ref Vector4 meleeAreaColor,
            ref bool meleeOnCommons,
            ref bool meleeOnHunter, ref bool meleeOnSmoker, ref bool meleeOnBoomer,
            ref bool meleeOnJockey, ref bool meleeOnSpitter, ref bool meleeOnCharger
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
                        ImGui.SliderFloat("Suavizado", ref aimbotSmoothness, 0.01f, 1.0f, "%.2f");

                        ImGui.SeparatorText("Objetivo");
                        int aimbotTargetInt = (int)aimbotTarget;
                        if (ImGui.RadioButton("Cabeza (Precisa)", ref aimbotTargetInt, (int)AimbotTarget.Head)) { aimbotTarget = AimbotTarget.Head; }
                        ImGui.SameLine();
                        if (ImGui.RadioButton("Pecho", ref aimbotTargetInt, (int)AimbotTarget.Chest)) { aimbotTarget = AimbotTarget.Chest; }

                        ImGui.SeparatorText("Filtro de Entidades");
                        ImGui.Checkbox("Jefes (Tank, Witch)", ref aimbotOnBosses);
                        ImGui.Checkbox("Especiales", ref aimbotOnSpecials);
                        ImGui.Checkbox("Comunes", ref aimbotOnCommons);
                        ImGui.Checkbox("Supervivientes", ref aimbotOnSurvivors);

                        ImGui.SeparatorText("Visualización de Rango");
                        ImGui.Checkbox("Mostrar FOV (Modo Círculo)", ref drawFovCircle);
                        ImGui.SliderFloat("Radio del FOV", ref fovCircleVisualRadius, 10.0f, 500.0f, "%.0f px");
                        ImGui.Checkbox("Habilitar Área de Aimbot (Modo Radio 3D)", ref enableAimbotArea);
                        if (enableAimbotArea)
                        {
                            ImGui.SliderFloat("Radio del Área Aimbot", ref aimbotAreaRadius, 50.0f, 1000.0f, "%.0f u");
                            ImGui.SliderInt("Segmentos (Área)", ref aimbotAreaSegments, 12, 100);
                            ImGui.ColorEdit4("Color (Área)", ref aimbotAreaColor, ImGuiColorEditFlags.NoInputs);
                        }
                    }
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("ESP"))
                {
                    ImGui.Checkbox("Habilitar ESP", ref enableEsp);
                    if (enableEsp)
                    {
                        ImGui.SeparatorText("Visibilidad por Tipo");
                        ImGui.Checkbox("Jefes", ref espOnBosses); ImGui.SameLine(); ImGui.ColorEdit4("##Color Jefes", ref espColorBosses, ImGuiColorEditFlags.NoInputs);
                        ImGui.Checkbox("Especiales", ref espOnSpecials); ImGui.SameLine(); ImGui.ColorEdit4("##Color Especiales", ref espColorSpecials, ImGuiColorEditFlags.NoInputs);
                        ImGui.Checkbox("Comunes", ref espOnCommons); ImGui.SameLine(); ImGui.ColorEdit4("##Color Comunes", ref espColorCommons, ImGuiColorEditFlags.NoInputs);
                        ImGui.Checkbox("Supervivientes", ref espOnSurvivors); ImGui.SameLine(); ImGui.ColorEdit4("##Color Supervivientes", ref espColorSurvivors, ImGuiColorEditFlags.NoInputs);

                        ImGui.SeparatorText("Componentes Visuales");
                        ImGui.Checkbox("Dibujar Esqueleto", ref espDrawSkeleton);
                        ImGui.Checkbox("Dibujar Huesos (Debug)", ref espDrawBones);

                        ImGui.SeparatorText("Colores Personalizados");
                        ImGui.ColorEdit4("Nombre Relleno", ref colorNombreFill);
                        ImGui.ColorEdit4("Nombre Borde", ref colorNombreBorde);
                        ImGui.ColorEdit4("Caja Relleno", ref colorCajaFill);
                        ImGui.ColorEdit4("Caja Borde", ref colorCajaBorde);
                        ImGui.ColorEdit4("Esqueleto Relleno", ref colorEsqueletoFill);
                        ImGui.ColorEdit4("Esqueleto Borde", ref colorEsqueletoBorde);
                    }
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Others"))
                {
                    ImGui.SeparatorText("Movimiento");
                    ImGui.Checkbox("Habilitar Bunny Hop", ref enableBunnyHop);

                    ImGui.SeparatorText("Área de Melee (Ataque Automático)");
                    ImGui.Checkbox("Habilitar Área Melee", ref enableMeleeArea);
                    if (enableMeleeArea)
                    {
                        ImGui.SliderFloat("Radio del Área Melee", ref meleeAreaRadius, 50.0f, 300.0f, "%.0f u");
                        ImGui.SliderInt("Segmentos (Melee)", ref meleeAreaSegments, 12, 100);
                        ImGui.ColorEdit4("Color (Melee)", ref meleeAreaColor, ImGuiColorEditFlags.NoInputs);

                        ImGui.SeparatorText("Objetivos Melee");
                        ImGui.Checkbox("Comunes", ref meleeOnCommons);
                        ImGui.Checkbox("Hunter", ref meleeOnHunter);
                        ImGui.SameLine();
                        ImGui.Checkbox("Smoker", ref meleeOnSmoker);
                        ImGui.SameLine();
                        ImGui.Checkbox("Boomer", ref meleeOnBoomer);
                        ImGui.Checkbox("Jockey", ref meleeOnJockey);
                        ImGui.SameLine();
                        ImGui.Checkbox("Spitter", ref meleeOnSpitter);
                        ImGui.SameLine();
                        ImGui.Checkbox("Charger", ref meleeOnCharger);
                    }
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }
}