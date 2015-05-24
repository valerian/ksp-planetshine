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

        public static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat)
        {
            if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
                throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");

            byte ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124)
                throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

            int height = ddsBytes[13] * 256 + ddsBytes[12];
            int width = ddsBytes[17] * 256 + ddsBytes[16];

            int DDS_HEADER_SIZE = 128;
            byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
            Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

            Texture2D texture = new Texture2D(width, height, textureFormat, false);
            texture.LoadRawTextureData(dxtBytes);
            texture.Apply();

            return texture;
        }

        public static Color GetTextureAverageColor(Texture2D texture)
        {
            try
            {
                Color[] texColors = texture.GetPixels();

                int total = texColors.Length;

                float r = 0;
                float g = 0;
                float b = 0;

                for (int i = 0; i < total; i++)
                {

                    r += texColors[i].r;

                    g += texColors[i].g;

                    b += texColors[i].b;

                }
                return new Color(r / total, g / total, b / total, 0);
            }
            catch (Exception e)
            {
                Debug.LogError(String.Format(
                    "[PlanetShine] An exception occured while extracting color from a Texture2D:\n{0}\nThe exception was:\n{1}",
                    texture,
                    e
                ));
            }
            return new Color();
        }

        public static Texture2D LoadGameTextureFromPath(string path)
        {
            
            string absolutePath = Application.dataPath + "/../GameData/" + path;
            string[] extentions = { "dds", "tga", "png", "jpg", "jpeg" };

            foreach (string extention in extentions)
            {
                try
                {
                    WWW www = new WWW("file://" + WWW.EscapeURL(absolutePath + "." + extention).Replace("%3A", ":").Replace("%2F", "/").Replace("+", "%20"));
                    while (!www.isDone)
                        System.Threading.Thread.Sleep(20);
                    if (www.error != null)
                        continue;
                    if (extention == "dds")
                    {
                        return LoadTextureDXT(www.bytes, TextureFormat.DXT1);
                    }
                    else
                    {
                        Texture2D texture = new Texture2D(2048, 1024);
                        www.LoadImageIntoTexture(texture);
                        return texture;
                    }
                }
                catch (Exception e)
                {

                }
            }

            return null;
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

