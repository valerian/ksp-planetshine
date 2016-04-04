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
using System.Reflection;

namespace PlanetShine
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class PlanetShine : MonoBehaviour
    {
        // local attributes
        private Config config = Config.Instance;
        public static PlanetShine Instance { get; private set; }
        public static GameObject[] albedoLights;
        public static DynamicAmbientLight ambientLight;

        public static bool renderEnabled = true;

        // for debug only
        private System.Diagnostics.Stopwatch performanceTimer = new System.Diagnostics.Stopwatch();
        public double performanceTimerLast = 0;

        public static LineRenderer debugLineLightDirection = null;
        public static LineRenderer debugLineSunDirection = null;
        public static LineRenderer debugLineBodyDirection = null;

        public static LineRenderer[] debugLineLights = null;

        // Albedo renderer
        public VisualAlbedoDeterminer albedo;
        public float bodyFov = 120f;

        // information related to the currently orbiting body
        public CelestialBodiesManager celestialBodiesManager = new CelestialBodiesManager();
        public CelestialBodyData body;
        public float bodySubRadius;

        // data for calculating and rendering albedo and ambient lights
        public Vector3 bodyVesselDirection;
        public Vector3 bodySunDirection;
        public float vesselBodyDistance;
        public float vesselAltitude;
        public float visibleSurface;
        public float sunAngle;
        public float visibleLightSunAngleMax;
        public float visibleLightSunAngleMin;
        public float visibleLightRatio;
        public float visibleLightAngleAverage;
        public float visibleLightAngleEffect;
        public float boostedVisibleLightAngleEffect;
        public Vector3 visibleLightPositionAverage;
        public float atmosphereReflectionRatio;
        public float atmosphereReflectionEffect;
        public float atmosphereAmbientRatio;
        public float atmosphereAmbientEffect;
        public float areaSpreadAngle;
        public float areaSpreadAngleRatio;
        public float lightDistanceEffect;
        public Vector3 visibleLightVesselDirection;
        public float lightIntensity;
        public Color vacuumColor;

        public int albedoLightsQuantity
        {
            // Sepcial case for suns: we only want 1 albedo light to correctly handle the shadow.
            get
            {
                return body.isSun ? 1 : config.albedoLightsQuantity;
            }
        }

        // loop counter used for performance optimizations
        public int fixedUpdateCounter = 0;
        public int frameCounter = 0;




        public void Awake()
        {
            if (Instance != null)
                Destroy(Instance.gameObject);
            Instance = this;
        }

        public void Start()
        {
            ambientLight = FindObjectOfType(typeof(DynamicAmbientLight)) as DynamicAmbientLight;

            vacuumColor = new Color(config.vacuumLightLevel, config.vacuumLightLevel, config.vacuumLightLevel);
            if (ambientLight != null)
            {
                ambientLight.vacuumAmbientColor = vacuumColor;
            }

            CreateAlbedoLights();
            CreateDebugLines();
            albedo = new VisualAlbedoDeterminer(64);
            albedo.elevation = 10000f;
        }

        private void CreateDebugLines()
        {
            debugLineLightDirection = Utils.CreateDebugLine(Color.white, Color.green);
            debugLineSunDirection = Utils.CreateDebugLine(Color.white, Color.yellow);
            debugLineBodyDirection = Utils.CreateDebugLine(Color.white, Color.red);
            debugLineLights = new LineRenderer[Config.maxAlbedoLightsQuantity];
            for (var i = 0; i < Config.maxAlbedoLightsQuantity; i++) {
                debugLineLights[i] = Utils.CreateDebugLine(Color.white, Color.blue);
            }
        }

        private void CreateAlbedoLights()
        {
            albedoLights = new GameObject[Config.maxAlbedoLightsQuantity]; 
            for (var i = 0; i < Config.maxAlbedoLightsQuantity; i++){
                if (albedoLights[i] != null)
                    Destroy (albedoLights[i]);
                albedoLights[i] = new GameObject();
                albedoLights[i].AddComponent<Light>();
                albedoLights[i].light.type = LightType.Directional;
                albedoLights[i].light.cullingMask = LayerMask.Default;
                albedoLights[i].light.shadows = LightShadows.Soft;
                albedoLights[i].light.shadowStrength = 1.0f;
                albedoLights[i].light.shadowSoftness = 20.0f;
                albedoLights[i].AddComponent<MeshRenderer>();
            }
        }

        // Find current celestial body info and color in config, or use default neutral settings
        private void UpdateCelestialBody()
        {
            if (body == null || body.celestialBody != FlightGlobals.ActiveVessel.mainBody)
                body = celestialBodiesManager.GetBody(FlightGlobals.ActiveVessel.mainBody);
        }

        private void UpdateDebugLines()
        {
            if (config.debug) {
                debugLineLightDirection.SetPosition(0, visibleLightPositionAverage );
                debugLineLightDirection.SetPosition(1, FlightGlobals.ActiveVessel.transform.position );

                debugLineSunDirection.SetPosition(0, Sun.Instance.sun.position);
                debugLineSunDirection.SetPosition(1, FlightGlobals.ActiveVessel.transform.position );

                debugLineBodyDirection.SetPosition(0, body.celestialBody.position);
                debugLineBodyDirection.SetPosition(1, FlightGlobals.ActiveVessel.transform.position );

            }

            debugLineLightDirection.enabled = config.debug;
            debugLineBodyDirection.enabled = config.debug;
            debugLineSunDirection.enabled = config.debug;
            foreach (LineRenderer line in debugLineLights)
                line.enabled = false;

            int i = 0;
            foreach (GameObject albedoLight in albedoLights) {
                if (albedoLightsQuantity > 1) {
                    debugLineLights[i].enabled = config.debug;
                    debugLineLights[i].SetPosition( 0, FlightGlobals.ActiveVessel.transform.position
                                                    - albedoLight.light.transform.forward * 10000 );
                    debugLineLights[i].SetPosition( 1, FlightGlobals.ActiveVessel.transform.position );                           
                } else {
                    debugLineLights[1].enabled = config.debug;
                    debugLineLights[1].SetPosition( 0, FlightGlobals.ActiveVessel.transform.position
                                                    - albedoLight.light.transform.forward * 10000 );
                    debugLineLights[1].SetPosition( 1, FlightGlobals.ActiveVessel.transform.position );         
                }
                i++;
            }

        }

        // thise is where all the calculation and rendering of albedo lights occur
        private void UpdateAlbedoLights()
        {
            // TODO try to simplify

            // reminder: "body" means celestial body, which is the currently orbiting planet/moon/sun
            // to avoid number rounding issues, we shamelessly assume the body is 0.1% smaller
            bodySubRadius = (float) body.celestialBody.Radius * 0.9999f;
            // direction from the center of the body to the current vessel
            bodyVesselDirection = (FlightGlobals.ActiveVessel.transform.position - body.celestialBody.position).normalized;
            // direction from the center of the body to the sun
            bodySunDirection = body.isSun ?
                bodyVesselDirection
                : (Vector3)(Sun.Instance.sun.position - body.celestialBody.position).normalized;
            // distance between the current vessel and the center of the body
            vesselBodyDistance = (float)(FlightGlobals.ActiveVessel.transform.position - body.celestialBody.position).magnitude;
            // altitude of the vessel, based on the radius of the body
            vesselAltitude = Math.Max(vesselBodyDistance - bodySubRadius, 1.0f);
            // visible surface of the body as seen from the vessel, in % of the hemisphere's radius
            visibleSurface = vesselAltitude / (float)(FlightGlobals.ActiveVessel.transform.position - body.celestialBody.position).magnitude;
            // angle between the sun and the vessel, relative to the center of the body
            sunAngle = Vector3.Angle (bodySunDirection, bodyVesselDirection);
            // sunAngle value for which no more of the body's visible surface is enlightened by the sun
            visibleLightSunAngleMax = 90f + (90f * visibleSurface);
            // sunAngle value for which 100% of the body's visible surface is enlightened by the sun
            visibleLightSunAngleMin = 90f - (90f * visibleSurface);
            // % of the body's visible surface that is enlightened by the sun
            visibleLightRatio = Mathf.Clamp01(((visibleLightSunAngleMax - sunAngle)
                                               / (visibleLightSunAngleMax - visibleLightSunAngleMin)));
            // angle between the vessel and the average center of the visible surface light, relative to the body center
            visibleLightAngleAverage = ((90f * visibleSurface) * (1f - (visibleLightRatio * (1f - (sunAngle / 180f)))));
            // average direction from which the albedo light comes to the vessel, using most of the previous angular calculations
            visibleLightPositionAverage = body.celestialBody.position + Vector3.RotateTowards(bodyVesselDirection, bodySunDirection,
                                                                                visibleLightAngleAverage * 0.01745f, 0.0f) * bodySubRadius;
            
            

            // albedo light intensity modificator caused by vessel altitude
            atmosphereReflectionRatio = Mathf.Clamp01((vesselAltitude - (body.virtualAtmosphereDepth * config.minAlbedoFadeAltitude))
                                                      / (body.virtualAtmosphereDepth * (config.maxAlbedoFadeAltitude
                                                                       - config.minAlbedoFadeAltitude)));

            // albedo light intensity modificator caused by attenuation within atmosphere
            atmosphereReflectionEffect = Mathf.Clamp01((1f - body.atmosphereAmbientLevel) + atmosphereReflectionRatio);
            
            
            
            // atmosphere ambient light intensity modificator based on altitude and atmosphere data
            atmosphereAmbientRatio = 1f - Mathf.Clamp01((vesselAltitude - (body.virtualAtmosphereDepth * config.minAmbientFadeAltitude))
                                                        / (body.virtualAtmosphereDepth * (config.maxAmbientFadeAltitude
                                                                         - config.minAmbientFadeAltitude)));
            // atmosphere ambient light intensity modificator based on several combined settings
            atmosphereAmbientEffect = body.atmosphereAmbientLevel * config.baseGroundAmbient * atmosphereAmbientRatio;



            // approximation of the angle corresponding to the visible size of the enlightened aread of the body, relative to the vessel
            bodyFov = 2 * Mathf.Rad2Deg * (float)Math.Acos(Math.Sqrt(Math.Max((vesselBodyDistance * vesselBodyDistance)
                                                                         - (bodySubRadius * bodySubRadius), 1.0f))
                                                               / vesselBodyDistance);
            
            // getting a practical angle value to use for the albedo lights directions
            areaSpreadAngle = Math.Min(45f, (visibleLightRatio * (1f - (sunAngle / 180f))) * bodyFov / 2);

            // % of the area spread angle, from 0 degrees to 45 degrees
            areaSpreadAngleRatio = Mathf.Clamp01(areaSpreadAngle / 45f);


            // max range of the albedo effect, based on the body radius and the settings
            //lightRange = bodySubRadius * config.albedoRange;
            // albedo light intensity modificator caused by the distance from the body, and based on the max range
            //vesselLightRangeRatio = (float) vesselAltitude / lightRange;
            // final modificator for albedo light intensity based on the distance form the body, with a scale adapted to computer screen's light rendering that lacks of dynamic range
            //lightDistanceEffect = 1.0f / (1.0f + 25.0f * vesselLightRangeRatio * vesselLightRangeRatio);
            /*lightDistanceEffect = 1.0f / 
                (
                1.0f
                + (2 * (vesselAltitude / bodySubRadius))
                + ((vesselAltitude * vesselAltitude) / (bodySubRadius * bodySubRadius))
                );*/
            /*
            lightDistanceEffect =
                ((1.0f - config.curvesMixRatio)
                / (1.0f + (vesselAltitude / (bodySubRadius * config.nearCurveStrength))))
                + (config.curvesMixRatio
                / (1.0f + ((vesselAltitude * vesselAltitude * vesselAltitude) / (bodySubRadius * bodySubRadius * bodySubRadius * config.farCurveStrength))));
             */
            lightDistanceEffect =
                ((1.0f - config.curvesMixRatio)
                / (1.0f + (vesselAltitude / (bodySubRadius * config.farCurveStrength))))
                + (config.curvesMixRatio
                / (1.0f + ((2 * vesselAltitude) / (bodySubRadius * config.nearCurveStrength)) + ((vesselAltitude * vesselAltitude) / (bodySubRadius * bodySubRadius * config.nearCurveStrength * config.nearCurveStrength))));
            // direction of the albedo light relative to the vessel
            visibleLightVesselDirection = (FlightGlobals.ActiveVessel.transform.position - visibleLightPositionAverage).normalized;

            // combining all previous albedo light modificators to set the final intensity
            /*
            lightIntensity = config.baseAlbedoIntensity / albedoLightsQuantity;
            lightIntensity *= visibleLightRatio * boostedVisibleLightAngleEffect * atmosphereReflectionEffect
                * lightDistanceEffect * body.albedoIntensity;
             * 
             * diff -> visibleLightRatio * boostedVisibleLightAngleEffect
             * */
            //lightIntensity = (config.baseAlbedoIntensity / albedoLightsQuantity) * body.albedoIntensity;

            lightIntensity = (config.baseAlbedoIntensity / albedoLightsQuantity) * atmosphereReflectionEffect * lightDistanceEffect * body.albedoIntensity;

            // boosting light intensity when there are multiple rendering lights spread with a wide angle
            // TODO find a formula for 60 degrees!
            if (albedoLightsQuantity > 1 )
                lightIntensity *= 1f + (areaSpreadAngleRatio * areaSpreadAngleRatio * 0.5f);
            
            int i = 0;
            foreach (GameObject albedoLight in albedoLights){
                if (config.useVertex && !body.isSun)
                    albedoLight.light.renderMode = LightRenderMode.ForceVertex;
                else
                    albedoLight.light.renderMode = LightRenderMode.ForcePixel;
                albedoLight.light.intensity = lightIntensity;
                albedoLight.light.transform.forward = visibleLightVesselDirection;
                // TODO: use ViewportToWorldPoint for consistent light directions
                // Spread the lights, but only if there are more than one
                if (albedoLightsQuantity > 1 ) { 
                    albedoLight.light.transform.forward = Quaternion.AngleAxis (areaSpreadAngle,
                                                                                Vector3.Cross (bodyVesselDirection,
                                                                                               bodySunDirection).normalized)
                        * albedoLight.light.transform.forward;
                    albedoLight.light.transform.forward = Quaternion.AngleAxis (i * (360f / albedoLightsQuantity),
                                                                                visibleLightVesselDirection)
                        * albedoLight.light.transform.forward;
                }
                      
                albedoLight.light.color = body.albedoColor;
                if (renderEnabled && (i < albedoLightsQuantity) && !MapView.MapIsEnabled) {
                    albedoLight.light.enabled = true;
                } else
                    albedoLight.light.enabled = false;
                i++;
            }
        }

        private void UpdateAmbientLights()
        {
            if (ambientLight != null) {
                vacuumColor.r = vacuumColor.g = vacuumColor.b = config.vacuumLightLevel;
                ambientLight.vacuumAmbientColor = vacuumColor;
                if (renderEnabled && !MapView.MapIsEnabled)
                {
                    // Reducing the stock ambientlight to make place for our ambientlight part, according to the ratio
                    RenderSettings.ambientLight = RenderSettings.ambientLight *
                        (1f - (config.groundAmbientOverrideRatio));
                    // Adding our ambientlight according to the ratio
                    RenderSettings.ambientLight += (atmosphereAmbientEffect * body.albedoColor) *
                        (config.groundAmbientOverrideRatio);
                }
            }
        }

        public void FixedUpdate()
        {
            if ((fixedUpdateCounter++ % config.updateFrequency) != 0)
                return;
            if (FlightGlobals.ActiveVessel == null)
                return;
            
            if (config.debug) {
                performanceTimer.Reset();
                performanceTimer.Start();
            }

            UpdateCelestialBody();

            UpdateAlbedoLights();
            UpdateDebugLines();

            if (config.debug) {
                performanceTimer.Stop();
                performanceTimerLast = performanceTimer.Elapsed.TotalMilliseconds;
            }
        }
        
        public void LateUpdate()
        {
            if (FlightGlobals.ActiveVessel == null)
                return;
            
            UpdateAmbientLights();

            if ((frameCounter++ % 3) != 0)
                return;
            if (!renderEnabled)
                return;
            body.albedoColor = albedo.DetermineColor(body, FlightGlobals.ActiveVessel);
        }
    }
}
