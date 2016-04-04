using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlanetShine
{
    public class VisualAlbedoDeterminer : AlbedoDeterminer
    {
        public AlbedoCamera localCamera;
        public AlbedoCamera scaledCamera;

        public int pixelSize;
        public float elevation = 10000f;

        public VisualAlbedoDeterminer(int pixelSize)
        {
            this.pixelSize = pixelSize;
            localCamera = new AlbedoCamera(false, LayerMask.LocalScenery, pixelSize);
            scaledCamera = new AlbedoCamera(true, LayerMask.ScaledScenery, pixelSize);
        }

        public override Color DetermineColor(CelestialBodyData sourceBodyData, Component target)
        {
            Render(sourceBodyData, target);
            return ExtractColor(sourceBodyData, target);
        }

        private void Render(CelestialBodyData sourceBodyData, Component target)
        {
            localCamera.Render(sourceBodyData.celestialBody, target, elevation);
            scaledCamera.Render(sourceBodyData.celestialBody, target, elevation);
        }

        private Color ExtractColor(CelestialBodyData sourceBodyData, Component target)
        {
            Color pixelLocal = Color.white;
            Color pixelScaled = Color.white;
            Color sumColors = new Color(0, 0, 0, 0);

            for (int i = 0; i < pixelSize; i++)
                for (int j = 0; j < pixelSize; j++)
                {
                    pixelLocal = localCamera.texture.GetPixel(i, j);
                    pixelScaled = scaledCamera.texture.GetPixel(i, j);
                    if (pixelLocal.Intensity() > pixelScaled.Intensity())
                        sumColors += pixelLocal;
                    else
                        sumColors += pixelScaled;
                }

            sumColors.a = 1f;
            return sumColors / (pixelSize * pixelSize);
        }
    }
}
