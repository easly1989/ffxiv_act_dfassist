using System;
using System.Globalization;
using System.Linq;
using Overlay.NET;
using Overlay.NET.Common;
using Process.NET;
using Process.NET.Memory;

namespace DFAssist.DirectX
{
    public class DirectXOverlay
    {
        private OverlayPlugin _directXoverlayPluginExample;
        private ProcessSharp _processSharp;

        public void Show()
        {
            Log.Debug(@"Please type the process name of the window you want to attach to, e.g 'notepad.");
            Log.Debug("Note: If there is more than one process found, the first will be used.");

            var processName = Console.ReadLine();

            var process = System.Diagnostics.Process.GetProcessesByName(processName).FirstOrDefault();
            if (process == null)
            {
                Log.Warn($"No process by the name of {processName} was found.");
                Log.Warn("Please open one or use a different name and restart the demo.");
                Console.ReadLine();
                return;
            }

            _directXoverlayPluginExample = new DirectxOverlayPlugin();
            _processSharp = new ProcessSharp(process, MemoryType.Remote);

            Log.Debug("Enter the frame rate the overlay should render at. e.g '60'");
            var result = Console.ReadLine();

            var fpsValid = int.TryParse(Convert.ToString(result, CultureInfo.InvariantCulture), NumberStyles.Any,
                NumberFormatInfo.InvariantInfo, out int fps);
            if (!fpsValid)
            {
                Log.Debug($"{result} is not valid. Please reload and try again by entering an integer such as '30' or '60' ");
                return;
            }

            var d3DOverlay = (DirectxOverlayPlugin)_directXoverlayPluginExample;
            d3DOverlay.Settings.Current.UpdateRate = 1000 / fps;
            _directXoverlayPluginExample.Initialize(_processSharp.WindowFactory.MainWindow);
            _directXoverlayPluginExample.Enable();

            // Log some info about the overlay.
            Log.Debug("Starting update loop (open the process you specified and drag around)");
            Log.Debug("Update rate: " + d3DOverlay.Settings.Current.UpdateRate.Milliseconds());

            var info = d3DOverlay.Settings.Current;

            Log.Debug($"Author: {info.Author}");
            Log.Debug($"Description: {info.Description}");
            Log.Debug($"Name: {info.Name}");
            Log.Debug($"Identifier: {info.Identifier}");
            Log.Debug($"Version: {info.Version}");

            Log.Info("Note: Settings are saved to a settings folder in your main app folder.");

            Log.Info("Give your window focus to enable the overlay (and unfocus to disable..)");

            Log.Info("Close the console to end the demo.");

            while (true)
            {
                _directXoverlayPluginExample.Update();
            }
        }
    }
}