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

        public static float TryParse(string s, float target)
        {
            float.TryParse(s, out target);
            return target;
        }

        public static int TryParse(string s, int target)
        {
            int.TryParse(s, out target);
            return target;
        }

        public static bool TryParse(string s, bool target)
        {
            bool.TryParse(s, out target);
            return target;
        }

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

        public static bool DoTexturesSizeMatch(List<Texture2D> textures)
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

