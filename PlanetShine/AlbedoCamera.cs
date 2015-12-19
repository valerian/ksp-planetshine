using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlanetShine
{
    public class AlbedoCamera
    {
        private static int counter = 0;

        public bool inScaledSpace;
        public GameObject cameraObject;
        public Camera camera;
        public Texture2D texture;
        public RenderTexture renderTexture;
        public CelestialBody targetBody;
        public int dimension;
        public bool active;
        public int layerMask;

        public AlbedoCamera(bool inScaledSpace, int layerMask, int dimension)
        {
            counter++;
            this.inScaledSpace = inScaledSpace;
            this.layerMask = layerMask;
            this.dimension = dimension;
            this.active = false;
            Init();
        }

        private void Init()
        {
            texture = new Texture2D(dimension, dimension, TextureFormat.RGB24, false);
            renderTexture = new RenderTexture(dimension, dimension, 24);
            cameraObject = new GameObject("PlanetShineCamera_" + counter);
            camera = cameraObject.AddComponent<Camera>();
            camera.targetTexture = renderTexture;
            camera.aspect = 1;
            camera.fieldOfView = 120;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = layerMask;
            camera.nearClipPlane = 0.0001f;
            camera.farClipPlane = 200000000f;
            camera.enabled = true;
        }

        public void Activate(CelestialBody target)
        {
            targetBody = target;
            active = true;
        }

        public void Deactivate()
        {
            targetBody = null;
            active = false;
        }

        public bool Update(CelestialBody targetBody, float elevation)
        {
            if (!active)
                return false;
            if (inScaledSpace)
            {
                cameraObject.transform.position = ScaledSpace.LocalToScaledSpace(FlightGlobals.ActiveVessel.transform.position);
                cameraObject.transform.position += (cameraObject.transform.position - targetBody.scaledBody.transform.position).normalized * (ScaledSpace.InverseScaleFactor * elevation);
                cameraObject.transform.LookAt(targetBody.scaledBody.transform.position);

                float distance = (float)(cameraObject.transform.position - targetBody.scaledBody.transform.position).magnitude;
                camera.fieldOfView = 2 * Mathf.Rad2Deg * Mathf.Acos(Mathf.Sqrt(Mathf.Max((distance * distance)
                    - (float)(ScaledSpace.InverseScaleFactor * ScaledSpace.InverseScaleFactor * targetBody.Radius * targetBody.Radius), 1)) / distance);

                camera.nearClipPlane = Mathf.Max(0.001f, distance - (((float)targetBody.Radius - (float)targetBody.atmosphereDepth) * 1.5f * ScaledSpace.InverseScaleFactor));
                camera.farClipPlane = distance + ((float)targetBody.Radius * ScaledSpace.InverseScaleFactor);
            }
            else
            {
                cameraObject.transform.position = FlightGlobals.ActiveVessel.transform.position;
                cameraObject.transform.position += (cameraObject.transform.position - targetBody.transform.position).normalized * elevation;
                cameraObject.transform.LookAt(targetBody.transform.position);

                float distance = (float)(cameraObject.transform.position - targetBody.transform.position).magnitude;
                camera.fieldOfView = 2 * Mathf.Rad2Deg * Mathf.Acos(Mathf.Sqrt(Mathf.Max((distance * distance) - (float)(targetBody.Radius * targetBody.Radius), 1)) / distance);

                camera.nearClipPlane = Mathf.Max(0.001f, distance - (((float)targetBody.Radius - (float)targetBody.atmosphereDepth) * 1.5f));
                camera.farClipPlane = distance + ((float)targetBody.Radius);
            }
            return true;
        }

        public bool Render()
        {
            if (!active)
                return false;
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = renderTexture;
            camera.Render();
            texture.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            texture.Apply(false, false);
            RenderTexture.active = currentRT;
            return false;
        }
    }
}
