using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DFAssist
{
    /// <summary>
    /// TODO: Refactor, remove hardcoded assembly loads
    /// Gonna HardCode all the needed assemblies to avoid any kind of problems
    /// Had to rewrite this code way too much
    ///
    /// As soon as I find a better way to resolve/load assemblies i'll do it,
    /// for now i'm ok with this... ^^'
    /// </summary>
    public static class AssemblyResolver
    {
        public static bool LoadAssemblies(string enviroment, Label labelStatus)
        {
            if (!LoadAssembly("Microsoft.WindowsAPICodePack", enviroment, labelStatus)) return false;
            if (!LoadAssembly("Microsoft.WindowsAPICodePack.Shell", enviroment, labelStatus)) return false;
            if (!LoadAssembly("Microsoft.WindowsAPICodePack.ShellExtensions", enviroment, labelStatus)) return false;
            if (!LoadAssembly("Newtonsoft.Json", enviroment, labelStatus)) return false;

            return true;
        }

        private static bool LoadAssembly(string assemblyName, string enviroment, Label labelStatus)
        {
            var currentDll = Path.Combine(enviroment, assemblyName + ".dll");
            if (File.Exists(currentDll))
            {
                Assembly.LoadFrom(currentDll);
                return true;
            }

            labelStatus.Text = $"Unable to find {currentDll}, the plugin cannot be starterd.";
            return false;

        }
    }
}
