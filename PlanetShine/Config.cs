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

        private Config()
        {
            //ramCounter = new PerformanceCounter("Process", "Working Set - Private");
            //ramCounter.InstanceName = "KSP.exe";
        }

        public static Config Instance
        {
            get 
            {
                return instance; 
            }
        }

        //private PerformanceCounter ramCounter = new PerformanceCounter("Process", "Working Set - Private");
        public uint ramUsage
        {
            get
            {
                //var ramCounter = new PerformanceCounter("Process", "Working Set - Private");
                //ramCounter.InstanceName = "KSP.exe";
                //return Convert.ToInt32(ramCounter.NextValue()) / ((int)(1024) * (int)(1024));
                //return Convert.ToInt32(GC.GetTotalMemory(true) / (1024 * 1024));
                Profiler.enabled = true;
                GC.GetTotalMemory(true);
                return Profiler.GetMonoUsedSize();
                //return 0;
            }
        }

        public bool blizzyToolbarInstalled = false;
        public bool kopernicusInstalled = false;
        public bool eveInstalled = false;

        public static string[] qualityLabels = {"Low", "Medium", "High"};
        public static int maxAlbedoLightsQuantity = 4;

        public int quality { get; private set; }
        public bool useVertex = false;
        public int albedoLightsQuantity = 4;
        public float baseAlbedoIntensity = 0.27f;
        public float vacuumLightLevel = 0.03f;
        public float baseGroundAmbient = 0.50f;
        public float groundAmbientOverrideRatio = 0.60f;
        public float minAlbedoFadeAltitude = 0.00f;
        public float maxAlbedoFadeAltitude = 0.65f;
        public float minAmbientFadeAltitude = 0.10f;
        public float maxAmbientFadeAltitude = 1.00f;
        public float nearCurveStrength = 1.0f;
        public float farCurveStrength = 20.0f;
        public float curvesMixRatio = 0.5f;
        public bool debug = false;
        public int updateFrequency = 1;

        public bool stockToolbarEnabled = true;

        public void setQuality(int selectedQuality)
        {
            quality = selectedQuality;
            switch (selectedQuality) {
            case 0:
                albedoLightsQuantity = 1;
                useVertex = true;
                updateFrequency = 5;
                break;
            case 1:
                albedoLightsQuantity = maxAlbedoLightsQuantity;
                useVertex = true;
                updateFrequency = 2;
                break;
            case 2:
                albedoLightsQuantity = maxAlbedoLightsQuantity;
                useVertex = false;
                updateFrequency = 1;
                break;
            default:
                break;
            }
        }
    }


    public class ConfigDefaults
    {
        private ConfigDefaults(){}

        public static float baseAlbedoIntensity = 0.27f;
        public static float vacuumLightLevel = 0.03f;
        public static float baseGroundAmbient = 0.50f;
        public static float groundAmbientOverrideRatio = 0.60f;
        public static float minAlbedoFadeAltitude = 0.00f;
        public static float maxAlbedoFadeAltitude = 0.65f;
        public static float minAmbientFadeAltitude = 0.10f;
        public static float maxAmbientFadeAltitude = 1.00f;
        public static float nearCurveStrength = 1.0f;
        public static float farCurveStrength = 20.0f;
        public static float curvesMixRatio = 0.5f;
    }

    
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ConfigManager : MonoBehaviour
    {
        public static ConfigManager Instance { get; private set; }
        private Config config = Config.Instance;
        private ConfigNode configFile;
        private ConfigNode configFileNode;

        public void Awake()
        {
            if (Instance != null)
                Destroy (Instance.gameObject);
            Instance = this;

            LoadSettings ();

            foreach (AssemblyLoader.LoadedAssembly assembly in AssemblyLoader.loadedAssemblies)
            {
                if (assembly.name == "Toolbar")
                    config.blizzyToolbarInstalled = true;
                if (assembly.name == "Kopernicus")
                    config.kopernicusInstalled = true;
                if (assembly.name == "EVEManager")
                    config.eveInstalled = true;
            }
        }
            
        public void LoadSettings()
        {
            configFile = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/PlanetShine/Config/Settings.cfg");
            configFileNode = configFile.GetNode("PlanetShine");

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
            config.nearCurveStrength = float.Parse(configFileNode.GetValue("nearCurveStrength"));
            config.farCurveStrength = float.Parse(configFileNode.GetValue("farCurveStrength"));
            config.curvesMixRatio = float.Parse(configFileNode.GetValue("curvesMixRatio"));
            config.useVertex = bool.Parse(configFileNode.GetValue("useVertex"));
            config.updateFrequency = int.Parse(configFileNode.GetValue("updateFrequency"));
            config.setQuality(int.Parse(configFileNode.GetValue("quality")));
            if (configFileNode.HasValue("stockToolbarEnabled"))
                config.stockToolbarEnabled = bool.Parse(configFileNode.GetValue("stockToolbarEnabled"));

            if (FlightGlobals.Bodies == null)
                return;
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
            configFileNode.SetValue("nearCurveStrength", config.nearCurveStrength.ToString());
            configFileNode.SetValue("farCurveStrength", config.farCurveStrength.ToString());
            configFileNode.SetValue("curvesMixRatio", config.curvesMixRatio.ToString());
            configFileNode.SetValue("useVertex", config.useVertex ? "True" : "False");
            configFileNode.SetValue("updateFrequency", config.updateFrequency.ToString());
            configFileNode.SetValue("quality", config.quality.ToString());
            configFileNode.SetValue("stockToolbarEnabled", config.stockToolbarEnabled ? "True" : "False");
            configFile.Save(KSPUtil.ApplicationRootPath + "GameData/PlanetShine/Config/Settings.cfg");
        }

    }
}

