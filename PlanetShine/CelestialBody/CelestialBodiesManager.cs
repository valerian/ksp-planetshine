using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlanetShine
{
    public class CelestialBodiesManager
    {
        Dictionary<CelestialBody, CelestialBodyData> bodies;

        public CelestialBodiesManager()
        {
            bodies = new Dictionary<CelestialBody, CelestialBodyData>();
        }

        public CelestialBodyData GetBody(CelestialBody celestialBody)
        {
            if (!bodies.ContainsKey(celestialBody))
                bodies[celestialBody] = new CelestialBodyData(celestialBody);
            return bodies[celestialBody];
        }
    }
}
