using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlanetShine
{
    class Kopernicus
    {
        public static void GenerateCelestialBodyConfig()
        {
            Debug.Log("[PlanetShine] Starting Kopernicus extraction");
            try
            {
            	// Retrieve the root config node
				ConfigNode rootKopernicusConfig = GameDatabase.Instance.GetConfigs ("Kopernicus") [0].config;

				// Search within all of the bodies
				foreach (ConfigNode bodyNode in rootKopernicusConfig.GetNodes("Body"))
                {
                    Debug.Log("[PlanetShine] Found body named " + bodyNode.GetValue("name") + " ...");
                    try
                    {
                        ConfigNode scaledVersionNode = bodyNode.GetNode("ScaledVersion");
                        if (scaledVersionNode == null)
                        {
                            Debug.Log("[PlanetShine] ... but did not find scaled version data");
                            continue;
                        }
                        ConfigNode materialNode = scaledVersionNode.GetNode("Material");
                        if (materialNode == null || !materialNode.HasValue("texture"))
                        {
                            Debug.Log("[PlanetShine] ... but did not find a texture");
                            continue;
                        }
                        Texture2D bodyTexture = Utils.LoadGameTextureFromPath(materialNode.GetValue("texture"));
                        Color bodyColor = Utils.GetTextureAverageColor(bodyTexture);
                        Debug.Log(String.Format("[PlanetShine] ... and successfully extracted color from body {0}: {1} (red: {2}, green: {3}, blue: {4}) !",
                            bodyNode.GetValue("name"),
                            bodyColor,
                            bodyColor.r * 256,
                            bodyColor.g * 256,
                            bodyColor.b * 256));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(String.Format(
                            "[PlanetShine] An exception occured while extracting Kopernicus body texture:\n{0}\nThe exception was:\n{1}",
                            bodyNode,
                            e
                        ));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(String.Format(
                    "[PlanetShine] An exception occured while extracting Kopernicus body textures\nThe exception was:\n{0}",
                    e
                ));
            }
            Debug.Log("[PlanetShine] Ending Kopernicus extraction");
        }
    }
}
