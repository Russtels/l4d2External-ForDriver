// GuiManager.cs (Versión Final Sincronizada)
using ImGuiNET;
using System.Numerics;

namespace left4dead2Menu
{
    internal class GuiManager
    {
        public void DrawMenuControls(
            // Aimbot
            ref bool enableAimbot, ref float aimbotTargetZOffset, ref bool drawFovCircle,
            ref float fovCircleVisualRadius, ref float aimbotSmoothness,
            ref AimbotTarget aimbotTarget,
            ref bool aimbotOnBosses, ref bool aimbotOnSpecials, ref bool aimbotOnCommons, ref bool aimbotOnSurvivors,
            // Nuevos para Aimbot Area
            ref bool enableAimbotArea, ref float aimbotAreaRadius, ref int aimbotAreaSegments, ref Vector4 aimbotAreaColor,

            // ESP
            ref bool enableEsp, ref bool espOnBosses, ref Vector4 espColorBosses,
            ref bool espOnSpecials, ref Vector4 espColorSpecials,
            ref bool espOnCommons, ref Vector4 espColorCommons,
            ref bool espOnSurvivors, ref Vector4 espColorSurvivors,
            ref bool espDrawHead, ref bool espDrawBody,

            // Others
            ref bool enableBunnyHop,
            ref bool enableMeleeArea, ref float meleeAreaRadius, ref int meleeAreaSegments, ref Vector4 meleeAreaColor
            )
        {
            if (ImGui.BeginTabBar("MainTabBar"))
            {
                if (ImGui.BeginTabItem("Aimbot"))
                {
                    ImGui.Checkbox("Habilitar Aimbot", ref enableAimbot);
                    if (enableAimbot)
                    {
                        ImGui.SeparatorText("config del aimbot");
                        ImGui.SliderFloat("Desplazamiento Z(arreglo en lo que saco el boneESP)", ref aimbotTargetZOffset, -50.0f, 50.0f, "%.1f u");
                        ImGui.SliderFloat("Suavizado", ref aimbotSmoothness, 0.01f, 1.0f, "%.2f");

                        ImGui.SeparatorText("Hitboxes");
                        int aimbotTargetInt = (int)aimbotTarget;
                        if (ImGui.RadioButton("Cabeza", ref aimbotTargetInt, (int)AimbotTarget.Head))
                        {
                            aimbotTarget = AimbotTarget.Head;
                        }
                        ImGui.SameLine();
                        if (ImGui.RadioButton("Pecho", ref aimbotTargetInt, (int)AimbotTarget.Chest))
                        {
                            aimbotTarget = AimbotTarget.Chest;
                        }

                        ImGui.SeparatorText("Objetivos");
                        ImGui.Checkbox("Jefes (Tank, Witch)", ref aimbotOnBosses);
                        ImGui.Checkbox("Especiales(Charger, Jockey, Smoker, Boomer , Spitter, Hunter)", ref aimbotOnSpecials);
                        ImGui.Checkbox("Commons", ref aimbotOnCommons);
                        ImGui.Checkbox("Supervivientes", ref aimbotOnSurvivors);

                        ImGui.SeparatorText("Aimbot FOV");
                        ImGui.Checkbox("Show FOV", ref drawFovCircle);
                        ImGui.SliderFloat("Radio del FOV", ref fovCircleVisualRadius, 10.0f, 500.0f, "%.0f px");
                        

                        // --- NUEVOS CONTROLES PARA AIMBOT AREA ---
                        ImGui.SeparatorText("Área de Aimbot (Modo Radio)");
                        ImGui.Checkbox("Habilitar Área de Aimbot", ref enableAimbotArea);
                        if (enableAimbotArea)
                        {
                            ImGui.SliderFloat("Radio del Área Aimbot", ref aimbotAreaRadius, 50.0f, 1000.0f, "%.0f u");
                            ImGui.SliderInt("Segmentos (Aimbot)", ref aimbotAreaSegments, 12, 100);
                            ImGui.ColorEdit4("Color (Aimbot)", ref aimbotAreaColor, ImGuiColorEditFlags.NoInputs);
                        }
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

                        ImGui.SeparatorText("Huesos (Improvisado)");
                        ImGui.Checkbox("Dibujar Cabeza", ref espDrawHead);
                        ImGui.Checkbox("Dibujar Cuerpo", ref espDrawBody);
                    }
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Others"))
                {
                    ImGui.SeparatorText("Movimiento");
                    ImGui.Checkbox("Habilitar Bunny Hop", ref enableBunnyHop);

                    // --- SECCIÓN RENOMBRADA Y ACTUALIZADA ---
                    ImGui.SeparatorText("Área de Melee (Ataque Automático)");
                    ImGui.Checkbox("Habilitar Área Melee", ref enableMeleeArea);
                    if (enableMeleeArea)
                    {
                        ImGui.SliderFloat("Radio del Área Melee", ref meleeAreaRadius, 50.0f, 300.0f, "%.0f u");
                        ImGui.SliderInt("Segmentos (Melee)", ref meleeAreaSegments, 12, 100);
                        ImGui.ColorEdit4("Color (Melee)", ref meleeAreaColor, ImGuiColorEditFlags.NoInputs);
                    }

                    ImGui.EndTabItem();

                    
                }
            }
            ImGui.EndTabBar();
        }
    }
}