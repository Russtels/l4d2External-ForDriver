// l4d2External/GuiManager.cs (CORREGIDO)
using ImGuiNET;
using System;
using System.Numerics;

namespace left4dead2Menu
{
    internal class GuiManager
    {
        public Action OnSaveConfig = delegate { };
        public Action OnLoadConfig = delegate { };

        public void DrawMenu(ref Config cfg)
        {
            if (ImGui.BeginTabBar("MainTabBar"))
            {
                DrawAimbotTab(ref cfg);
                DrawTriggerBotTab(ref cfg);
                DrawEspTab(ref cfg);
                DrawBunnyHopTab(ref cfg);
                DrawAutoShoveTab(ref cfg);
                DrawAutoLevelTab(ref cfg);
                DrawAutoCellingTab(ref cfg);
                DrawConfigTab();
            }
            ImGui.EndTabBar();
        }

        private void DrawAimbotTab(ref Config cfg)
        {
            if (ImGui.BeginTabItem("Aimbot"))
            {
                bool enableAimbot = cfg.EnableAimbot;
                if (ImGui.Checkbox("Habilitar Aimbot", ref enableAimbot))
                    cfg.EnableAimbot = enableAimbot;

                if (cfg.EnableAimbot)
                {
                    ImGui.SeparatorText("Configuración General");
                    float aimbotSmoothness = cfg.AimbotSmoothness;
                    if (ImGui.SliderFloat("Suavizado", ref aimbotSmoothness, 0.01f, 1.0f, "%.2f"))
                        cfg.AimbotSmoothness = aimbotSmoothness;

                    ImGui.SeparatorText("Objetivo");
                    int aimbotTargetInt = (int)cfg.AimbotTargetSelection;
                    if (ImGui.RadioButton("Cabeza", ref aimbotTargetInt, (int)AimbotTarget.Head))
                        cfg.AimbotTargetSelection = AimbotTarget.Head;
                    ImGui.SameLine();
                    if (ImGui.RadioButton("Pecho", ref aimbotTargetInt, (int)AimbotTarget.Chest))
                        cfg.AimbotTargetSelection = AimbotTarget.Chest;

                    ImGui.SeparatorText("Filtro de Entidades");
                    bool aimbotOnBosses = cfg.AimbotOnBosses;
                    if (ImGui.Checkbox("Jefes (Tank, Witch)", ref aimbotOnBosses))
                        cfg.AimbotOnBosses = aimbotOnBosses;

                    bool aimbotOnSpecials = cfg.AimbotOnSpecials;
                    if (ImGui.Checkbox("Especiales", ref aimbotOnSpecials))
                        cfg.AimbotOnSpecials = aimbotOnSpecials;

                    bool aimbotOnCommons = cfg.AimbotOnCommons;
                    if (ImGui.Checkbox("Comunes", ref aimbotOnCommons))
                        cfg.AimbotOnCommons = aimbotOnCommons;

                    bool aimbotOnSurvivors = cfg.AimbotOnSurvivors;
                    if (ImGui.Checkbox("Supervivientes", ref aimbotOnSurvivors))
                        cfg.AimbotOnSurvivors = aimbotOnSurvivors;

                    ImGui.SeparatorText("Visualización de Rango");
                    bool drawFovCircle = cfg.DrawFovCircle;
                    if (ImGui.Checkbox("Mostrar FOV (Círculo 2D)", ref drawFovCircle))
                        cfg.DrawFovCircle = drawFovCircle;

                    float fovCircleVisualRadius = cfg.FovCircleVisualRadius;
                    if (ImGui.SliderFloat("Radio del FOV", ref fovCircleVisualRadius, 10.0f, 500.0f, "%.0f px"))
                        cfg.FovCircleVisualRadius = fovCircleVisualRadius;

                    bool enableAimbotArea = cfg.EnableAimbotArea;
                    if (ImGui.Checkbox("Habilitar Área de Aimbot (Radio 3D)", ref enableAimbotArea))
                        cfg.EnableAimbotArea = enableAimbotArea;

                    if (cfg.EnableAimbotArea)
                    {
                        float aimbotAreaRadius = cfg.AimbotAreaRadius;
                        if (ImGui.SliderFloat("Radio del Área", ref aimbotAreaRadius, 50.0f, 1000.0f, "%.0f u"))
                            cfg.AimbotAreaRadius = aimbotAreaRadius;

                        Vector4 aimbotAreaColor = cfg.AimbotAreaColor;
                        if (ImGui.ColorEdit4("Color del Área", ref aimbotAreaColor, ImGuiColorEditFlags.NoInputs))
                            cfg.AimbotAreaColor = aimbotAreaColor;
                    }
                }
                ImGui.EndTabItem();
            }
        }

        private void DrawTriggerBotTab(ref Config cfg)
        {
            if (ImGui.BeginTabItem("TriggerBot"))
            {
                bool enableTriggerBot = cfg.EnableTriggerBot;
                if (ImGui.Checkbox("Habilitar TriggerBot", ref enableTriggerBot))
                    cfg.EnableTriggerBot = enableTriggerBot;

                if (cfg.EnableTriggerBot)
                {
                    ImGui.SeparatorText("Configuración General");
                    float triggerRadius = cfg.TriggerBotRadius;
                    if (ImGui.SliderFloat("Radio de Detección (px)", ref triggerRadius, 1.0f, 50.0f, "%.1f px"))
                        cfg.TriggerBotRadius = triggerRadius;
                    ImGui.Text("Este es el radio en píxeles alrededor de la mira.");

                    ImGui.SeparatorText("Filtro de Entidades");
                    bool triggerOnBosses = cfg.TriggerOnBosses;
                    if (ImGui.Checkbox("Jefes (Tank, Witch)##Trigger", ref triggerOnBosses))
                        cfg.TriggerOnBosses = triggerOnBosses;

                    bool triggerOnSpecials = cfg.TriggerOnSpecials;
                    if (ImGui.Checkbox("Especiales##Trigger", ref triggerOnSpecials))
                        cfg.TriggerOnSpecials = triggerOnSpecials;

                    bool triggerOnCommons = cfg.TriggerOnCommons;
                    if (ImGui.Checkbox("Comunes##Trigger", ref triggerOnCommons))
                        cfg.TriggerOnCommons = triggerOnCommons;

                    bool triggerOnSurvivors = cfg.TriggerOnSurvivors;
                    if (ImGui.Checkbox("Supervivientes##Trigger", ref triggerOnSurvivors))
                        cfg.TriggerOnSurvivors = triggerOnSurvivors;
                }
                ImGui.EndTabItem();
            }
        }

        private void DrawEspTab(ref Config cfg)
        {
            if (ImGui.BeginTabItem("Visuals (ESP)"))
            {
                bool enableEsp = cfg.EnableEsp;
                if (ImGui.Checkbox("Habilitar ESP", ref enableEsp))
                    cfg.EnableEsp = enableEsp;

                if (cfg.EnableEsp)
                {
                    ImGui.SeparatorText("Visibilidad por Tipo");
                    bool espOnBosses = cfg.EspOnBosses;
                    if (ImGui.Checkbox("Jefes", ref espOnBosses))
                        cfg.EspOnBosses = espOnBosses;

                    bool espOnSpecials = cfg.EspOnSpecials;
                    if (ImGui.Checkbox("Especiales", ref espOnSpecials))
                        cfg.EspOnSpecials = espOnSpecials;

                    bool espOnCommons = cfg.EspOnCommons;
                    if (ImGui.Checkbox("Comunes", ref espOnCommons))
                        cfg.EspOnCommons = espOnCommons;

                    bool espOnSurvivors = cfg.EspOnSurvivors;
                    if (ImGui.Checkbox("Supervivientes", ref espOnSurvivors))
                        cfg.EspOnSurvivors = espOnSurvivors;

                    ImGui.SeparatorText("Componentes Visuales");
                    bool espDrawSkeleton = cfg.EspDrawSkeleton;
                    if (ImGui.Checkbox("Dibujar Esqueleto", ref espDrawSkeleton))
                        cfg.EspDrawSkeleton = espDrawSkeleton;

                    bool espDrawHeadBox = cfg.EspDrawHeadBox;
                    if (ImGui.Checkbox("Dibujar Círculo en Cabeza", ref espDrawHeadBox))
                        cfg.EspDrawHeadBox = espDrawHeadBox;

                    ImGui.SeparatorText("Personalización de Colores");
                    Vector4 colorEspBoxFill = cfg.ColorEspBoxFill;
                    if (ImGui.ColorEdit4("Caja (Relleno)", ref colorEspBoxFill))
                        cfg.ColorEspBoxFill = colorEspBoxFill;

                    Vector4 colorEspBoxBorder = cfg.ColorEspBoxBorder;
                    if (ImGui.ColorEdit4("Caja (Borde)", ref colorEspBoxBorder))
                        cfg.ColorEspBoxBorder = colorEspBoxBorder;

                    ImGui.Spacing();
                    Vector4 colorEspSkeletonFill = cfg.ColorEspSkeletonFill;
                    if (ImGui.ColorEdit4("Esqueleto (Relleno)", ref colorEspSkeletonFill))
                        cfg.ColorEspSkeletonFill = colorEspSkeletonFill;

                    Vector4 colorEspSkeletonBorder = cfg.ColorEspSkeletonBorder;
                    if (ImGui.ColorEdit4("Esqueleto (Borde)", ref colorEspSkeletonBorder))
                        cfg.ColorEspSkeletonBorder = colorEspSkeletonBorder;

                    ImGui.Spacing();
                    Vector4 colorEspNameFill = cfg.ColorEspNameFill;
                    if (ImGui.ColorEdit4("Nombre (Relleno)", ref colorEspNameFill))
                        cfg.ColorEspNameFill = colorEspNameFill;

                    Vector4 colorEspNameBorder = cfg.ColorEspNameBorder;
                    if (ImGui.ColorEdit4("Nombre (Borde)", ref colorEspNameBorder))
                        cfg.ColorEspNameBorder = colorEspNameBorder;

                    ImGui.Spacing();
                    Vector4 colorEspHeadFill = cfg.ColorEspHeadFill;
                    if (ImGui.ColorEdit4("Círculo Cabeza (Relleno)", ref colorEspHeadFill))
                        cfg.ColorEspHeadFill = colorEspHeadFill;

                    Vector4 colorEspHeadBorder = cfg.ColorEspHeadBorder;
                    if (ImGui.ColorEdit4("Círculo Cabeza (Borde)", ref colorEspHeadBorder))
                        cfg.ColorEspHeadBorder = colorEspHeadBorder;
                    ImGui.Spacing();
                    Vector4 colorHealthBarFull = cfg.ColorHealthBarFull;
                    if (ImGui.ColorEdit4("Vida Llena", ref colorHealthBarFull))
                        cfg.ColorHealthBarFull = colorHealthBarFull;

                    Vector4 colorHealthBarEmpty = cfg.ColorHealthBarEmpty;
                    if (ImGui.ColorEdit4("Vida Vacía", ref colorHealthBarEmpty))
                        cfg.ColorHealthBarEmpty = colorHealthBarEmpty;

                    Vector4 colorHealthBarBackground = cfg.ColorHealthBarBackground;
                    if (ImGui.ColorEdit4("Fondo de Vida", ref colorHealthBarBackground))
                        cfg.ColorHealthBarBackground = colorHealthBarBackground;
                
                }
                ImGui.EndTabItem();
            }
        }

        private void DrawBunnyHopTab(ref Config cfg)
        {
            if (ImGui.BeginTabItem("BunnyHop"))
            {
                ImGui.SeparatorText("Movimiento");
                bool enableBunnyHop = cfg.EnableBunnyHop;
                if (ImGui.Checkbox("Habilitar Bunny Hop", ref enableBunnyHop))
                    cfg.EnableBunnyHop = enableBunnyHop;
                ImGui.TextDisabled("Funciona mejor en servidores sin plugins anti-trampas.");

                ImGui.EndTabItem();
            }
        }

        private void DrawAutoShoveTab(ref Config cfg)
        {
            if (ImGui.BeginTabItem("AutoShove"))
            {
                ImGui.SeparatorText("Área de Empuje (Auto Shove)");
                bool enableAutoShove = cfg.EnableAutoShove;
                if (ImGui.Checkbox("Habilitar Auto Shove", ref enableAutoShove))
                    cfg.EnableAutoShove = enableAutoShove;

                if (cfg.EnableAutoShove)
                {
                    int shoveKey = cfg.AutoShoveKey;
                    if (ImGui.InputInt("Tecla AutoShove (VK Code)", ref shoveKey))
                        cfg.AutoShoveKey = shoveKey;

                    float shoveRadius = cfg.ShoveRadius;
                    if (ImGui.SliderFloat("Radio del Área##Shove", ref shoveRadius, 50.0f, 300.0f, "%.0f u"))
                        cfg.ShoveRadius = shoveRadius;

                    Vector4 shoveAreaColor = cfg.ShoveAreaColor;
                    if (ImGui.ColorEdit4("Color del Área##Shove", ref shoveAreaColor, ImGuiColorEditFlags.NoInputs))
                        cfg.ShoveAreaColor = shoveAreaColor;

                    ImGui.SeparatorText("Objetivos del Empuje");

                    // <<< CHECKBOXES AÑADIDOS AQUÍ >>>
                    bool shoveOnCommons = cfg.ShoveOnCommons;
                    if (ImGui.Checkbox("Comunes##Shove", ref shoveOnCommons)) cfg.ShoveOnCommons = shoveOnCommons;

                    bool shoveOnHunter = cfg.ShoveOnHunter;
                    if (ImGui.Checkbox("Hunter##Shove", ref shoveOnHunter)) cfg.ShoveOnHunter = shoveOnHunter;
                    ImGui.SameLine();

                    bool shoveOnSmoker = cfg.ShoveOnSmoker;
                    if (ImGui.Checkbox("Smoker##Shove", ref shoveOnSmoker)) cfg.ShoveOnSmoker = shoveOnSmoker;
                    ImGui.SameLine();

                    bool shoveOnBoomer = cfg.ShoveOnBoomer;
                    if (ImGui.Checkbox("Boomer##Shove", ref shoveOnBoomer)) cfg.ShoveOnBoomer = shoveOnBoomer;

                    bool shoveOnJockey = cfg.ShoveOnJockey;
                    if (ImGui.Checkbox("Jockey##Shove", ref shoveOnJockey)) cfg.ShoveOnJockey = shoveOnJockey;
                    ImGui.SameLine();

                    bool shoveOnSpitter = cfg.ShoveOnSpitter;
                    if (ImGui.Checkbox("Spitter##Shove", ref shoveOnSpitter)) cfg.ShoveOnSpitter = shoveOnSpitter;
                    ImGui.SameLine();

                    bool shoveOnCharger = cfg.ShoveOnCharger;
                    if (ImGui.Checkbox("Charger##Shove", ref shoveOnCharger)) cfg.ShoveOnCharger = shoveOnCharger;
                }
                ImGui.EndTabItem();
            }
        }

        private void DrawAutoLevelTab(ref Config cfg)
        {
            if (ImGui.BeginTabItem("AutoLevel"))
            {
                ImGui.SeparatorText("Área de Leveleo (Auto Level)");
                bool enableAutoLevel = cfg.EnableAutoLevel;
                if (ImGui.Checkbox("Habilitar Auto Level", ref enableAutoLevel))
                    cfg.EnableAutoLevel = enableAutoLevel;

                if (cfg.EnableAutoLevel)
                {
                    float levelRadius = cfg.LevelRadius;
                    if (ImGui.SliderFloat("Radio del Área##Level", ref levelRadius, 50.0f, 500.0f, "%.0f u"))
                        cfg.LevelRadius = levelRadius;

                    Vector4 levelAreaColor = cfg.LevelAreaColor;
                    if (ImGui.ColorEdit4("Color del Área##Level", ref levelAreaColor, ImGuiColorEditFlags.NoInputs))
                        cfg.LevelAreaColor = levelAreaColor;

                    ImGui.SeparatorText("Objetivos del Leveleo");

                    // <<< CHECKBOXES DETALLADOS IMPLEMENTADOS >>>
                    bool levelOnCommons = cfg.LevelOnCommons;
                    if (ImGui.Checkbox("Comunes##Level", ref levelOnCommons)) cfg.LevelOnCommons = levelOnCommons;

                    bool levelOnBosses = cfg.LevelOnBosses;
                    if (ImGui.Checkbox("Jefes (Tank, Witch)##Level", ref levelOnBosses)) cfg.LevelOnBosses = levelOnBosses;

                    ImGui.SeparatorText("Infectados Especiales");
                    bool levelOnHunter = cfg.LevelOnHunter;
                    if (ImGui.Checkbox("Hunter##Level", ref levelOnHunter)) cfg.LevelOnHunter = levelOnHunter;
                    ImGui.SameLine();

                    bool levelOnSmoker = cfg.LevelOnSmoker;
                    if (ImGui.Checkbox("Smoker##Level", ref levelOnSmoker)) cfg.LevelOnSmoker = levelOnSmoker;
                    ImGui.SameLine();

                    bool levelOnBoomer = cfg.LevelOnBoomer;
                    if (ImGui.Checkbox("Boomer##Level", ref levelOnBoomer)) cfg.LevelOnBoomer = levelOnBoomer;

                    bool levelOnJockey = cfg.LevelOnJockey;
                    if (ImGui.Checkbox("Jockey##Level", ref levelOnJockey)) cfg.LevelOnJockey = levelOnJockey;
                    ImGui.SameLine();

                    bool levelOnSpitter = cfg.LevelOnSpitter;
                    if (ImGui.Checkbox("Spitter##Level", ref levelOnSpitter)) cfg.LevelOnSpitter = levelOnSpitter;
                    ImGui.SameLine();

                    bool levelOnCharger = cfg.LevelOnCharger;
                    if (ImGui.Checkbox("Charger##Level", ref levelOnCharger)) cfg.LevelOnCharger = levelOnCharger;
                }
                ImGui.EndTabItem();
            }
        }
        private void DrawAutoCellingTab(ref Config cfg)
        {
            if (ImGui.BeginTabItem("AutoCelling"))
            {
                ImGui.SeparatorText("Ángulo de Cámara Forzado");
                bool enableAutoCelling = cfg.EnableAutoCelling;
                if (ImGui.Checkbox("Habilitar Auto Celling", ref enableAutoCelling))
                    cfg.EnableAutoCelling = enableAutoCelling;

                if (cfg.EnableAutoCelling)
                {
                    int cellingKey = cfg.AutoCellingKey;
                    if (ImGui.InputInt("Tecla AutoCelling (VK Code)", ref cellingKey))
                        cfg.AutoCellingKey = cellingKey;
                }
                ImGui.EndTabItem();
            }
        }

        private void DrawConfigTab()
        {
            if (ImGui.BeginTabItem("Configuración"))
            {
                ImGui.SeparatorText("Gestión de la Configuración");
                if (ImGui.Button("Guardar Configuración", new Vector2(200, 30)))
                {
                    OnSaveConfig();
                }
                ImGui.SameLine();
                if (ImGui.Button("Cargar Configuración", new Vector2(200, 30)))
                {
                    OnLoadConfig();
                }
                ImGui.Text("La configuración se guarda en 'config.json'");
                ImGui.EndTabItem();
            }
        }
    }
}