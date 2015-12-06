using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlanetShine
{
    public class CelestialBodiesManager
    {
        Dictionary<CelestialBody, Body> bodies;

        public CelestialBodiesManager()
        {
            bodies = new Dictionary<CelestialBody, Body>();
        }

        public Body GetBody(CelestialBody celestialBody)
        {
            if (!bodies.ContainsKey(celestialBody))
                bodies[celestialBody] = new Body(celestialBody);
            return bodies[celestialBody];
        }
    }
}
