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

namespace PlanetShine
{
    public class Utils
    {
        public static float TryParse(string s, float target)
        {
            float.TryParse(s, out target);
            return target;
        }

        public static int TryParse(string s, int target)
        {
            int.TryParse(s, out target);
            return target;
        }

        public static bool TryParse(string s, bool target)
        {
            bool.TryParse(s, out target);
            return target;
        }

        public static LineRenderer CreateDebugLine(Color startColor, Color endColor)
        {
            GameObject obj = new GameObject ("Line");
            LineRenderer line = obj.AddComponent< LineRenderer > ();
            line.material = new Material (Shader.Find ("Particles/Additive"));
            line.SetColors (startColor, endColor);
            line.SetWidth (0.05f, 0.05f); 
            line.SetVertexCount (2);
            return line;
        }

        public static bool DoTexturesSizeMatch(List<Texture2D> textures)
        {
            int width = 0;
            int height = 0;
            bool firstPass = true;

            foreach (Texture2D texture in textures)
            {
                if (firstPass)
                    firstPass = false;
                else
                    if (texture.width != width || texture.height != height)
                        return false;
                width = texture.width;
                height = texture.height;
            }

            return true;
        }

        

        public static Type FindType(string fullName)
        {
            return
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName.Equals(fullName));
        }

        public static Type FindTypeContains(string partialName)
        {
            return
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName.Contains(partialName));
        }

        public static Camera GetCameraByName(string name)
        {
            for (int i = 0; i < Camera.allCamerasCount; ++i)
            {
                if (Camera.allCameras[i].name == name)
                {
                    return Camera.allCameras[i];
                }
            }
            return null;
        }

        public static void CameraInvertAlpha()
        {
            var mat = new Material("Shader \"Hidden/Alpha\" {" +
                    "SubShader {" +
                    "    Pass {" +
                    "        ZTest Always Cull Off ZWrite Off" +
                    "        ColorMask A" +
                    "        Color (1,1,1,1)" +
                    "    }" +
                    "}" +
                    "}"
                );
            mat.shader.hideFlags = HideFlags.HideAndDontSave;
            mat.hideFlags = HideFlags.HideAndDontSave;
            GL.PushMatrix();
            GL.LoadOrtho();
            for (var i = 0; i < mat.passCount; ++i)
            {
                mat.SetPass(i);
                GL.Begin(GL.QUADS);
                GL.Vertex3(0f, 0f, 0.1f);
                GL.Vertex3(1f, 0f, 0.1f);
                GL.Vertex3(1f, 1f, 0.1f);
                GL.Vertex3(0f, 1f, 0.1f);
                GL.End();
            }
            GL.PopMatrix();
        }
    }


    public class DisplaySettingOption<T>
    {
        public string label { get; private set; }
        public T value { get; private set; }

        public DisplaySettingOption(string label, T value)
        {
            this.label = label;
            this.value = value;
        }
    }

    public class InvertAlpha : MonoBehaviour
    {       
        public void OnPostRender() {
            var mat = new Material ("Shader \"Hidden/Alpha\" {" +
                    "SubShader {" +
                    "    Pass {" +
                    "        ZTest Always Cull Off ZWrite Off" +
                    "        ColorMask A Blend Zero OneMinusDstAlpha" +
                    "        Color (1,1,1,1)" +
                    "    }" +
                    "}" +
                    "}"
                );
            mat.shader.hideFlags = HideFlags.HideAndDontSave;
            mat.hideFlags = HideFlags.HideAndDontSave;
            GL.PushMatrix();
            GL.LoadOrtho();
            for (var i = 0; i < mat.passCount; ++i) {
                mat.SetPass (i);
                GL.Begin (GL.QUADS);
                GL.Vertex3 (0f, 0f, 0.1f);
                GL.Vertex3 (1f, 0f, 0.1f);
                GL.Vertex3 (1f, 1f, 0.1f);
                GL.Vertex3 (0f, 1f, 0.1f);
                GL.End ();
            }
            GL.PopMatrix ();
        }
    }

    // A script that when attached to the camera, makes the resulting
    // colors inverted. See its effect in play mode.
    public class InvertColors : MonoBehaviour
    {
        private Material mat;

        // Will be called from camera after regular rendering is done.
        public void OnPostRender()
        {
            if (!mat)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things. In this case, we just want to use
                // a blend mode that inverts destination colors.			
                var shader = Shader.Find("Hidden/Internal-Colored");
                mat = new Material(shader);
                mat.hideFlags = HideFlags.HideAndDontSave;
                // Set blend mode to invert destination colors.
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                // Turn off backface culling, depth writes, depth test.
                mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                mat.SetInt("_ZWrite", 0);
                mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            }

            GL.PushMatrix();
            GL.LoadOrtho();

            // activate the first shader pass (in this case we know it is the only pass)
            mat.SetPass(0);
            // draw a quad over whole screen
            GL.Begin(GL.QUADS);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(1, 0, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(0, 1, 0);
            GL.End();

            GL.PopMatrix();
        }
    }
}

