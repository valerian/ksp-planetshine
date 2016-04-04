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

            // Return
            return finalTexture;
        }

        public static int GetInverseMipLevel(this Texture2D texture, int inverseMiplevel)
        {
            return Math.Min((texture.mipmapCount - 1), (texture.mipmapCount - inverseMiplevel));
        }

        public static Color GetAverageColorFast(this Texture2D texture)
        {
            texture.Apply(true);
            Color[] texColors = texture.GetPixels(texture.mipmapCount - 1);
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

            texColors = null;
            return new Color(r / total, g / total, b / total, a / total);  
        }

        public static Color GetAverageColorSlow(this Texture2D texture)
        {
            return texture.GetAverageColorSlow(0, 0, texture.width, texture.height);
        }

        public static Color GetAverageColorSlow(this Texture2D texture, int x, int y, int width, int height)
        {
            if (x < 0 || y < 0 || x + width > texture.width || y + height > texture.height)
                throw new Exception("Target rect out of texture bounds");
            int total = width * height;
            Color sumColors = new Color(0, 0, 0, 0);

            for (int i = x; i < x + width; i++)
                for (int j = y; j < y + height; j++)
                    sumColors += texture.GetPixel(i, j);
            return sumColors / total;
        }

        public static Color GetAverageColorCenterWeighted(this Texture2D texture, float centerWeight, float curvePower = 2)
        {
            float total = 0;
            Color sumColors = new Color(0, 0, 0, 0);
            Vector2 center = new Vector2(texture.width / 2, texture.height / 2);
            Vector2 local = new Vector2();
            float diagonalRadius = Mathf.Sqrt((texture.width * texture.width) + (texture.height * texture.height)) / 2;
            float weight = 0;

            for (int x = 0; x < texture.width; x++)
                for (int y = 0; y < texture.height; y++)
                {
                    local.x = x;
                    local.y = y;
                    weight = Mathf.Pow(1f + ((1f - ((local - center).magnitude / diagonalRadius)) * (centerWeight - 1f)), curvePower);
                    sumColors += texture.GetPixel(x, y) * weight;
                    total += weight;
                }
            return sumColors / total;
        }

        public static Color GetAverageColorPartial(this Texture2D texture, int x, int y, int blockWidth, int blockHeight)
        {
            Color[] texColors = texture.GetPixels(x, y, blockWidth, blockHeight);
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

            texColors = null;
            return new Color(r / total, g / total, b / total, a / total);
        }
    }
}
