using System;
using System.Threading;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using DFAssist.Contracts.Repositories;
using Splat;

namespace DFAssist.Helpers
{
    // ReSharper disable InconsistentNaming
    public class ACTPluginUpdateHelper
    {
        private static ACTPluginUpdateHelper _instance;
        public static ACTPluginUpdateHelper Instance => _instance ?? (_instance = new ACTPluginUpdateHelper());

        private readonly ILocalizationRepository _localizationRepository;
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

            _updateThread = new Thread(FormActMain_UpdateCheckClicked) { IsBackground = true };
            _updateThread.Start();
        }

        private void FormActMain_UpdateCheckClicked()
        {
            const int pluginId = 71;
            
            try
            {
                var pluginData = Locator.Current.GetService<ActPluginData>();
                var localVersion = ActGlobals.oFormActMain.PluginGetSelfData(pluginData.pluginObj).pluginVersion;
                var remoteVersion = ActGlobals.oFormActMain.PluginGetRemoteVersion(pluginId);
                if (Version.Parse(localVersion) >= Version.Parse(remoteVersion))
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

                MessageBox.Show(_localizationRepository.GetText("ui-update-restart-message"), _localizationRepository.GetText("ui-update-restart-title"), MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            catch (Exception ex)
            {
                ActGlobals.oFormActMain.WriteExceptionLog(ex, "Plugin Update Check");
            }
        }
    }
    // ReSharper restore InconsistentNaming
}