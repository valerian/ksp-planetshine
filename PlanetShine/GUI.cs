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
		private IButton button;
		private static Rect windowPosition = new Rect(0,60,240,80);
		private static GUIStyle windowStyle = null;
		private static bool buttonState = false;
		private bool toolbarInstalled = false;


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
			button.ToolTip = "Toggle This Button's Icon";
			button.OnClick += (e) => {
				button.TexturePath = buttonState ? "PlanetShine/Icons/ps_disabled" : "PlanetShine/Icons/ps_enabled";
				buttonState = !buttonState;
			};
		}

		public void Awake() { 
			RenderingManager.AddToPostDrawQueue(0, OnDraw);
		}

		public void Start() {
			windowStyle = new GUIStyle (HighLogic.Skin.window);
		}

		private void OnDraw(){
			if (buttonState)
				windowPosition = GUI.Window(1234, windowPosition, OnWindow, "PlanetShine 0.1.3 - Early Beta", windowStyle);
		}

		private void OnWindow(int windowID)
		{

			GUILayout.BeginHorizontal();
			VesselManager.renderEnabled = GUILayout.Toggle(VesselManager.renderEnabled, "Ground ambient & reflective lights");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			Config.Instance.debug = GUILayout.Toggle(Config.Instance.debug, "Debug mode");
			GUILayout.EndHorizontal(); 

			GUI.DragWindow();
		}
			
		internal void OnDestroy() {
			button.Destroy ();
		}
	}
}

