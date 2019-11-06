using System;
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
#if DEBUG
        private string _pdbPath;
#endif
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
            if (_initialized)
                return;

            try
            {
                _pluginData = ActGlobals.oFormActMain.PluginGetSelfData(plugin);
                if (_pluginData == null)
                {
                    ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "[DFAssist] Unable to find DFAssist data from ActGlobals.oFormActMain!");
                    return;
                }

                var enviroment = Path.GetDirectoryName(_pluginData.pluginFile.ToString());
                if (string.IsNullOrWhiteSpace(enviroment))
                {
                    ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "[DFAssist] Unable to find the plugin base directory!");
                    return;
                }

                _librariesPath = Path.Combine(enviroment, "libs");
                if (!Directory.Exists(_librariesPath))
                {
                    ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "[DFAssist] Unable to find the 'libs' directory!");
                    return;
                }

#if DEBUG
                // we also add the pdb folder to load,
                // this is completely optional and will be done only if we are in debug
                _pdbPath = Path.Combine(enviroment, "pdb");
                if(!Directory.Exists(_pdbPath))
                {
                    ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "[DFAssist] Unable to find the 'pdb' directory!");
                    return;
                }
#endif

                _initialized = true;
            }
            catch (Exception)
            {
                ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "[DFAssist] There was an error when attaching to AssemblyResolve!");
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
                || args.RequestingAssembly == null)
                return null;

            // check if any of the assemblies of this plugin is requesting an AssemblyResolve
            var requestingAssemblyName = GetAssemblyName(args.RequestingAssembly.FullName);
            if (requestingAssemblyName != "DFAssist"
                && requestingAssemblyName != "DFAssist.Plugin"
                && requestingAssemblyName != "DFAssist.Core"
                && requestingAssemblyName != "DFAssist.Contracts"
                && requestingAssemblyName != "DFAssist.WinToast")
                return null;

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            var filename = GetAssemblyName(args.Name);
            var dllFile = filename + ".dll".ToLower();
            var dllFileFullPath = Path.Combine(_librariesPath, dllFile);
#if DEBUG
            var pdbFile = filename + ".pdb".ToLower();
            var pdbFileFullPath = Path.Combine(_pdbPath, pdbFile);
#endif
            if (File.Exists(dllFileFullPath))
            {
                try
                {
                    var dllBytes = File.ReadAllBytes(dllFileFullPath);
#if DEBUG
                    if(File.Exists(pdbFileFullPath))
                    {
                        var pdbBytes = File.ReadAllBytes(pdbFileFullPath);
                        return Assembly.Load(dllBytes, pdbBytes);
                    }
#endif
                    return Assembly.Load(dllBytes);
                }
                catch (Exception)
                {
                    _pluginData.lblPluginStatus.Text = $"Unable to load {args.Name} library, it may needs to be 'Unblocked'.";
                    return null;
                }
            }

            _pluginData.lblPluginStatus.Text = $"Unable to find {dllFile}, the plugin cannot be starterd.";
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
