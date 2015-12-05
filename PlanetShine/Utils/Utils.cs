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
using System.Linq;
using UnityEngine;

namespace PlanetShine
{
    public class Utils
    {
        public static LineRenderer CreateDebugLine(Color startColor, Color endColor)
        {
            GameObject obj = new GameObject ("Line");
            LineRenderer line = obj.AddComponent< LineRenderer > ();
            line.material = new Material (Shader.Find ("Particles/Additive"));
            line.SetColors (startColor, endColor);
            line.SetWidth (0.05f, 0.05f); 
            line.SetVertexCount (2);
            return line;
        }

        public static Color GetUnreadableTextureAverageColor(Texture2D texture)
        {
            Texture2D readableTexture = CreateReadable(texture);
            Logger.Log("RAM usage before GetTextureAverageColor: " + Config.Instance.ramUsage + " MB");
            Color color = GetTextureAverageColor(readableTexture);
            Logger.Log("RAM usage after GetTextureAverageColor and before Texture2D.DestroyImmediate(readableTexture): " + Config.Instance.ramUsage + " MB");
            Texture2D.DestroyImmediate(readableTexture);
            Logger.Log("RAM usage after Texture2D.DestroyImmediate(readableTexture): " + Config.Instance.ramUsage + " MB");
            return color;
        }

        public static Color GetPixelsAverageColor(Color[] texColors)
        {
            Logger.Log("RAM usage at start of GetPixelsAverageColor: " + Config.Instance.ramUsage + " MB");
            int total = texColors.Length;

            float r = 0;
            float g = 0;
            float b = 0;

            foreach (Color pixel in texColors)
            {
                r += pixel.r;
                g += pixel.g;
                b += pixel.b;
            }

            Logger.Log("RAM usage at end of GetPixelsAverageColor: " + Config.Instance.ramUsage + " MB");

            return new Color(r / total, g / total, b / total, 1.0f);
        }

        public static float GetPixelsAverageAlpha(Color[] texColors)
        {
            int total = texColors.Length;

            float alpha = 0;

            foreach (Color pixel in texColors)
            {
                alpha += pixel.a;
            }
            return alpha / total;
        }

        public static Color GetTextureAverageColor(Texture2D texture)
        {
            Logger.Log("RAM usage at start of GetTextureAverageColor: " + Config.Instance.ramUsage + " MB");
            return GetPixelsAverageColor(texture.GetPixels());
        }

        public static float GetTextureAverageAlpha(Texture2D texture)
        {
            return GetPixelsAverageAlpha(texture.GetPixels());
        }

        public static Color GetRimOuterColor(Texture2D texture, float fraction)
        {
            return GetPixelsAverageColor(texture.GetPixels(0, 0, (int) Math.Round(texture.width * fraction), texture.height));
        }

        public static bool DoTexturesMatch(Texture2D[] textures)
        {
            int width = 0;
            int height = 0;
            bool firstPass = true;

            foreach (Texture2D texture in textures)
            {
                if (firstPass)
                    firstPass = false;
                else
                    if (texture.width != width || texture.height != height)
                        return false;
                width = texture.width;
                height = texture.height;
            }

            return true;
        }

        public static float GetTexturesCombinedAlpha(Texture2D[] textures)
        {
            if (textures.Length == 0 ||
                !DoTexturesMatch(textures))
                throw new Exception("Trying to combine non matching textures");

            Color[][] pixels = new Color[textures.Length][];
            int k = 0;

            for (int i = 0; i < textures.Length; i++)
                pixels[i] = textures[i].GetPixels();

            int length = pixels[0].Length;

            float totalAlpha = 0;
            float tmpAlpha = 0;


            for (int j = 0; j < length; j++ )
            {
                tmpAlpha = 0;
                for (k = 0; k < pixels.Length; k++)
                    if (pixels[k][j].a > tmpAlpha)
                        tmpAlpha = pixels[k][j].a;
                totalAlpha += tmpAlpha;
            }

            return totalAlpha / length;
        }


        public static Texture2D CreateReadable(Texture2D original)
        {
            Logger.Log("RAM usage before CreateReadable: " + Config.Instance.ramUsage + " MB");

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

            Logger.Log("RAM usage at the end of CreateReadable: " + Config.Instance.ramUsage + " MB");

            // Return
            return finalTexture;
        }

        public static Type FindType(string fullName)
        {
            return
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName.Equals(fullName));
        }

        public static Type FindTypeContains(string partialName)
        {
            return
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName.Contains(partialName));
        }
    }


    public class DisplaySettingOption<T>
    {
        public string label { get; private set; }
        public T value { get; private set; }

        public DisplaySettingOption(string label, T value)
        {
            this.label = label;
            this.value = value;
        }
    }


}

