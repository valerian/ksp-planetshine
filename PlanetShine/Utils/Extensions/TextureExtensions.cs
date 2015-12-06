using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlanetShine
{
    public static class TextureExtensions
    {
        public static Texture2D CreateReadable(this Texture2D original)
        {
            //TODO check if there is a compressed format I can use
            Logger.DebugRam("Texture2D CreateReadable start");
            Logger.Debug("MipMaps: " + original.mipmapCount);

            // Checks
            if (original == null) return null;
            if (original.width == 0 || original.height == 0) return null;

            // Create the new texture
            Texture2D finalTexture = new Texture2D(original.width, original.height);

            // isn't read or writeable ... we'll have to get tricksy
            RenderTexture rt = RenderTexture.GetTemporary(original.width, original.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, 1);
            Graphics.Blit(original, rt);
            RenderTexture.active = rt;

            // Load new texture
            finalTexture.ReadPixels(new Rect(0, 0, finalTexture.width, finalTexture.height), 0, 0);

            // Kill the old one
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            Logger.DebugRam("Texture2D CreateReadable end");

            // Return
            return finalTexture;
        }

        public static int GetInverseMipLevel(this Texture2D texture, int inverseMiplevel)
        {
            return Math.Min((texture.mipmapCount - 1), (texture.mipmapCount - inverseMiplevel));
        }

        public static Color GetAverageColorFast(this Texture2D texture)
        {
            Logger.DebugRam("Texture2D GetAverageColorFast start");
            texture.Apply(true);
            Logger.DebugRam("Texture2D GetAverageColorFast applied texture");
            Color[] texColors = texture.GetPixels(texture.mipmapCount - 1);
            Logger.DebugRam("Texture2D GetAverageColorFast got " + texColors.Length + " pixels");
            int total = texColors.Length;

            float r = 0;
            float g = 0;
            float b = 0;
            float a = 0;

            foreach (Color pixel in texColors)
            {
                r += pixel.r;
                g += pixel.g;
                b += pixel.b;
                a += pixel.a;
            }

            Logger.DebugRam("Texture2D GetAverageColorFast before null array");
            texColors = null;
            Logger.DebugRam("Texture2D GetAverageColorFast end");
            return new Color(r / total, g / total, b / total, a / total);  
        }

        public static Color GetAverageColorPartial(this Texture2D texture, int x, int y, int blockWidth, int blockHeight)
        {
            Logger.DebugRam("Texture2D GetAverageColorPartial start");
            Color[] texColors = texture.GetPixels(x, y, blockWidth, blockHeight);
            Logger.DebugRam("Texture2D GetAverageColorPartial got " + texColors.Length + " pixels");
            int total = texColors.Length;

            float r = 0;
            float g = 0;
            float b = 0;
            float a = 0;

            foreach (Color pixel in texColors)
            {
                r += pixel.r;
                g += pixel.g;
                b += pixel.b;
                a += pixel.a;
            }

            Logger.DebugRam("Texture2D GetAverageColorPartial before null array");
            texColors = null;
            Logger.DebugRam("Texture2D GetAverageColorPartial end");
            return new Color(r / total, g / total, b / total, a / total);
        }
    }
}
