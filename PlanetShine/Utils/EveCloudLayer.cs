using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlanetShine
{
    struct EveCloudLayer
    {
        public Color color;
        public Texture2D texture;
        public float coverage;

        public EveCloudLayer(Material layer2D)
        {
            Logger.DebugRam("EveCloudLayer Constructor start");
            color = layer2D.color;
            texture = ((Texture2D)layer2D.mainTexture).CreateReadable();
            coverage = texture.GetAverageColor().a;
            Logger.DebugRam("EveCloudLayer Constructor end");
        }

        public static Color DetermineAverageColor(List<EveCloudLayer> layers)
        {
            Logger.DebugRam("EveCloudLayer DetermineAverageColor start");
            Color colorTotal = new Color(0, 0, 0, 0);
            float coverageTotal = 0;

            foreach (EveCloudLayer layer in layers)
            {
                colorTotal += layer.color * layer.coverage;
                coverageTotal += layer.coverage;
            }

            colorTotal.a = coverageTotal;
            Logger.DebugRam("EveCloudLayer DetermineAverageColor end");
            return colorTotal / coverageTotal;
        }

        public static float DetermineCoverage(List<EveCloudLayer> layers)
        {
            Logger.DebugRam("EveCloudLayer DetermineCoverage start");
            if (layers == null || layers.Count == 0)
                return 0;
            else if (layers.Count == 1)
                return layers.First().coverage;

            // we have more than 1 layers

            float coverage = 0;

            // try to use GetCombinedAlpha()
            if (GetCombinedAlpha(layers, ref coverage))
                return coverage;

            // GetCombinedAlpha() didn't work, we do a poor estimation

            // picking the layer with the most coverage
            foreach (var layer in layers)
                coverage = Math.Max(coverage, layer.coverage);

            // if two layers, arbitrary decrease of the uncovered area by 20%
            if (layers.Count == 2)
                return (coverage + ((1.0f - coverage) * 0.2f));

            Logger.DebugRam("EveCloudLayer DetermineCoverage end");

            // if more than two layers, arbitrary decrease of the uncovered area by 40%
            return (coverage + ((1.0f - coverage) * 0.4f));
        }

        public static bool GetCombinedAlpha(List<EveCloudLayer> layers, ref float alpha)
        {
            Logger.DebugRam("EveCloudLayer GetCombinedAlpha start");
            if (layers == null || layers.Count == 0)
                return false;

            int width = 0;
            int height = 0;
            bool firstPass = true;

            // checking if textures sizes match, if not return false
            foreach (EveCloudLayer layer in layers)
            {
                if (firstPass)
                    firstPass = false;
                else
                    if (layer.texture.width != width || layer.texture.height != height)
                        return false;
                width = layer.texture.width;
                height = layer.texture.height;
            }

            Color[][] pixels = new Color[layers.Count][];

            for (int i = 0; i < layers.Count; i++)
                pixels[i] = layers[i].texture.GetPixels();

            int pixelCount = pixels[0].Length;
            float totalAlpha = 0;
            float tmpAlpha = 0;

            // we superpose textures and determine the average opacity

            for (int pixelIdx = 0; pixelIdx < pixelCount; pixelIdx++)
            {
                tmpAlpha = 0;
                for (int textureIdx = 0; textureIdx < pixels.Length; textureIdx++)
                    tmpAlpha = Math.Max(tmpAlpha, pixels[textureIdx][pixelIdx].a);
                totalAlpha += tmpAlpha;
            }

            alpha = totalAlpha / ((float) pixelCount);
            Logger.DebugRam("EveCloudLayer GetCombinedAlpha end");
            return true;
        }
    }
}
