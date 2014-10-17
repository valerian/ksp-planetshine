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

	public sealed class Config
	{
		private static readonly Config instance = new Config();

		private Config(){}

		public static Config Instance
		{
			get 
			{
				return instance; 
			}
		}

        public static string[] qualityLabels = {"Low", "Medium", "High"};
		public static int maxAlbedoLightsQuantity = 4;

        public int quality { get; private set; }
		public bool useVertex = false;
		public int albedoLightsQuantity = 4;
		public float baseAlbedoIntensity = 0.18f;
		public float vacuumLightLevel = 0.025f;
		public float baseGroundAmbient = 0.20f;
		public float groundAmbientOverrideRatio = 0.5f;
		public float minAlbedoFadeAltitude = 0.02f;
		public float maxAlbedoFadeAltitude = 0.10f;
		public float minAmbientFadeAltitude = 0.00f;
		public float maxAmbientFadeAltitude = 0.06f;
		public float albedoRange = 8f;
		public bool debug = false;
		public Dictionary<CelestialBody, CelestialBodyInfo> celestialBodyInfos = new Dictionary<CelestialBody, CelestialBodyInfo>();

        public void setQuality(int selectedQuality)
        {
            quality = selectedQuality;
            switch (selectedQuality) {
            case 0:
                albedoLightsQuantity = 1;
                useVertex = true;
                break;
            case 1:
                albedoLightsQuantity = maxAlbedoLightsQuantity;
                useVertex = true;
                break;
            case 2:
                albedoLightsQuantity = maxAlbedoLightsQuantity;
                useVertex = false;
                break;
            default:
                break;
            }
        }
	}

	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class ConfigManager : MonoBehaviour
	{
		public static ConfigManager Instance { get; private set; }
		private Config config = Config.Instance;
        private ConfigNode configFile;
        private ConfigNode configFileNode;

		public void Start()
		{
			if (Instance != null)
				Destroy (Instance.gameObject);
			Instance = this;

			LoadSettings ();
		}
			
		public void LoadSettings()
		{
            configFile = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/PlanetShine/Config/Settings.cfg");
            configFileNode = configFile.GetNode("PlanetShine");
			var celestialBodies = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/PlanetShine/Config/CelestialBodies.cfg");

			if (bool.Parse (configFileNode.GetValue ("useAreaLight")))
				config.albedoLightsQuantity = Config.maxAlbedoLightsQuantity;
			else
				config.albedoLightsQuantity = 1;

			config.baseAlbedoIntensity = float.Parse(configFileNode.GetValue("baseAlbedoIntensity"));
			config.vacuumLightLevel = float.Parse(configFileNode.GetValue("vacuumLightLevel"));
			config.baseGroundAmbient = float.Parse(configFileNode.GetValue("baseGroundAmbient"));
			config.groundAmbientOverrideRatio = float.Parse(configFileNode.GetValue("groundAmbientOverrideRatio"));
			config.minAlbedoFadeAltitude = float.Parse(configFileNode.GetValue("minAlbedoFadeAltitude"));
			config.maxAlbedoFadeAltitude = float.Parse(configFileNode.GetValue("maxAlbedoFadeAltitude"));
			config.minAmbientFadeAltitude = float.Parse(configFileNode.GetValue("minAmbientFadeAltitude"));
			config.maxAmbientFadeAltitude = float.Parse(configFileNode.GetValue("maxAmbientFadeAltitude"));
			config.albedoRange = float.Parse(configFileNode.GetValue("albedoRange"));
			config.useVertex = bool.Parse(configFileNode.GetValue("useVertex"));
			config.setQuality(int.Parse(configFileNode.GetValue("quality")));

			foreach (ConfigNode bodySettings in celestialBodies.GetNodes("CelestialBodyColor"))
			{
				CelestialBody body = FlightGlobals.Bodies.Find(n => n.name == bodySettings.GetValue("name"));
				if (FlightGlobals.Bodies.Contains(body))
				{
					Color color = ConfigNode.ParseColor(bodySettings.GetValue("color"))
                        * float.Parse(bodySettings.GetValue("intensity"));
					color.r = (color.r / 255f);
					color.g = (color.g / 255f);
					color.b = (color.b / 255f);
					color.a = 1;
					if (!config.celestialBodyInfos.ContainsKey(body))
						config.celestialBodyInfos.Add(body, new CelestialBodyInfo(color, float.Parse(bodySettings.GetValue("intensity")), float.Parse(bodySettings.GetValue("groundAmbient"))));
				}
			}
		}

        public void SaveSettings()
        {
			configFileNode.SetValue("useAreaLight", (config.albedoLightsQuantity > 1) ? "True" : "False");
            configFileNode.SetValue("baseAlbedoIntensity", config.baseAlbedoIntensity.ToString());
            configFileNode.SetValue("vacuumLightLevel", config.vacuumLightLevel.ToString());
            configFileNode.SetValue("baseGroundAmbient", config.baseGroundAmbient.ToString());
            configFileNode.SetValue("groundAmbientOverrideRatio", config.groundAmbientOverrideRatio.ToString());
            configFileNode.SetValue("minAlbedoFadeAltitude", config.minAlbedoFadeAltitude.ToString());
            configFileNode.SetValue("maxAlbedoFadeAltitude", config.maxAlbedoFadeAltitude.ToString());
            configFileNode.SetValue("minAmbientFadeAltitude", config.minAmbientFadeAltitude.ToString());
            configFileNode.SetValue("maxAmbientFadeAltitude", config.maxAmbientFadeAltitude.ToString());
            configFileNode.SetValue("albedoRange", config.albedoRange.ToString());
            configFileNode.SetValue("useVertex", config.useVertex ? "True" : "False");
            configFileNode.SetValue("quality", config.quality.ToString());
            configFile.Save(KSPUtil.ApplicationRootPath + "GameData/PlanetShine/Config/Settings.cfg");
        }

	}
}

