#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

#endregion

namespace PlanetShine
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class Logger : MonoBehaviour
    {
        #region Constants

        private static readonly string fileName;
        private static readonly AssemblyName assemblyName;

        #endregion

        #region Fields

        private static readonly List<string[]> messages = new List<string[]>();

        #endregion

        #region Initialisation

        static Logger()
        {
            assemblyName = Assembly.GetExecutingAssembly().GetName();
            fileName = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, "log");
            File.Delete(fileName);

            lock (messages)
            {
                messages.Add(new[] { "Executing: " + assemblyName.Name + " - " + assemblyName.Version });
                messages.Add(new[] { "Assembly: " + Assembly.GetExecutingAssembly().Location });
            }
            Blank();
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        #endregion

        #region Printing

        public static void Blank()
        {
            lock (messages)
            {
                messages.Add(new string[] { });
            }
        }

        public static void Log(object obj)
        {
            lock (messages)
            {
                try
                {
                    if (obj is IEnumerable)
                    {
                        messages.Add(new[] { "Log " + DateTime.Now.TimeOfDay, obj.ToString() });
                        foreach (var o in obj as IEnumerable)
                        {
                            messages.Add(new[] { "\t", o.ToString() });
                        }
                    }
                    else
                    {
                        messages.Add(new[] { "Log " + DateTime.Now.TimeOfDay, obj.ToString() });
                    }
                }
                catch (Exception ex)
                {
                    Exception(ex);
                }
            }
        }

        public static void Log(string name, object obj)
        {
            lock (messages)
            {
                try
                {
                    if (obj is IEnumerable)
                    {
                        messages.Add(new[] { "Log " + DateTime.Now.TimeOfDay, name });
                        foreach (var o in obj as IEnumerable)
                        {
                            messages.Add(new[] { "\t", o.ToString() });
                        }
                    }
                    else
                    {
                        messages.Add(new[] { "Log " + DateTime.Now.TimeOfDay, obj.ToString() });
                    }
                }
                catch (Exception ex)
                {
                    Exception(ex);
                }
            }
        }

        public static void Log(string message)
        {
            lock (messages)
            {
                messages.Add(new[] { "Log " + DateTime.Now.TimeOfDay, message });
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerStepThrough]
        public static void Debug(string message)
        {
            lock (messages)
            {
                messages.Add(new[] { "Debug " + DateTime.Now.TimeOfDay, message });
            }
        }

        public static void Warning(string message)
        {
            lock (messages)
            {
                messages.Add(new[] { "Warning " + DateTime.Now.TimeOfDay, message });
            }
        }

        public static void Error(string message)
        {
            lock (messages)
            {
                messages.Add(new[] { "Error " + DateTime.Now.TimeOfDay, message });
            }
        }

        public static void Exception(Exception ex)
        {
            lock (messages)
            {
                messages.Add(new[] { "Exception " + DateTime.Now.TimeOfDay, ex.Message });
                messages.Add(new[] { string.Empty, ex.StackTrace });
                Blank();
            }
        }

        public static void Exception(Exception ex, string location)
        {
            lock (messages)
            {
                messages.Add(new[] { "Exception " + DateTime.Now.TimeOfDay, location + " // " + ex.Message });
                messages.Add(new[] { string.Empty, ex.StackTrace });
                Blank();
            }
        }

        #endregion

        #region Flushing

        public static void Flush()
        {
            lock (messages)
            {
                if (messages.Count > 0)
                {
                    using (var file = File.AppendText(fileName))
                    {
                        foreach (var message in messages)
                        {
                            file.WriteLine(message.Length > 0 ? message.Length > 1 ? "[" + message[0] + "]: " + message[1] : message[0] : string.Empty);
                            if (message.Length > 0)
                            {
                                print(message.Length > 1 ? assemblyName.Name + " -> " + message[1] : assemblyName.Name + " -> " + message[0]);
                            }
                        }
                    }
                    messages.Clear();
                }
            }
        }

        private void LateUpdate()
        {
            Flush();
        }

        #endregion

        #region Destruction

        private void OnDestroy()
        {
            Flush();
        }

        ~Logger()
        {
            Flush();
        }

        #endregion
    }
}
