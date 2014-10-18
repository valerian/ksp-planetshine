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

