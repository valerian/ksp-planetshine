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
using UnityEngine;

namespace PlanetShine
{
    class CelestialBodyEffect
    {
        public CelestialBodySettings settings;
        public CelestialBody body;

        public Dictionary<CelestialBody, GameObject> incomingLights = new Dictionary<CelestialBody, GameObject>();


        public CelestialBodyEffect(CelestialBody body, CelestialBodySettings settings)
        {
            this.body = body;
            this.settings = settings;
            foreach (CelestialBody sourceBody in findPotentialLightSources())
            {
                GameObject newLight = new GameObject();
                newLight.AddComponent<Light>();
                newLight.light.enabled = false;
                newLight.light.type = LightType.Spot;
                newLight.light.cullingMask = (1 << 0); // TODO put the right mask
                newLight.light.shadows = LightShadows.Soft;
                newLight.light.shadowStrength = 0.8f;
                newLight.light.shadowSoftness = 20.0f;
                newLight.AddComponent<MeshRenderer>();

                incomingLights.Add(sourceBody, newLight);
            }
        }

        public CelestialBodyEffect(CelestialBody body) : this(body, CelestialBodySettings.neutral) { }

        public List<CelestialBody> findPotentialLightSources()
        {
            List<CelestialBody> bodies = new List<CelestialBody>();
            return bodies;
        }

        public void update()
        {

        }
    }
}
