using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DFAssist
{
    public static class AssemblyResolver
    {
        private static bool _initialized;
        private static string _librariesPath;
        private static readonly Dictionary<string, bool> Assemblies;

        static AssemblyResolver()
        {
            Assemblies = new Dictionary<string, bool>();
        }

        /// <summary>
        /// must be called before LoadAssembly!
        /// </summary>
        /// <param name="enviroment"></param>
        public static void Initialize(string enviroment)
        {
            if(_initialized)
                return;

            _initialized = true;
            _librariesPath = Path.Combine(enviroment, "libs");

            try
            {
                foreach (var file in Directory.GetFiles(_librariesPath))
                {
                    var key = Path.GetFileNameWithoutExtension(file);
                    if(Assemblies.ContainsKey(key))
                        continue;

                    Assemblies.Add(key, false);
                }
            }
            catch (Exception)
            {
                // no dll loaded..
                Debug.WriteLine("Unable to load any DLL from libs..., the plugin cannot start!");
            }
        }

        public static bool LoadAssembly(ResolveEventArgs args, Label labelStatus, out Assembly result)
        {
            result = null;

            if(!_initialized || args.RequestingAssembly == null || GetAssemblyName(args.RequestingAssembly.FullName) != nameof(DFAssist))
                return true; // avoid throwing, maybe it will be initialized later... who knows? >_<

            string currentDll;
            var name = GetAssemblyName(args.Name);
            if(Assemblies.TryGetValue(name, out _))
            {
                currentDll = Path.Combine(_librariesPath, name + ".dll");
            }
            else
            {
                // all the other assemblies should be loaded automatically
                return true;
            }

            if (File.Exists(currentDll))
            {
                try
                {
                    var dllBytes = File.ReadAllBytes(currentDll);
                    result = AppDomain.CurrentDomain.Load(dllBytes);
                    Assemblies[name] = true;
                    return true;
                }
                catch (Exception)
                {
                    labelStatus.Text = $"Unable to load {args.Name} library, it may needs to be 'Unblocked'.";
                    return false;
                }
            }

            labelStatus.Text = $"Unable to find {currentDll}, the plugin cannot be starterd.";
            return false;
        }

        private static string GetAssemblyName(string fullAssemblyName)
        {
            return fullAssemblyName.IndexOf(",", StringComparison.Ordinal) > -1 
                ? fullAssemblyName.Substring(0, fullAssemblyName.IndexOf(",", StringComparison.Ordinal))
                : fullAssemblyName;
        }
    }
}
