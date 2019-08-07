using System.Windows.Forms;
using Advanced_Combat_Tracker;

namespace DFAssist.Loader
{
    // ReSharper disable InconsistentNaming
    public class DFAssistActPlugin : IActPluginV1
    {
        private dynamic _mainControl;

        public DFAssistActPlugin()
        {
            AssemblyResolver.Instance.Attach(this);
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            _mainControl = new MainControl(this);
            _mainControl.InitPlugin(pluginScreenSpace, pluginStatusText);
        }

        public void DeInitPlugin()
        {
            AssemblyResolver.Instance.Detach();
            _mainControl.DeInitPlugin();
            _mainControl = null;
        }
    }
    // ReSharper restore InconsistentNaming
}