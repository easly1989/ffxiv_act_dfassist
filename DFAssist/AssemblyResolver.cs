using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Advanced_Combat_Tracker;

namespace DFAssist
{
    public class AssemblyResolver :
        IDisposable
    {
        private static AssemblyResolver _instance;
        public static AssemblyResolver Instance => _instance ?? (_instance = new AssemblyResolver());

        private IActPluginV1 _plugin;
        public List<string> Directories { get; } = new List<string>();

        public void Initialize(IActPluginV1 plugin)
        {
            _plugin = plugin;

            Directories.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Advanced Combat Tracker\Plugins"));

            AppDomain.CurrentDomain.AssemblyResolve -= CustomAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve += CustomAssemblyResolve;
        }

        public static void Free()
        {
            _instance?.Dispose();
            _instance = null;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CustomAssemblyResolve;
            _plugin = null;
        }

        private Assembly CustomAssemblyResolve(object sender, ResolveEventArgs e)
        {
            Assembly TryLoadAssembly(string directory, string extension)
            {
                var asm = new AssemblyName(e.Name);

                var asmPath = Path.Combine(directory, asm.Name + extension);
                return File.Exists(asmPath) ? Assembly.LoadFrom(asmPath) : null;
            }

            var pluginDirectory = ActGlobals.oFormActMain?.PluginGetSelfData(_plugin)?.pluginFile.DirectoryName;
            if (!string.IsNullOrEmpty(pluginDirectory))
            {
                if (Directories.All(x => x != pluginDirectory))
                {
                    Directories.Add(pluginDirectory);
                    Directories.Add(Path.Combine(pluginDirectory, "references"));

                    var architect = Environment.Is64BitProcess ? "x64" : "x86";
                    Directories.Add(Path.Combine(pluginDirectory, $@"{architect}"));
                    Directories.Add(Path.Combine(pluginDirectory, $@"references\{architect}"));
                }
            }

            foreach (var directory in Directories)
            {
                var asm = TryLoadAssembly(directory, ".dll");
                if (asm != null)
                {
                    return asm;
                }
            }

            return null;
        }
    }
}
