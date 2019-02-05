using System.Threading;
using System.Threading.Tasks;

namespace DFAssist.DirectX
{
    public class DirectXToastManager
    {
        private readonly DirectXOverlay _directXOverlay;
        private CancellationTokenSource _cancellationTokenSource;

        public DirectXToastManager()
        {
            _directXOverlay = new DirectXOverlay();
        }

        public void Show(string title, string message, int processId)
        {
            _cancellationTokenSource = new CancellationTokenSource(10000);
            Task.Factory.StartNew(() =>
            {
                _directXOverlay.Handle(title, message, processId, _cancellationTokenSource.Token);
            }, _cancellationTokenSource.Token);
        }

        public void Hide()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}