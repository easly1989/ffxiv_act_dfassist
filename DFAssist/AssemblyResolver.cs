using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DFAssist
{
    public static class AssemblyResolver
    {
        public static bool LoadAssembly(string assemblyName, string enviroment, Label labelStatus, out Assembly result)
        {
            result = null;

            var name = GetAssemblyName(assemblyName);
            var currentDll = Path.Combine(enviroment, "libs", name + ".ref");
            if (File.Exists(currentDll))
            {
                try
                {
                    var dllBytes = File.ReadAllBytes(currentDll);
                    result = AppDomain.CurrentDomain.Load(dllBytes);
                    return true;
                }
                catch (Exception)
                {
                    labelStatus.Text = $"Unable to load {assemblyName} library, it may needs to be 'Unblocked'.";
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
