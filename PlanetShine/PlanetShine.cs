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

        // information related to the currently orbiting body
        public CelestialBody body;
        public Color bodyColor;

        public Color bodyTextureColor;
        public Color bodyAtmosphereColor;

        // TODO remove later, for debugging only
        public Texture2D bodyRimTexture = null;

        public float bodyAtmosphereAmbient;
        public float bodyGroundAmbientOverride;
        public float bodyIntensity;
        public float bodyRadius;
        public bool bodyIsSun = false;

        public bool bodyTextureLoaded = false;

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
        public float lightRange;
        public float vesselLightRangeRatio;
        public float lightDistanceEffect;
        public Vector3 visibleLightVesselDirection;
        public float lightIntensity;
        public Color vacuumColor;

        public int albedoLightsQuantity
        {
            // Sepcial case for suns: we only want 1 albedo light to correctly handle the shadow.
            get
            {
                return bodyIsSun ? 1 : config.albedoLightsQuantity;
            }
        }

        // loop counter used for performance optimizations
        public int fixedUpdateCounter = 0;




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
                albedoLights[i].light.cullingMask = (1 << 0);
                albedoLights[i].light.shadows = LightShadows.Soft;
                albedoLights[i].light.shadowStrength = 1.0f;
                albedoLights[i].light.shadowSoftness = 20.0f;
                albedoLights[i].AddComponent<MeshRenderer>();
            }
        }

        // Find current celestial body info and color in config, or use default neutral settings
        private void UpdateCelestialBody()
        {
            //TODO seriously clean this big mess, it's a priority
            //TODO try to find a temporary alternative color to unloaded textures
            if (body == FlightGlobals.ActiveVessel.mainBody &&
                (bodyTextureLoaded || Sun.Instance.sun == body || body.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length > 0))
                return;

            bodyTextureLoaded = false;
            body = FlightGlobals.ActiveVessel.mainBody;
            bodyColor = new Color(100f/256f,100f/256f,100f/256f);
            bodyTextureColor = new Color(100f / 256f, 100f / 256f, 100f / 256f);
            bodyAtmosphereColor = new Color(100f / 256f, 100f / 256f, 100f / 256f);
            bodyGroundAmbientOverride = 1.0f;
            
            if (bodyRimTexture != null)
            {
                Destroy(bodyRimTexture);
                bodyRimTexture = null;
            }

            if (Sun.Instance.sun == body || body.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length > 0)
            {
                //TODO extract color from shader and/or sunlight
                bodyAtmosphereAmbient = 0.2f;
                bodyIntensity = 6.0f;
                bodyIsSun = true;
            }
            else
            {
                if (body.scaledBody.renderer.sharedMaterial.mainTexture == null)
                {
                    Logger.Log("No Scaled Space texture found, looking for Kopernicus ScaledSpaceDemand");
                    var scaledSpaceDemand = body.scaledBody.GetComponent("ScaledSpaceDemand");
                    if (scaledSpaceDemand == null)
                    {
                        Logger.Log("Kopernicus ScaledSpaceDemand not found");
                        return;
                    }
                    scaledSpaceDemand.GetType().GetMethod("OnBecameVisible", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(scaledSpaceDemand, null);
                    Logger.Log("Succesfully triggered Kopernicus ScaledSpaceDemand");
                }

                if (body.scaledBody.renderer.sharedMaterial.mainTexture == null)
                {
                    Logger.Log("Still no texture!");
                    return;
                }

                bodyTextureLoaded = true;
                bodyTextureColor = Utils.GetUnreadableTextureAverageColor((Texture2D)body.scaledBody.renderer.sharedMaterial.mainTexture);
                bodyColor = bodyTextureColor;

                if (body.atmosphere)
                {
                    bodyAtmosphereAmbient = 1.0f;
                    if (body.scaledBody.renderer.sharedMaterial.GetTexture("_rimColorRamp") != null)
                    {
                        bodyRimTexture = Utils.CreateReadable((Texture2D)body.scaledBody.renderer.sharedMaterial.GetTexture("_rimColorRamp"));
                        bodyAtmosphereColor = Utils.GetRimOuterColor(bodyRimTexture, 0.2f);
                        bodyColor = (bodyColor * 0.6f) + (bodyAtmosphereColor * 0.4f);
                        bodyColor.a = 1.0f;
                    }
                }
                else
                {
                    bodyAtmosphereAmbient = 0.2f;
                }
                bodyIntensity = 1.0f;
                bodyIsSun = false;
            }

            //TODO set on a per-attribute override
            if (config.celestialBodyInfos.ContainsKey(body)) {
                bodyColor = config.celestialBodyInfos[body].albedoColor;
                bodyIntensity = config.celestialBodyInfos[body].albedoIntensity;
                bodyAtmosphereAmbient = config.celestialBodyInfos[body].atmosphereAmbientLevel;
                bodyGroundAmbientOverride = config.celestialBodyInfos[body].groundAmbientOverride;
                bodyIsSun = config.celestialBodyInfos[body].isSun | bodyIsSun;
            }
        }

        private void UpdateDebugLines()
        {
            if (config.debug) {
                debugLineLightDirection.SetPosition( 0, visibleLightPositionAverage );
                debugLineLightDirection.SetPosition( 1, FlightGlobals.ActiveVessel.transform.position );

                debugLineSunDirection.SetPosition(0, Sun.Instance.sun.position);
                debugLineSunDirection.SetPosition( 1, FlightGlobals.ActiveVessel.transform.position );

                debugLineBodyDirection.SetPosition( 0, body.position );
                debugLineBodyDirection.SetPosition( 1, FlightGlobals.ActiveVessel.transform.position );

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
            // TODO use ACTUAL atmosphere height! Maybe use atmosphere gradient as well
            // TODO try to simplify

            // reminder: "body" means celestial body, which is the currently orbiting planet/moon/sun
            // to avoid number rounding issues, we shamelessly assume the body is 0.1% smaller
            bodyRadius = (float) body.Radius * 0.999f;
            // direction from the center of the body to the current vessel
            bodyVesselDirection = (FlightGlobals.ActiveVessel.transform.position - body.position).normalized;
            // direction from the center of the body to the sun
            bodySunDirection = bodyIsSun ?
                bodyVesselDirection
                : (Vector3)(Sun.Instance.sun.position - body.position).normalized;
            // distance between the current vessel and the center of the body
            vesselBodyDistance = (float) (FlightGlobals.ActiveVessel.transform.position - body.position).magnitude;
            // altitude of the vessel, based on the radius of the body
            vesselAltitude = Math.Max(vesselBodyDistance - bodyRadius, 1.0f);
            // visible surface of the body as seen from the vessel, in % of the hemisphere's radius
            visibleSurface = vesselAltitude / (float) (FlightGlobals.ActiveVessel.transform.position - body.position).magnitude;
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
            // approximation of the sun light intensity reduction caused by the sun angle to the body surface
            visibleLightAngleEffect = Mathf.Clamp01(1f - ((sunAngle - visibleLightAngleAverage) / 90f));
            // slighly increased version of visibleLightAngleEffect for a nicer rendering
            boostedVisibleLightAngleEffect = Mathf.Clamp01(visibleLightAngleEffect + 0.3f);
            // average direction from which the albedo light comes to the vessel, using most of the previous angular calculations
            visibleLightPositionAverage = body.position + Vector3.RotateTowards(bodyVesselDirection, bodySunDirection,
                                                                                visibleLightAngleAverage * 0.01745f, 0.0f) * bodyRadius;
            // albedo light intensity modificator caused by vessel altitude
            atmosphereReflectionRatio = Mathf.Clamp01((vesselAltitude - (bodyRadius * config.minAlbedoFadeAltitude))
                                                      / (bodyRadius * (config.maxAlbedoFadeAltitude
                                                                       - config.minAlbedoFadeAltitude)));
            // albedo light intensity modificator caused by attenuation within atmosphere
            atmosphereReflectionEffect = Mathf.Clamp01((1f - bodyAtmosphereAmbient) + atmosphereReflectionRatio);
            // atmosphere ambient light intensity modificator based on altitude and atmosphere data
            atmosphereAmbientRatio = 1f - Mathf.Clamp01((vesselAltitude - (bodyRadius * config.minAmbientFadeAltitude))
                                                        / (bodyRadius * (config.maxAmbientFadeAltitude
                                                                         - config.minAmbientFadeAltitude)));
            // atmosphere ambient light intensity modificator based on several combined settings
            atmosphereAmbientEffect = bodyAtmosphereAmbient * config.baseGroundAmbient * atmosphereAmbientRatio;
            // approximation of the angle corresponding to the visible size of the enlightened aread of the body, relative to the vessel
            areaSpreadAngle = Math.Min(60f, (visibleLightRatio * (1f - (sunAngle / 180f)))
                                       * Mathf.Rad2Deg * (float) Math.Acos(Math.Sqrt(Math.Max((vesselBodyDistance * vesselBodyDistance)
                                                                                     - (bodyRadius * bodyRadius), 1.0f))
                                                                           / vesselBodyDistance));
            // % of the area spread angle, from 0 degrees to 60 degrees
            areaSpreadAngleRatio = Mathf.Clamp01(areaSpreadAngle / 60f);
            // max range of the albedo effect, based on the body radius and the settings
            lightRange = bodyRadius * config.albedoRange;
            // albedo light intensity modificator caused by the distance from the body, and based on the max range
            vesselLightRangeRatio = (float) vesselAltitude / lightRange;
            // final modificator for albedo light intensity based on the distance form the body, with a scale adapted to computer screen's light rendering that lacks of dynamic range
            lightDistanceEffect = 1.0f / (1.0f + 25.0f * vesselLightRangeRatio * vesselLightRangeRatio);
            // direction of the albedo light relative to the vessel
            visibleLightVesselDirection = (FlightGlobals.ActiveVessel.transform.position - visibleLightPositionAverage).normalized;

            // combining all previous albedo light modificators to set the final intensity
            lightIntensity = config.baseAlbedoIntensity / albedoLightsQuantity;
            lightIntensity *= visibleLightRatio * boostedVisibleLightAngleEffect * atmosphereReflectionEffect
                * lightDistanceEffect * bodyIntensity;

            // boosting light intensity when there are multiple rendering lights spread with a wide angle
            // TODO check if it's still fine with the new 60 degree max (was 45 before)
            if (albedoLightsQuantity > 1 )
                lightIntensity *= 1f + (areaSpreadAngleRatio * areaSpreadAngleRatio * 0.5f);
            
            int i = 0;
            foreach (GameObject albedoLight in albedoLights){
                if (config.useVertex && !bodyIsSun)
                    albedoLight.light.renderMode = LightRenderMode.ForceVertex;
                else
                    albedoLight.light.renderMode = LightRenderMode.ForcePixel;
                albedoLight.light.intensity = lightIntensity;
                albedoLight.light.transform.forward = visibleLightVesselDirection;
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
                      
                albedoLight.light.color = bodyColor;
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
                    //TODO bring back ambient light mixed with vacuumColor, because ambient light only works on very low altitudes
                    //TODO find ambientlight fading curve
                    RenderSettings.ambientLight = RenderSettings.ambientLight *
                        (1f - (config.groundAmbientOverrideRatio * bodyGroundAmbientOverride));
                    RenderSettings.ambientLight += (atmosphereAmbientEffect * visibleLightAngleEffect * bodyColor) *
                        (config.groundAmbientOverrideRatio * bodyGroundAmbientOverride);
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
            UpdateAmbientLights();
        }
    }
}
