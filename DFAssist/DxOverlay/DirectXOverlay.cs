using System.Linq;
using Overlay.NET;
using Process.NET;
using Process.NET.Memory;

namespace DFAssist.DxOverlay
{
    public class DirectXOverlay
    {
        private OverlayPlugin _directXoverlayPlugin;
        private ProcessSharp _processSharp;

        public void Show()
        {
            var processName = "notepad++";
            var process = System.Diagnostics.Process.GetProcessesByName(processName).FirstOrDefault();
            if (process == null)
                return;

            _directXoverlayPlugin = new DirectxOverlayPlugin();
            _processSharp = new ProcessSharp(process, MemoryType.Remote);

            var d3DOverlay = (DirectxOverlayPlugin)_directXoverlayPlugin;
            d3DOverlay.Settings.Current.UpdateRate = 1000 / 60;
            _directXoverlayPlugin.Initialize(_processSharp.WindowFactory.MainWindow);
            _directXoverlayPlugin.Enable();
            
            //while (true)
            //{
            //    _directXoverlayPlugin.Update();
            //}
        }
    }
}