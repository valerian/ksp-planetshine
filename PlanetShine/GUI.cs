/*
* (C) Copyright 2014, Valerian Gaudeau
* 
* Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
* project is in no way associated with nor endorsed by Squad.
* 
* This code is licensed under the Apache License Version 2.0. See the LICENSE.txt
* file for more information.
*/
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using UnityEngine;


namespace PlanetShine
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class Gui : MonoBehaviour
	{
		private Config config = Config.Instance;
		PlanetShine planetShine;
		private IButton button;
		private static Rect configWindowPosition = new Rect(0,60,200,80);
		private static Rect debugWindowPosition = new Rect(Screen.width - 420,60,80,80);
		private static GUIStyle windowStyle = null;
		private static GUIStyle redText = null;
		private static bool buttonState = false;
		private bool toolbarInstalled = false;
		private int debugWindowLabelWidth = 200;
		private int debugWindowDataWidth = 200;


		internal Gui() {
			foreach (AssemblyLoader.LoadedAssembly assembly in AssemblyLoader.loadedAssemblies)
			{
				if (assembly.name == "Toolbar")
					toolbarInstalled = true;
			}
			if (!toolbarInstalled)
				return;
			button = ToolbarManager.Instance.add("PlanetShine", "Gui");
			button.TexturePath = "PlanetShine/Icons/ps_disabled";
			button.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
			button.ToolTip = "PlanetShine Settings";
			button.OnClick += (e) => {
				planetShine = PlanetShine.Instance;
				button.TexturePath = buttonState ? "PlanetShine/Icons/ps_disabled" : "PlanetShine/Icons/ps_enabled";
				buttonState = !buttonState;
			};
		}

		public void Awake() { 
			RenderingManager.AddToPostDrawQueue(0, OnDraw);
		}

		public void Start() {
			windowStyle = new GUIStyle (HighLogic.Skin.window);
			redText = new GUIStyle (HighLogic.Skin.window);
			redText.normal.textColor = Color.red;
		}

		private void OnDraw(){
			if (buttonState) {
				configWindowPosition = GUILayout.Window (143751300, configWindowPosition, OnConfigWindow, "PlanetShine 0.1.5 - Early Beta", windowStyle);
				if (config.debug && PlanetShine.Instance != null) {
					debugWindowPosition = GUILayout.Window (143751301, debugWindowPosition, OnDebugWindow, "--- PLANETSHINE DEBUG ---", windowStyle);
				}
			}
		}

		private void OnConfigWindow(int windowID)
		{
			GUILayout.BeginVertical();
			PlanetShine.renderEnabled = GUILayout.Toggle(PlanetShine.renderEnabled, "Ground ambient & reflective lights");
			config.debug = GUILayout.Toggle(config.debug, "Debug mode");

			GUILayout.Label ("CONFIG WILL NOT BE SAVED! For testing only");

			GUILayout.Label ("Vacuum light level: " + config.vacuumLightLevel);
			config.vacuumLightLevel = (float) Math.Round(GUILayout.HorizontalSlider(config.vacuumLightLevel, 0.0f, 0.10f), 3);

			GUILayout.Label ("Albedo effect intensity: " + config.baseAlbedoIntensity);
			config.baseAlbedoIntensity = (float) Math.Round(GUILayout.HorizontalSlider(config.baseAlbedoIntensity, 0.0f, 0.30f), 2);

			GUILayout.Label ("Ground ambient effect intensity: " + config.baseGroundAmbient);
			config.baseGroundAmbient = (float) Math.Round(GUILayout.HorizontalSlider(config.baseGroundAmbient, 0.0f, 0.50f), 2);


			GUILayout.Label ("Albedo range: " + config.albedoRange +
				" (" + Math.Round(config.albedoRange * planetShine.bodyRadius / 1000f) +
				"km from " + planetShine.body.name + ")");
			config.albedoRange = (float) Math.Round(GUILayout.HorizontalSlider(config.albedoRange, 0.0f, 20f), 1);



			config.albedoLightsQuantity = GUILayout.Toggle(config.albedoLightsQuantity == 3, "Use area lights") ? 3 : 1;

			GUILayout.Label ("Area spread angle max: " + config.areaSpreadAngleMax);
			config.areaSpreadAngleMax = (float) Math.Round(GUILayout.HorizontalSlider(config.areaSpreadAngleMax, 1.0f, 90.0f));

			GUILayout.Label ("Area spread intensity multiplicator: " + config.areaSpreadIntensityMultiplicator);
			config.areaSpreadIntensityMultiplicator = (float) Math.Round(GUILayout.HorizontalSlider(config.areaSpreadIntensityMultiplicator, 0.0f, 5.0f), 1);




			GUILayout.EndVertical(); 

			GUI.DragWindow();

			configWindowPosition.x = Mathf.Clamp (configWindowPosition.x, 0f, Screen.width - configWindowPosition.width);
			configWindowPosition.y = Mathf.Clamp (configWindowPosition.y, 0f, Screen.height - configWindowPosition.height);
		}

		private void OnDebugWindow(int windowID)
		{
			GUILayout.BeginVertical();

			VariableDebugLabel ("performanceTimerLast", planetShine.performanceTimerLast);
			VariableDebugLabel ("body.name", planetShine.body.name);
			VariableDebugLabel ("bodyColor", planetShine.bodyColor);
			VariableDebugLabel ("bodyGroundAmbient", planetShine.bodyGroundAmbient);
			VariableDebugLabel ("bodyIntensity", planetShine.bodyIntensity);
			VariableDebugLabel ("bodyRadius", planetShine.bodyRadius);
			VariableDebugLabel ("bodyVesselDirection", planetShine.bodyVesselDirection);
			VariableDebugLabel ("bodySunDirection", planetShine.bodySunDirection);
			VariableDebugLabel ("vesselAltitude", planetShine.vesselAltitude);
			VariableDebugLabel ("visibleSurface", planetShine.visibleSurface);
			VariableDebugLabel ("sunAngle", planetShine.sunAngle);
			VariableDebugLabel ("visibleLightSunAngleMax", planetShine.visibleLightSunAngleMax);
			VariableDebugLabel ("visibleLightSunAngleMin", planetShine.visibleLightSunAngleMin);
			VariableDebugLabel ("visibleLightRatio", planetShine.visibleLightRatio);
			VariableDebugLabel ("visibleLightAngleAverage", planetShine.visibleLightAngleAverage);
			VariableDebugLabel ("visibleLightAngleEffect", planetShine.visibleLightAngleEffect);
			VariableDebugLabel ("boostedVisibleLightAngleEffect", planetShine.boostedVisibleLightAngleEffect);
			VariableDebugLabel ("visibleLightPositionAverage", planetShine.visibleLightPositionAverage);
			VariableDebugLabel ("atmosphereReflectionEffect", planetShine.atmosphereReflectionEffect);
			VariableDebugLabel ("atmosphereAmbientRatio", planetShine.atmosphereAmbientRatio);
			VariableDebugLabel ("atmosphereAmbientEffect", planetShine.atmosphereAmbientEffect);
			VariableDebugLabel ("areaSpreadAngle", planetShine.areaSpreadAngle);
			VariableDebugLabel ("areaSpreadAngleRatio", planetShine.areaSpreadAngleRatio);
			VariableDebugLabel ("lightRange", planetShine.lightRange);
			VariableDebugLabel ("vesselLightRangeRatio", planetShine.vesselLightRangeRatio);
			VariableDebugLabel ("lightDistanceEffect", planetShine.lightDistanceEffect);
			VariableDebugLabel ("visibleLightVesselDirection", planetShine.visibleLightVesselDirection);
			VariableDebugLabel ("lightIntensity", planetShine.lightIntensity);
			VariableDebugLabel ("vacuumColor", planetShine.vacuumColor);

			GUILayout.EndVertical(); 

			GUI.DragWindow();

			debugWindowPosition.x = Mathf.Clamp (debugWindowPosition.x, 0f, Screen.width - debugWindowPosition.width);
			debugWindowPosition.y = Mathf.Clamp (debugWindowPosition.y, 0f, Screen.height - debugWindowPosition.height);
		}

		private void VariableDebugLabel<T>(string name, T data)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (name, GUILayout.Width(debugWindowLabelWidth));
			GUILayout.Label (data.ToString(), GUILayout.Width(debugWindowDataWidth));
			GUILayout.EndHorizontal ();
		}


		internal void OnDestroy() {
			button.Destroy ();
		}
	}
}

