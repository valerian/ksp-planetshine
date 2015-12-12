using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlanetShine
{
    public class Body
    {
        public static Color defaultColor { get { return new Color(100f / 256f, 100f / 256f, 100f / 256f); } }

        public CelestialBody celestialBody { get; private set; }

        public bool isSun { get; private set; }
        public bool isAlbedoColorAuto { get; private set; }

        public float albedoIntensity { get; private set; }
        public float atmosphereAmbientLevel { get; private set; }

        private bool hasClouds = false;

        public Color albedoColor { get; private set; }
        public Color terrainColor { get; private set; }
        public Color atmosphereColor { get; private set; }
        public Color cloudsColor { get; private set; }

        public float atmosphereScaledCoverage { get; private set; }
        public float cloudsScaledCoverage { get; private set; }

        public Texture2D bodyRimTexture { get; private set; }


        public Body(CelestialBody sourceBody)
        {
            Logger.DebugRam("Body constructor start");
            celestialBody = sourceBody;

            isSun = (Sun.Instance.sun == celestialBody || celestialBody.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length > 0);

            LoadDefaults();
            LoadConfig();
            if (isAlbedoColorAuto)
                AutoDetermineColors();

            // mandatory cleaning after having instanciated a few textures
            GC.Collect();
            Resources.UnloadUnusedAssets();
            Logger.DebugRam("Body constructor end");
        }

        private void LoadDefaults()
        {
            //TODO have defaults customizable in Config.cfg
            isAlbedoColorAuto = true;
            albedoIntensity = isSun ? 6.0f : 1.0f;
            atmosphereAmbientLevel = celestialBody.atmosphere ? 1.0f : 0.2f;
            albedoColor = defaultColor;
            terrainColor = defaultColor;
            atmosphereColor = defaultColor;
            cloudsColor = defaultColor;
            atmosphereScaledCoverage = 0f;
            cloudsScaledCoverage = 0f;
        }

        private void LoadConfig()
        {
            ConfigNode bodyNode = GameDatabase.Instance.FindConfigNode("PlanetshineCelestialBody", "name", celestialBody.name);
            if (bodyNode == null)
                return;

            var color = bodyNode.GetValue("color");
            switch(color)
            {
                case null:
                    break;
                case "auto":
                    isAlbedoColorAuto = true;
                    break;
                default:
                    isAlbedoColorAuto = false;
                    albedoColor = ConfigNode.ParseColor32(color);
                    break;      
            }
            if (bodyNode.HasValue("intensity"))
                albedoIntensity = Utils.TryParse(bodyNode.GetValue("intensity"), albedoIntensity);
            if (bodyNode.HasValue("atmosphereAmbient"))
            {
                atmosphereAmbientLevel = Utils.TryParse(bodyNode.GetValue("atmosphereAmbient"), albedoIntensity);
            }
        }

        private void AutoDetermineColors()
        {
            // We will need memory!
            GC.Collect();
            Resources.UnloadUnusedAssets();
            Logger.DebugRam("Body AutoDetermineColors start");

            bool isTerrainDetermined = isSun ? AutoDetermineSunTerrainColor() : AutoDetermineSolidTerrainColor();
            if (!isTerrainDetermined)
                Logger.Error("Could not determine terrain color for " + celestialBody.name);

            if (celestialBody.atmosphere)
            {
                AutoDetermineAtmosphereColor();
                if (Config.Instance.eveInstalled)
                    hasClouds = AutoDetermineEveClouds();
            }

            albedoColor = Color.Lerp(Color.Lerp(terrainColor, cloudsColor, cloudsScaledCoverage), atmosphereColor, atmosphereScaledCoverage);
            Logger.DebugRam("Body AutoDetermineColors end");
        }

        private bool AutoDetermineSolidTerrainColor()
        {
            Logger.DebugRam("Body AutoDetermineSolidTerrainColor start");
            //TODO maybe remove the poles from the average color
            if (celestialBody.scaledBody.renderer.sharedMaterial.mainTexture == null)
                DemandKopernicusTexture();
            if (celestialBody.scaledBody.renderer.sharedMaterial.mainTexture == null)
            {
                Logger.Debug("Failed to determine terrain color");
                return false;
            }

            Texture2D texture = null;

            try
            {
                texture = ((Texture2D)celestialBody.scaledBody.renderer.sharedMaterial.mainTexture).CreateReadable();
                Logger.Debug("GetAverageColor TERRAIN");
                terrainColor = texture.GetAverageColorFast();
                Logger.Debug("Successfully determined terrain color");
                return true;
            }
            catch (Exception e)
            {
                Logger.Exception(e);
            }
            finally
            {
                Texture2D.Destroy(texture);
                Logger.DebugRam("Body AutoDetermineSolidTerrainColor end");
            }

            return false;
        }

        private bool AutoDetermineSunTerrainColor()
        {
            //TODO code it!
            //assuming by default that it's yellow-ish for the time being
            terrainColor = new Color32(255, 255, 180, 255);
            return true;
        }

        private bool DemandKopernicusTexture()
        {
            Logger.DebugRam("Body DemandKopernicusTexture start");
            Logger.Debug("Looking for Kopernicus ScaledSpaceDemand");
            if (!Config.Instance.kopernicusInstalled)
                return false;
            var scaledSpaceDemand = celestialBody.scaledBody.GetComponent("ScaledSpaceDemand");
            if (scaledSpaceDemand == null)
            {
                Logger.Debug("Kopernicus ScaledSpaceDemand not found");
                return false;
            }
            scaledSpaceDemand.GetType().GetMethod("OnBecameVisible", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(scaledSpaceDemand, null);
            Logger.Debug("Succesfully triggered Kopernicus ScaledSpaceDemand");
            Logger.DebugRam("Body DemandKopernicusTexture end");
            return true;
        }

        private bool AutoDetermineAtmosphereColor()
        {
            Logger.DebugRam("Body AutoDetermineAtmosphereColor start");
            if (!celestialBody.atmosphere)
                return false;

            Texture2D readableRim = null;

            try
            {
                Texture2D rim = (Texture2D)celestialBody.scaledBody.renderer.sharedMaterial.GetTexture("_rimColorRamp");
                if (rim == null)
                {
                    Logger.Error("Could not determine atmosphere color!");
                    return false;
                }
                readableRim = rim.CreateReadable();
                Logger.Debug("GetAverageColor RIM");
                atmosphereColor = readableRim.GetAverageColorPartial(0, 0, readableRim.width / 5, readableRim.height);

                // TODO determine coverage based on rimPower and rimBlend
                atmosphereScaledCoverage = 0.33f;

                return true;
            }
            catch (Exception e)
            {
                Logger.Exception(e);
            }
            finally
            {
                if (readableRim != null)
                    Texture2D.Destroy(readableRim);
                Logger.DebugRam("Body AutoDetermineAtmosphereColor end");
            }
            
            return false;
        }

        private bool AutoDetermineEveClouds()
        {
            Logger.DebugRam("Body AutoDetermineEveClouds start");
            if (!Config.Instance.eveInstalled)
                return false;

            Type cloudsObjectType = Utils.FindTypeContains("CloudsObject");
            if (cloudsObjectType == null)
                return false;

            UnityEngine.Object[] cloudsObjectList = UnityEngine.Object.FindObjectsOfType(cloudsObjectType);
            if (cloudsObjectList == null || cloudsObjectList.Length == 0)
                return false;

            List<EveCloudLayer> cloudLayers = new List<EveCloudLayer>();

            try
            {
                foreach (var cloudObject in cloudsObjectList)
                {
                    if ((string)cloudsObjectType.GetProperty("Body").GetValue(cloudObject, null) == celestialBody.name)
                    {
                        var layer2DField = cloudsObjectType.GetField("layer2D", BindingFlags.Instance | BindingFlags.NonPublic);
                        var layer2D = layer2DField.GetValue(cloudObject);
                        if (layer2D == null)
                            continue;
                        var cloudMaterialField = layer2D.GetType().GetField("CloudMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
                        Material cloudMaterial = (Material)cloudMaterialField.GetValue(layer2D);
                        if (cloudMaterial == null)
                            continue;
                        cloudLayers.Add(new EveCloudLayer(cloudMaterial));
                    }
                }
                cloudsScaledCoverage = EveCloudLayer.DetermineCoverage(cloudLayers);
                cloudsColor = EveCloudLayer.DetermineAverageColor(cloudLayers);
                return true;
            }
            catch (Exception e)
            {
                cloudsScaledCoverage = 0;
                Logger.Exception(e);
            }
            finally
            {
                foreach (EveCloudLayer cloudLayer in cloudLayers)
                    Texture2D.Destroy(cloudLayer.texture);
                Logger.DebugRam("Body AutoDetermineEveClouds end");
            }

            return false;
        }
    }
}
