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
    public class CelestialBodyInfo
    {
        public Color albedoColor;
        public float albedoIntensity;
        public float atmosphereAmbientLevel;
        public float groundAmbientOverride;
        public bool isSun;

        public CelestialBodyInfo (Color albedoColor, float albedoIntensity, float atmosphereAmbientLevel, float groundAmbientOverride, bool isSun)
        {
            this.albedoColor = albedoColor;
            this.albedoIntensity = albedoIntensity;
            this.atmosphereAmbientLevel = atmosphereAmbientLevel;
            this.groundAmbientOverride = groundAmbientOverride;
            this.isSun = isSun;
        }
    }
}

