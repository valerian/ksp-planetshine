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

        public CelestialBody sourceBody;
        public Component target;

        public int dimension;
        public int layerMask;

        public AlbedoCamera(bool inScaledSpace, int layerMask, int dimension)
        {
            counter++;
            this.inScaledSpace = inScaledSpace;
            this.layerMask = layerMask;
            this.dimension = dimension;
            Init();
        }

        ~AlbedoCamera()
        {
            Texture2D.Destroy(texture);
            texture = null;
            camera = null;
            renderTexture = null;
            cameraObject.DestroyGameObject();
            cameraObject = null;
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

        public void Render(CelestialBody sourceBody, Component target, float elevation)
        {
            if (inScaledSpace)
            {
                cameraObject.transform.position = ScaledSpace.LocalToScaledSpace(target.transform.position);
                cameraObject.transform.position += (cameraObject.transform.position - sourceBody.scaledBody.transform.position).normalized * (ScaledSpace.InverseScaleFactor * elevation);
                cameraObject.transform.LookAt(sourceBody.scaledBody.transform.position);

                float distance = (float)(cameraObject.transform.position - sourceBody.scaledBody.transform.position).magnitude;
                camera.fieldOfView = 2 * Mathf.Rad2Deg * Mathf.Acos(Mathf.Sqrt(Mathf.Max((distance * distance)
                    - (float)(ScaledSpace.InverseScaleFactor * ScaledSpace.InverseScaleFactor * sourceBody.Radius * sourceBody.Radius), 1)) / distance);

                camera.nearClipPlane = Mathf.Max(0.001f, distance - (((float)sourceBody.Radius - (float)sourceBody.atmosphereDepth) * 1.5f * ScaledSpace.InverseScaleFactor));
                camera.farClipPlane = distance + ((float)sourceBody.Radius * ScaledSpace.InverseScaleFactor);
            }
            else
            {
                cameraObject.transform.position = target.transform.position;
                cameraObject.transform.position += (cameraObject.transform.position - sourceBody.transform.position).normalized * elevation;
                cameraObject.transform.LookAt(sourceBody.transform.position);

                float distance = (float)(cameraObject.transform.position - sourceBody.transform.position).magnitude;
                camera.fieldOfView = 2 * Mathf.Rad2Deg * Mathf.Acos(Mathf.Sqrt(Mathf.Max((distance * distance) - (float)(sourceBody.Radius * sourceBody.Radius), 1)) / distance);

                camera.nearClipPlane = Mathf.Max(0.001f, distance - (((float)sourceBody.Radius - (float)sourceBody.atmosphereDepth) * 1.5f));
                camera.farClipPlane = distance + ((float)sourceBody.Radius);
            }

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = renderTexture;
            camera.Render();
            texture.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            texture.Apply(false, false);
            RenderTexture.active = currentRT;
        }
    }
}
