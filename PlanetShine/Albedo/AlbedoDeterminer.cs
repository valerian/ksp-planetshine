using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlanetShine
{
    public abstract class AlbedoDeterminer
    {
        public abstract Color DetermineColor(CelestialBodyData sourceBodyData, Component target);
    }
}
