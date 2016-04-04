using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlanetShine
{
    public class ComputedAlbedoDeterminer : AlbedoDeterminer
    {

        protected ComputedAlbedoDeterminer()
        {

        }

        public override Color DetermineColor(CelestialBodyData sourceBodyData, Component target)
        {
            // to avoid number rounding issues, we shamelessly assume the body is 0.0001% smaller
            float bodySubRadius = (float)sourceBodyData.celestialBody.Radius * 0.9999f;
            // direction from the center of the body to the current vessel
            Vector3 bodyVesselDirection = (FlightGlobals.ActiveVessel.transform.position - sourceBodyData.celestialBody.position).normalized;
            // direction from the center of the body to the sun
            Vector3 bodySunDirection = sourceBodyData.isSun ?
                bodyVesselDirection
                : (Vector3)(Sun.Instance.sun.position - sourceBodyData.celestialBody.position).normalized;
            // distance between the current vessel and the center of the body
            float vesselBodyDistance = (float)(FlightGlobals.ActiveVessel.transform.position - sourceBodyData.celestialBody.position).magnitude;
            // altitude of the vessel, based on the radius of the body
            float vesselAltitude = Math.Max(vesselBodyDistance - bodySubRadius, 1.0f);
            // visible surface of the body as seen from the vessel, in % of the hemisphere's radius
            float visibleSurface = vesselAltitude / (float)(FlightGlobals.ActiveVessel.transform.position - sourceBodyData.celestialBody.position).magnitude;



            // angle between the sun and the vessel, relative to the center of the body
            float sunAngle = Vector3.Angle(bodySunDirection, bodyVesselDirection);
            // sunAngle value for which no more of the body's visible surface is enlightened by the sun
            float visibleLightSunAngleMax = 90f + (90f * visibleSurface);
            // sunAngle value for which 100% of the body's visible surface is enlightened by the sun
            float visibleLightSunAngleMin = 90f - (90f * visibleSurface);
            // % of the body's visible surface that is enlightened by the sun
            float visibleLightRatio = Mathf.Clamp01(((visibleLightSunAngleMax - sunAngle)
                                                      / (visibleLightSunAngleMax - visibleLightSunAngleMin)));



            // angle between the vessel and the average center of the visible surface light, relative to the body center                                                                                                                       
            float visibleLightAngleAverage = ((90f * visibleSurface) * (1f - (visibleLightRatio * (1f - (sunAngle / 180f)))));
            // approximation of the sun light intensity reduction caused by the sun angle to the body surface                                                                                                                                  
            float visibleLightAngleEffect = Mathf.Clamp01(1f - ((sunAngle - visibleLightAngleAverage) / 90f));
            // slighly increased version of visibleLightAngleEffect for a nicer rendering                                                                                                                                                      
            float boostedVisibleLightAngleEffect = Mathf.Clamp01(visibleLightAngleEffect + 0.3f);



            return sourceBodyData.albedoColor * visibleLightRatio * boostedVisibleLightAngleEffect;
        }
    }
}
