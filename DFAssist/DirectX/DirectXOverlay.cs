using System.Threading;
using Process.NET;
using Process.NET.Memory;

namespace DFAssist.DirectX
{
    public class DirectXOverlay
    {
        private DirectxOverlayPlugin _directXoverlayPlugin;
        private ProcessSharp _processSharp;

        public void Handle(string title, string message, int processId, CancellationToken cancellationToken)
        {
            var process = System.Diagnostics.Process.GetProcessById(processId);

            _directXoverlayPlugin = new DirectxOverlayPlugin(title, message);
            _processSharp = new ProcessSharp(process, MemoryType.Remote);

            var d3DOverlay = _directXoverlayPlugin;
            d3DOverlay.Settings.Current.UpdateRate = 1000 / 60;
            _directXoverlayPlugin.Initialize(_processSharp.WindowFactory.MainWindow);
            _directXoverlayPlugin.Enable();

            while (!cancellationToken.IsCancellationRequested)
            {
                _directXoverlayPlugin.Update();
            }

            _directXoverlayPlugin.Disable();
            _directXoverlayPlugin.ClearScreen();
        }
    }
}