using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Advanced_Combat_Tracker;

namespace DFAssist.Loader
{
    public class AssemblyResolver
    {
        private static AssemblyResolver _instance;
        public static AssemblyResolver Instance => _instance ?? (_instance = new AssemblyResolver());

        private bool _attached;
        private bool _initialized;
        private string _librariesPath;
        private ActPluginData _pluginData;
        private IActPluginV1 _plugin;

        public bool Attach(IActPluginV1 plugin)
        {
            if (_attached)
                return true;
            
            _attached = true;
            _plugin = plugin;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            return true;
        }

        private void Initialize(IActPluginV1 plugin)
        {
            if(_initialized)
                return;

            try
            {
                _pluginData = ActGlobals.oFormActMain.PluginGetSelfData(plugin);
                if(_pluginData == null)
                    return;

                var enviroment = Path.GetDirectoryName(_pluginData.pluginFile.ToString());
                if (string.IsNullOrWhiteSpace(enviroment))
                    return;

                _librariesPath = Path.Combine(enviroment, "libs");
                if (!Directory.Exists(_librariesPath))
                    return;

                _initialized = true;
            }
            catch (Exception)
            {
                Debug.WriteLine("There was an error when attaching to AssemblyResolve!");
                throw;
            }
        }

        public void Detach()
        {
            if (!_attached)
                return;

            _attached = false;
            _initialized = false;
            
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            _librariesPath = null;
            _pluginData = null;
            _plugin = null;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // will be done only the first time the AssemblyResolve is called
            Initialize(_plugin);

            if (!_initialized
                || args.Name.Contains(".resources")
                || args.RequestingAssembly == null
                || GetAssemblyName(args.RequestingAssembly.FullName) != nameof(DFAssist))
                return null;

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            var filename = GetAssemblyName(args.Name) + ".dll".ToLower();
            var asmFile = Path.Combine(_librariesPath, filename);

            if (File.Exists(asmFile))
            {
                try
                {
                    return Assembly.LoadFrom(asmFile);
                }
                catch (Exception)
                {
                    _pluginData.lblPluginStatus.Text = $"Unable to load {args.Name} library, it may needs to be 'Unblocked'.";
                    return null;
                }
            }

            _pluginData.lblPluginStatus.Text = $"Unable to find {asmFile}, the plugin cannot be starterd.";
            return null;
        }

        private static string GetAssemblyName(string fullAssemblyName)
        {
            return fullAssemblyName.IndexOf(",", StringComparison.Ordinal) > -1
                ? fullAssemblyName.Substring(0, fullAssemblyName.IndexOf(",", StringComparison.Ordinal))
                : fullAssemblyName;
        }
    }
}
