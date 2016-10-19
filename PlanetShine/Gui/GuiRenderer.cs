using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlanetShine
{
    class GuiRenderer
    {
        private Config config = Config.Instance;
        private GuiManager guiManager;
        private PlanetShine planetShine;

        private static string[] tabs = { "Performance", "Intensity", "Advanced" };
        private static int currentTab = 0;
        private static Color tabUnselectedColor = new Color(0.8f, 0.8f, 0.8f);
        private static Color tabSelectedColor = new Color(0.1f, 0.1f, 0.1f);
        private static Color tabUnselectedTextColor = new Color(0.8f, 0.8f, 0.8f);
        private static Color tabSelectedTextColor = new Color(0.4f, 0.4f, 0.4f);
        private static Rect configWindowPosition = new Rect(0, 60, 200, 80);
        private static Rect debugWindowPosition = new Rect(Screen.width - 420, 60, 80, 80);
        private static GUIStyle windowStyle = null;
        private static GUIStyle tabStyle = null;
        private Color originalBackgroundColor;
        private Color originalTextColor;
        private int debugWindowLabelWidth = 200;
        private int debugWindowDataWidth = 200;
        private int settingsLabelWidth = 100;
        private int updateCounter = 0;

        public GuiRenderer(GuiManager manager)
        {
            guiManager = manager;
            windowStyle = new GUIStyle(HighLogic.Skin.window);
            tabStyle = new GUIStyle(HighLogic.Skin.window);
            planetShine = PlanetShine.Instance;
        }

        public bool Render(PlanetShine planetShine)
        {
            configWindowPosition = GUILayout.Window(143751300, configWindowPosition,
                                         OnConfigWindow, "PlanetShine 0.2.5.2 - Beta", windowStyle);
            if (config.debug && PlanetShine.Instance != null)
            {
                debugWindowPosition = GUILayout.Window(143751301, debugWindowPosition,
                                                        OnDebugWindow, "--- PLANETSHINE DEBUG ---", windowStyle);
            }
            if ((updateCounter % 100) == 0)
            {
                ConfigManager.Instance.SaveSettings();
            }
            updateCounter++;
            return true;
        }

        private void OnConfigWindow(int windowID)
        {
            originalBackgroundColor = GUI.backgroundColor;
            originalTextColor = GUI.contentColor;

            if (GUI.Button(new Rect(configWindowPosition.width - 22, 3, 19, 19), "x"))
                guiManager.isConfigDisplayed = false;

            GUILayout.Space(15);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            for (int i = 0; i < tabs.Length; ++i)
            {
                GUI.backgroundColor = currentTab == i ? tabSelectedColor : tabUnselectedColor;
                GUI.contentColor = currentTab == i ? new Color(0.6f, 1.0f, 0.8f) : new Color(0.4f, 0.66f, 0.53f);
                if (GUILayout.Button(tabs[i]))
                {
                    currentTab = i;
                }
            }
            GUI.contentColor = originalTextColor;
            GUI.backgroundColor = originalBackgroundColor;

            GUILayout.EndHorizontal();

            GUILayout.Space(30);

            switch (currentTab)
            {
                case 0:
                    DisplayTab();
                    break;
                case 1:
                    IntensityTab();
                    break;
                case 2:
                    AdvancedTab();
                    break;
                default:
                    break;
            }


            GUILayout.EndVertical();

            GUI.DragWindow();

            configWindowPosition.x = Mathf.Clamp(configWindowPosition.x, 0f, Screen.width - configWindowPosition.width);
            configWindowPosition.y = Mathf.Clamp(configWindowPosition.y, 0f, Screen.height - configWindowPosition.height);
        }

        private void DisplayTab()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Quality preset", GUILayout.Width(settingsLabelWidth));
            for (int i = 0; i < 3; ++i)
            {
                GUI.backgroundColor = config.quality == i ? tabSelectedColor : tabUnselectedColor;
                GUI.contentColor = config.quality == i ? Color.white : new Color(0.6f, 0.6f, 0.6f);
                if (GUILayout.Button(Config.qualityLabels[i]))
                {
                    config.setQuality(i);
                }
            }
            GUI.backgroundColor = originalBackgroundColor;
            GUI.contentColor = originalTextColor;

            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            QualityChoiceRow("Lights quantity", ref config.albedoLightsQuantity,
                             new DisplaySettingOption<int>[]
                    {
                        new DisplaySettingOption<int>("Single", 1),
                        new DisplaySettingOption<int>("Multiple (area)", Config.maxAlbedoLightsQuantity)
                    });

            QualityChoiceRow("Lights rendering", ref config.useVertex,
                             new DisplaySettingOption<bool>[]
                    {
                        new DisplaySettingOption<bool>("Vertex mode", true),
                        new DisplaySettingOption<bool>("Pixel mode", false)
                    });

            QualityChoiceRow("Update frequency", ref config.updateFrequency,
                             new DisplaySettingOption<int>[]
                    {
                        new DisplaySettingOption<int>("10 per second", 5),
                        new DisplaySettingOption<int>("25 per second", 2),
                        new DisplaySettingOption<int>("50 per second", 1),
                    });

        }

        private void IntensityTab()
        {
            GUILayout.Label("Planetshine light intensity: " + config.baseAlbedoIntensity);
            GUILayout.BeginHorizontal();
            config.baseAlbedoIntensity = (float)Math.Round(GUILayout.HorizontalSlider(config.baseAlbedoIntensity, 0.0f, 0.50f), 2);
            ResetButton(ref config.baseAlbedoIntensity, ConfigDefaults.baseAlbedoIntensity);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.Label("Vacuum ambient light level: " + config.vacuumLightLevel);
            GUILayout.BeginHorizontal();
            config.vacuumLightLevel = 5 * (float)Math.Round(GUILayout.HorizontalSlider(config.vacuumLightLevel / 5f, 0.0f, 0.2f), 3);
            ResetButton(ref config.vacuumLightLevel, ConfigDefaults.vacuumLightLevel);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.Label("Ground and atmosphere ambient light intensity: " + config.baseGroundAmbient);
            GUILayout.BeginHorizontal();
            config.baseGroundAmbient = (float)Math.Round(GUILayout.HorizontalSlider(config.baseGroundAmbient, 0.0f, 2.00f), 1);
            ResetButton(ref config.baseGroundAmbient, ConfigDefaults.baseGroundAmbient);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.Label("Ground ambient light override: " + config.groundAmbientOverrideRatio * 100 + "%");
            GUILayout.BeginHorizontal();
            config.groundAmbientOverrideRatio = (float)Math.Round(GUILayout.HorizontalSlider(config.groundAmbientOverrideRatio, 0.0f, 1.0f), 1);
            ResetButton(ref config.groundAmbientOverrideRatio, ConfigDefaults.groundAmbientOverrideRatio);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.Label("Planetshine maximum range: " + config.albedoRange);
            GUILayout.Label("(approximately " + Math.Round(config.albedoRange * planetShine.bodyRadius / 2000f) +
                             "km from " + planetShine.body.name + ")");
            GUILayout.BeginHorizontal();
            config.albedoRange = (float)Math.Round(GUILayout.HorizontalSlider(config.albedoRange, 0.0f, 20f), 1);
            ResetButton(ref config.albedoRange, ConfigDefaults.albedoRange);
            GUILayout.EndHorizontal();
        }

        private void AdvancedTab()
        {
            PlanetShine.renderEnabled = GUILayout.Toggle(PlanetShine.renderEnabled, "Planetshine global ON/OFF");
            if (config.blizzyToolbarInstalled)
            {
                bool stockToolbarEnabledOldValue = config.stockToolbarEnabled;
                config.stockToolbarEnabled = GUILayout.Toggle(config.stockToolbarEnabled, "Use stock toolbar");
                if (config.stockToolbarEnabled != stockToolbarEnabledOldValue)
                    guiManager.UpdateToolbarStock();
            }

            GUILayout.Space(15);


            GUI.contentColor = new Color(0.8f, 1.0f, 0.8f);
            GUILayout.Label("Planetshine fade altitude: " + config.minAlbedoFadeAltitude + " to " + config.maxAlbedoFadeAltitude);
            GUILayout.Label("(from " + Math.Round(config.minAlbedoFadeAltitude * planetShine.bodyRadius / 1000f) +
                             "km to " + Math.Round(config.maxAlbedoFadeAltitude * planetShine.bodyRadius / 1000f) +
                             "km on " + planetShine.body.name + ")");
            GUI.contentColor = originalTextColor;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Min", GUILayout.Width(50));
            config.minAlbedoFadeAltitude = (float)Math.Round(GUILayout.HorizontalSlider(config.minAlbedoFadeAltitude, 0.0f,
                                                                                         config.maxAlbedoFadeAltitude), 2);
            ResetButton(ref config.minAlbedoFadeAltitude, ConfigDefaults.minAlbedoFadeAltitude);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max", GUILayout.Width(50));
            config.maxAlbedoFadeAltitude = (float)Math.Round(GUILayout.HorizontalSlider(config.maxAlbedoFadeAltitude,
                                                                                         config.minAlbedoFadeAltitude, 0.10f), 2);
            ResetButton(ref config.maxAlbedoFadeAltitude, ConfigDefaults.maxAlbedoFadeAltitude);
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            GUI.contentColor = new Color(0.8f, 1.0f, 0.8f);
            GUILayout.Label("Ground ambient fade altitude: " + config.minAmbientFadeAltitude + " to " + config.maxAmbientFadeAltitude);
            GUILayout.Label("(from " + Math.Round(config.minAmbientFadeAltitude * planetShine.bodyRadius / 1000f) +
                             "km to " + Math.Round(config.maxAmbientFadeAltitude * planetShine.bodyRadius / 1000f) +
                             "km on " + planetShine.body.name + ")");
            GUI.contentColor = originalTextColor;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Min", GUILayout.Width(50));
            config.minAmbientFadeAltitude = (float)Math.Round(GUILayout.HorizontalSlider(config.minAmbientFadeAltitude, 0.0f,
                                                                                         config.maxAmbientFadeAltitude), 2);
            ResetButton(ref config.minAmbientFadeAltitude, ConfigDefaults.minAmbientFadeAltitude);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max", GUILayout.Width(50));
            config.maxAmbientFadeAltitude = (float)Math.Round(GUILayout.HorizontalSlider(config.maxAmbientFadeAltitude,
                                                                                         config.minAmbientFadeAltitude, 0.10f), 2);
            ResetButton(ref config.maxAmbientFadeAltitude, ConfigDefaults.maxAmbientFadeAltitude);
            GUILayout.EndHorizontal();


            GUILayout.Space(30);

            GUI.contentColor = new Color(1.0f, 0.7f, 0.6f);
            config.debug = GUILayout.Toggle(config.debug, "Debug mode");
            GUI.contentColor = originalTextColor;
        }

        private void OnDebugWindow(int windowID)
        {
            GUILayout.BeginVertical();


            VariableDebugLabel("MapView.MapIsEnabled", MapView.MapIsEnabled);
            VariableDebugLabel("performanceTimerLast", planetShine.performanceTimerLast);
            VariableDebugLabel("body.name", planetShine.body.name);

            GUI.contentColor = planetShine.bodyColor;
            VariableDebugLabel("bodyColor", planetShine.bodyColor);
            GUI.contentColor = originalTextColor;

            VariableDebugLabel("bodyAtmosphereAmbient", planetShine.bodyAtmosphereAmbient);
            VariableDebugLabel("bodyIntensity", planetShine.bodyIntensity);
            VariableDebugLabel("bodyRadius", planetShine.bodyRadius);
            VariableDebugLabel("bodyVesselDirection", planetShine.bodyVesselDirection);
            VariableDebugLabel("bodySunDirection", planetShine.bodySunDirection);
            VariableDebugLabel("vesselAltitude", planetShine.vesselAltitude);
            VariableDebugLabel("visibleSurface", planetShine.visibleSurface);
            VariableDebugLabel("sunAngle", planetShine.sunAngle);
            VariableDebugLabel("visibleLightSunAngleMax", planetShine.visibleLightSunAngleMax);
            VariableDebugLabel("visibleLightSunAngleMin", planetShine.visibleLightSunAngleMin);
            VariableDebugLabel("visibleLightRatio", planetShine.visibleLightRatio);
            VariableDebugLabel("visibleLightAngleAverage", planetShine.visibleLightAngleAverage);
            VariableDebugLabel("visibleLightAngleEffect", planetShine.visibleLightAngleEffect);
            VariableDebugLabel("boostedVisibleLightAngleEffect", planetShine.boostedVisibleLightAngleEffect);
            VariableDebugLabel("visibleLightPositionAverage", planetShine.visibleLightPositionAverage);
            VariableDebugLabel("atmosphereReflectionRatio", planetShine.atmosphereReflectionRatio);
            VariableDebugLabel("atmosphereReflectionEffect", planetShine.atmosphereReflectionEffect);
            VariableDebugLabel("atmosphereAmbientRatio", planetShine.atmosphereAmbientRatio);
            VariableDebugLabel("atmosphereAmbientEffect", planetShine.atmosphereAmbientEffect);
            VariableDebugLabel("areaSpreadAngle", planetShine.areaSpreadAngle);
            VariableDebugLabel("areaSpreadAngleRatio", planetShine.areaSpreadAngleRatio);
            VariableDebugLabel("lightRange", planetShine.lightRange);
            VariableDebugLabel("vesselLightRangeRatio", planetShine.vesselLightRangeRatio);
            VariableDebugLabel("lightDistanceEffect", planetShine.lightDistanceEffect);
            VariableDebugLabel("visibleLightVesselDirection", planetShine.visibleLightVesselDirection);
            VariableDebugLabel("lightIntensity", planetShine.lightIntensity);
            VariableDebugLabel("vacuumColor", planetShine.vacuumColor);
            VariableDebugLabel("Gui.updateCounter", updateCounter);

            GUILayout.EndVertical();

            GUI.DragWindow();

            debugWindowPosition.x = Mathf.Clamp(debugWindowPosition.x, 0f, Screen.width - debugWindowPosition.width);
            debugWindowPosition.y = Mathf.Clamp(debugWindowPosition.y, 0f, Screen.height - debugWindowPosition.height);
        }

        private void QualityChoiceRow<T>(string label, ref T target, DisplaySettingOption<T>[] choices)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(settingsLabelWidth));
            foreach (DisplaySettingOption<T> choice in choices)
            {
                GUI.backgroundColor = EqualityComparer<T>.Default.Equals(target, choice.value)
                    ? tabSelectedColor : tabUnselectedColor;
                GUI.contentColor = EqualityComparer<T>.Default.Equals(target, choice.value)
                    ? Color.white : new Color(0.6f, 0.6f, 0.6f);
                if (GUILayout.Button(choice.label)
                    && !EqualityComparer<T>.Default.Equals(target, choice.value))
                {
                    config.setQuality(3);
                    target = choice.value;
                }
            }
            GUI.backgroundColor = originalBackgroundColor;
            GUI.contentColor = originalTextColor;
            GUILayout.EndHorizontal();
        }

        private void VariableDebugLabel<T>(string name, T data)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(debugWindowLabelWidth));
            GUILayout.Label(data.ToString(), GUILayout.Width(debugWindowDataWidth));
            GUILayout.EndHorizontal();
        }

        private void ResetButton<T>(ref T target, T original)
        {
            if (GUILayout.Button("Reset", GUILayout.Width(50)))
                target = original;
        }
    }
}
