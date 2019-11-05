using System;
using System.Linq;
using System.Windows.Forms;
using Advanced_Combat_Tracker;

namespace DFAssist.Helpers
{
    // ReSharper disable InconsistentNaming
    public class FFXIVPluginHelper : IDisposable
    {
        private static FFXIVPluginHelper _instance;
        public static FFXIVPluginHelper Instance => _instance ?? (_instance = new FFXIVPluginHelper());

        private const string ffxivActPluginDll = "FFXIV_ACT_Plugin.dll";

        private ActPluginData _ffxivPluginData;

        private Action<bool> _onIsEnabledChanged;

        public FFXIVPluginHelper()
        {
            var plugins = ActGlobals.oFormActMain.ActPlugins;
            _ffxivPluginData = plugins.FirstOrDefault(x => x.lblPluginTitle.Text == ffxivActPluginDll);
        }

        public bool Check(ActPluginData dfAssistPluginData, Action<bool> onIsEnabledChanged)
        {
            // Before anything else, if the FFXIV Parsing Plugin is not already initialized
            // than this plugin cannot start
            if (_ffxivPluginData == null)
            {
                MessageBox.Show($"{ffxivActPluginDll} must be installed BEFORE DFAssist!");
                dfAssistPluginData.cbEnabled.Checked = false;
                return false;
            }

            if (!_ffxivPluginData.cbEnabled.Checked)
            {
                MessageBox.Show($"{ffxivActPluginDll} must be enabled");
                dfAssistPluginData.cbEnabled.Checked = false;
                return false;
            }

            _onIsEnabledChanged = onIsEnabledChanged;
            _ffxivPluginData.cbEnabled.CheckedChanged += FFXIVParsingPlugin_IsEnabledChanged;
            return true;
        }

        private void FFXIVParsingPlugin_IsEnabledChanged(object sender, EventArgs e)
        {
            _onIsEnabledChanged?.Invoke(_ffxivPluginData.cbEnabled.Checked);
        }

        public void Dispose()
        {
            if (_ffxivPluginData != null)
                _ffxivPluginData.cbEnabled.CheckedChanged -= FFXIVParsingPlugin_IsEnabledChanged;

            _ffxivPluginData = null;
            _instance = null;
        }
    }
    // ReSharper restore InconsistentNaming
}
