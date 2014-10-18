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

		// for all of the calculations of UpdateAlbedoLights
        public CelestialBody body;
        public Color bodyColor;
		public float bodyGroundAmbient;
		public float bodyIntensity;
        public float bodyRadius;
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

        public int fixedUpdateCounter = 0; 

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
				albedoLights[i].AddComponent<MeshRenderer>();
			}
		}

		private void UpdateCelestialBody()
        {            
			body = FlightGlobals.ActiveVessel.mainBody;
			bodyColor = new Color(100f/256f,100f/256f,100f/256f);
			bodyGroundAmbient = 0.3f;
			bodyIntensity = 1.0f;

			if (config.celestialBodyInfos.ContainsKey(body)) {
				bodyColor = config.celestialBodyInfos[body].albedoColor;
				bodyIntensity = config.celestialBodyInfos[body].albedoIntensity;
				bodyGroundAmbient = config.celestialBodyInfos[body].atmosphereAmbientLevel;
			}
        }

        private void UpdateDebugLines()
        {
			if (config.debug) {
				debugLineLightDirection.SetPosition( 0, visibleLightPositionAverage );
				debugLineLightDirection.SetPosition( 1, FlightGlobals.ActiveVessel.transform.position );

				debugLineSunDirection.SetPosition( 0, FlightGlobals.Bodies[0].position );
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
                if (config.albedoLightsQuantity > 1) {
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

		private void UpdateAlbedoLights()
		{
			bodyRadius = (float) body.Radius * 0.999f;
			bodyVesselDirection = (FlightGlobals.ActiveVessel.transform.position - body.position).normalized;
			bodySunDirection = (body.name == "Sun") ?
                bodyVesselDirection
                : (Vector3) (FlightGlobals.Bodies[0].position - body.position).normalized;
			vesselBodyDistance = (float) (FlightGlobals.ActiveVessel.transform.position - body.position).magnitude;
			vesselAltitude = vesselBodyDistance - bodyRadius;
			visibleSurface = vesselAltitude / (float) (FlightGlobals.ActiveVessel.transform.position - body.position).magnitude;
			sunAngle = Vector3.Angle (bodySunDirection, bodyVesselDirection);
			visibleLightSunAngleMax = 90f + (90f * visibleSurface);
			visibleLightSunAngleMin = 90f - (90f * visibleSurface);
			visibleLightRatio = Mathf.Clamp01(((visibleLightSunAngleMax - sunAngle)
                                               / (visibleLightSunAngleMax - visibleLightSunAngleMin)));
			visibleLightAngleAverage = ((90f * visibleSurface) * (1f - (visibleLightRatio * (1f - (sunAngle / 270f)))));
			visibleLightAngleEffect = Mathf.Clamp01(1f - ((sunAngle - visibleLightAngleAverage) / 90f));
			boostedVisibleLightAngleEffect = Mathf.Clamp01(visibleLightAngleEffect + 0.3f);
			visibleLightPositionAverage = body.position + Vector3.RotateTowards(bodyVesselDirection, bodySunDirection,
                                                                                visibleLightAngleAverage * 0.01745f, 0.0f) * bodyRadius;
			atmosphereReflectionEffect = Mathf.Clamp01((1f - bodyGroundAmbient) +
                                                       ((vesselAltitude - (bodyRadius * config.minAlbedoFadeAltitude))
                                                        / (bodyRadius * (config.maxAlbedoFadeAltitude
                                                                         - config.minAlbedoFadeAltitude))));
			atmosphereAmbientRatio = 1f - Mathf.Clamp01((vesselAltitude - (bodyRadius * config.minAmbientFadeAltitude))
                                                        / (bodyRadius * (config.maxAmbientFadeAltitude
                                                                         - config.minAmbientFadeAltitude)));
			atmosphereAmbientEffect = bodyGroundAmbient * config.baseGroundAmbient * atmosphereAmbientRatio;
            areaSpreadAngle = Math.Min(45f, (visibleLightRatio * (1f - (sunAngle / 270f)))
                                       * Mathf.Rad2Deg * (float) Math.Acos(Math.Sqrt((vesselBodyDistance * vesselBodyDistance)
                                                                                     - (bodyRadius * bodyRadius))
                                                                           / vesselBodyDistance));
			areaSpreadAngleRatio = Mathf.Clamp01(areaSpreadAngle / 45f);
			lightRange = bodyRadius * config.albedoRange;
			vesselLightRangeRatio = (float) vesselAltitude / lightRange;
			lightDistanceEffect = 1.0f / (1.0f + 25.0f * vesselLightRangeRatio * vesselLightRangeRatio);
			visibleLightVesselDirection = (FlightGlobals.ActiveVessel.transform.position - visibleLightPositionAverage).normalized;


			lightIntensity = config.baseAlbedoIntensity / config.albedoLightsQuantity;
			lightIntensity *= visibleLightRatio * boostedVisibleLightAngleEffect * atmosphereReflectionEffect
                * lightDistanceEffect * bodyIntensity;
            if (config.albedoLightsQuantity > 1 )
                lightIntensity *= 1f + (areaSpreadAngleRatio * areaSpreadAngleRatio * 0.5f);
            
			int i = 0;
			foreach (GameObject albedoLight in albedoLights){
                albedoLight.light.renderMode = config.useVertex ? LightRenderMode.ForceVertex : LightRenderMode.ForcePixel;
				albedoLight.light.intensity = lightIntensity;
				albedoLight.light.transform.forward = visibleLightVesselDirection;
				if (config.albedoLightsQuantity > 1 ) { // Spread the lights, but only if there are more than one
					albedoLight.light.transform.forward = Quaternion.AngleAxis (areaSpreadAngle,
                                                                                Vector3.Cross (bodyVesselDirection,
                                                                                               bodySunDirection).normalized)
                        * albedoLight.light.transform.forward;
					albedoLight.light.transform.forward = Quaternion.AngleAxis (i * (360f / config.albedoLightsQuantity),
                                                                                visibleLightVesselDirection)
                        * albedoLight.light.transform.forward;
				}
                      
				albedoLight.light.color = bodyColor;
				albedoLight.light.enabled = renderEnabled && (i < config.albedoLightsQuantity);
				i++;
			}
		}

        private void UpdateAmbientLights()
        {
            if (ambientLight != null) {
				vacuumColor.r = vacuumColor.g = vacuumColor.b = config.vacuumLightLevel;
				ambientLight.vacuumAmbientColor = vacuumColor;
				if (renderEnabled) {
					ambientLight.vacuumAmbientColor += atmosphereAmbientEffect * visibleLightAngleEffect * bodyColor;
                    RenderSettings.ambientLight = (ambientLight.vacuumAmbientColor * config.groundAmbientOverrideRatio)
                        + (RenderSettings.ambientLight * (1f - config.groundAmbientOverrideRatio));
                }
			}
        }


		public void Start()
		{
			if (Instance != null)
				Destroy (Instance.gameObject);
			Instance = this;

			ambientLight = FindObjectOfType(typeof(DynamicAmbientLight)) as DynamicAmbientLight;

			vacuumColor = new Color (config.vacuumLightLevel, config.vacuumLightLevel, config.vacuumLightLevel);
			if (ambientLight != null) {
				ambientLight.vacuumAmbientColor = vacuumColor;
			}

			CreateAlbedoLights ();
			CreateDebugLines ();
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
