using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlanetShine
{
    public class CelestialBodyData
    {
        public static Color defaultColor { get { return new Color(100f / 256f, 100f / 256f, 100f / 256f); } }

        public CelestialBody celestialBody { get; private set; }

        public bool isSun { get; private set; }
        public bool isAlbedoColorAuto { get; private set; }

        public float virtualAtmosphereDepth { get; private set; }

        public float albedoIntensity { get; private set; }
        public float atmosphereAmbientLevel { get; private set; }

        public Color albedoColor { get; set; }

        public CelestialBodyData(CelestialBody sourceBody)
        {
            celestialBody = sourceBody;

            isSun = (Sun.Instance.sun == celestialBody || celestialBody.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length > 0);

            LoadDefaults();
            LoadConfig();
        }

        private void LoadDefaults()
        {
            //TODO have defaults customizable in Config.cfg
            isAlbedoColorAuto = true;
            albedoIntensity = isSun ? 6.0f : 1.0f;
            atmosphereAmbientLevel = celestialBody.atmosphere ? 0.9f : 0.2f;
            albedoColor = defaultColor;
            virtualAtmosphereDepth = celestialBody.atmosphere ? (float)celestialBody.atmosphereDepth : ((float)celestialBody.Radius * 0.1f);
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
    }
}
