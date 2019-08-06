using System;
using System.Threading;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using DFAssist.Contracts.Repositories;
using Splat;

namespace DFAssist.Helpers
{
    // ReSharper disable InconsistentNaming
    public class ACTPluginUpdateHelper : IDisposable
    {
        private static ACTPluginUpdateHelper _instance;
        public static ACTPluginUpdateHelper Instance => _instance ?? (_instance = new ACTPluginUpdateHelper());

        private ILocalizationRepository _localizationRepository;
        private Thread _updateThread;

        public ACTPluginUpdateHelper()
        {
            _localizationRepository = Locator.Current.GetService<ILocalizationRepository>();
        }

        public void Subscribe()
        {
            ActGlobals.oFormActMain.UpdateCheckClicked += FormActMain_UpdateCheckClicked;
            if (!ActGlobals.oFormActMain.GetAutomaticUpdatesAllowed())
                return;

            _updateThread = new Thread(FormActMain_UpdateCheckClicked);
            _updateThread.Start();
        }

        private void FormActMain_UpdateCheckClicked()
        {
            const int pluginId = 71;
            try
            {
                var pluginData = Locator.Current.GetService<ActPluginData>();
                var localDate = ActGlobals.oFormActMain.PluginGetSelfDateUtc(pluginData.pluginObj);
                var remoteDate = ActGlobals.oFormActMain.PluginGetRemoteDateUtc(pluginId);
                if (localDate.AddHours(2) >= remoteDate)
                    return;

                var result = MessageBox.Show(_localizationRepository.GetText("ui-update-available-message"), _localizationRepository.GetText("ui-update-available-title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                    return;

                var updatedFile = ActGlobals.oFormActMain.PluginDownload(pluginId);
                if (pluginData.pluginFile.Directory != null)
                    ActGlobals.oFormActMain.UnZip(updatedFile.FullName, pluginData.pluginFile.Directory.FullName);

                ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, false);
                Application.DoEvents();
                ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, true);
            }
            catch (Exception ex)
            {
                ActGlobals.oFormActMain.WriteExceptionLog(ex, "Plugin Update Check");
            }
        }

        public void Dispose()
        {
            ActGlobals.oFormActMain.UpdateCheckClicked -= FormActMain_UpdateCheckClicked;

            try
            {
                if (_updateThread != null && _updateThread.IsAlive)
                    _updateThread.Abort();
            }
            catch (Exception)
            {
                // Abort throws by default, we just ignore this!
            }

            _localizationRepository = null;
            _updateThread = null;
            _instance = null;
        }
    }
    // ReSharper restore InconsistentNaming
}