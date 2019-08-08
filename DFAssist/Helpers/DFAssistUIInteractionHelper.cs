using System;
using System.Windows.Forms;
using DFAssist.Contracts.Repositories;
using Microsoft.Win32;
using Splat;

namespace DFAssist.Helpers
{
    // ReSharper disable InconsistentNaming
    public class DFAssistUIInteractionHelper : IDisposable
    {
        private static DFAssistUIInteractionHelper _instance;
        public static DFAssistUIInteractionHelper Instance => _instance ?? (_instance = new DFAssistUIInteractionHelper());

        private bool _subscribed;
        private IActLogger _logger;
        private ILocalizationRepository _localizationRepository;
        private MainControl _mainControl;

        public DFAssistUIInteractionHelper()
        {
            _mainControl = Locator.Current.GetService<MainControl>();
            _localizationRepository = Locator.Current.GetService<ILocalizationRepository>();
            _logger = Locator.Current.GetService<IActLogger>();
        }

        public void Subscribe()
        {
            if(_subscribed)
                return;

            _subscribed = true;
            _mainControl.DisableToasts.CheckStateChanged += DisableToastsOnCheckedChanged;
            _mainControl.PersistToasts.CheckStateChanged += PersistToastsOnCheckedChanged;
            _mainControl.EnableActToast.CheckStateChanged += EnableActToastsOnCheckedChanged;
            _mainControl.TtsCheckBox.CheckStateChanged += EnableTtsOnCheckedChanged;
            _mainControl.ClearLogButton.Click += ClearLogsButton_Click;

        }

        public void UnSubscribe()
        {
            if(!_subscribed)
                return;
            
            _subscribed = false;
            _mainControl.DisableToasts.CheckStateChanged -= DisableToastsOnCheckedChanged;
            _mainControl.PersistToasts.CheckStateChanged -= PersistToastsOnCheckedChanged;
            _mainControl.EnableActToast.CheckStateChanged -= EnableActToastsOnCheckedChanged;
            _mainControl.TtsCheckBox.CheckStateChanged -= EnableTtsOnCheckedChanged;
            _mainControl.ClearLogButton.Click -= ClearLogsButton_Click;
        }

                private void DisableToastsOnCheckedChanged(object sender, EventArgs e)
        {
            _logger.Write($"UI: [DisableToasts] Desired Value: {_mainControl.DisableToasts.Checked}", LogLevel.Debug);
            _mainControl.EnableActToast.Enabled = !_mainControl.DisableToasts.Checked;
            _mainControl.PersistToasts.Enabled = _mainControl.EnableActToast.Enabled && !_mainControl.EnableActToast.Checked;
        }

        private void EnableActToastsOnCheckedChanged(object sender, EventArgs e)
        {
            _logger.Write($"UI: [LegacyToasts] Desired Value: {_mainControl.EnableActToast.Checked}", LogLevel.Debug);
            _mainControl.PersistToasts.Enabled = !_mainControl.EnableActToast.Checked;

            ToastHelper.Instance.SendNotification(_localizationRepository.GetText("ui-toast-notification-test-title"), _localizationRepository.GetText("ui-toast-notification-test-message"));
        }

        private void PersistToastsOnCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _logger.Write($"UI: [PersistentToasts] Desired Value: {_mainControl.PersistToasts.Checked}!", LogLevel.Debug);

                var keyName = $@"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\{DFAssistPlugin.AppId}";
                using(var key = Registry.CurrentUser.OpenSubKey(keyName, true))
                {
                    if(_mainControl.PersistToasts.Checked)
                    {
                        if(key == null)
                        {
                            _logger.Write("UI: [PersistentToasts] Key not found in the registry, Adding a new one!", LogLevel.Debug);
                            Registry.SetValue($@"HKEY_CURRENT_USER\{keyName}", "ShowInActionCenter", 1, RegistryValueKind.DWord);
                        }
                        else
                        {
                            _logger.Write("UI: [PersistentToasts] Key found in the registry, setting value to 1!", LogLevel.Debug);
                            key.SetValue("ShowInActionCenter", 1, RegistryValueKind.DWord);
                        }
                    }
                    else
                    {
                        if(key == null)
                        {
                            _logger.Write("UI: [PersistentToasts] Key not found in the registry, nothing to do!", LogLevel.Debug);
                            return;
                        }

                        _logger.Write($"UI: [PersistentToasts] Key found in the registry, Removing value!", LogLevel.Debug);
                        key.DeleteValue("ShowInActionCenter");
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Write(ex, $"UI: Unable to remove/add the registry key to make Toasts persistent!", LogLevel.Error);
            }
        }

        private void EnableTtsOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            _logger.Write($"UI: [TTS] Desired Value: {_mainControl.TtsCheckBox.Checked}", LogLevel.Debug);
            TTSHelper.Instance.SendNotification(_localizationRepository.GetText("ui-tts-notification-test-message"), _localizationRepository.GetText("ui-tts-notification-test-title"));
        }

        private void ClearLogsButton_Click(object sender, EventArgs e)
        {
            _mainControl.LoggingRichTextBox.Clear();
        }

        public void Dispose()
        {
            UnSubscribe();

            _logger = null;
            _mainControl = null;
            _localizationRepository = null;
            _instance = null;
        }
    }
    // ReSharper restore InconsistentNaming
}
