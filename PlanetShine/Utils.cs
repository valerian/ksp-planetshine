/*
* (C) Copyright 2014, Valerian Gaudeau
* 
* Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
* project is in no way associated with nor endorsed by Squad.
* 
* This code is licensed under the Apache License Version 2.0. See the LICENSE.txt
* file for more information.
*/

using System;
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
			line.SetWidth (5000, 0.1f); 
			line.SetVertexCount (2);
			return line;
		}
	}
}

