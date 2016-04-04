using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlanetShine
{
    public static class ColorExtensions
    {
        public static float Intensity(this Color color)
        {
            return Mathf.Max(color.r, color.g, color.b);
        }
    }
}
