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

            // Startup defaults
            // Discord must be disabled for the moment, still under development
            _mainControl.DiscordCheckBox.Enabled = false;
            _mainControl.EnableActToast.Enabled = !_mainControl.DisableToasts.Checked;
            _mainControl.PersistToasts.Enabled = !_mainControl.DisableToasts.Checked && !_mainControl.EnableActToast.Checked;
            _mainControl.TtsVoicesComboBox.Enabled = _mainControl.TtsCheckBox.Checked;
            _mainControl.TelegramChatIdTextBox.Enabled = _mainControl.TelegramCheckBox.Checked;
            _mainControl.TelegramTokenTextBox.Enabled = _mainControl.TelegramCheckBox.Checked;
            _mainControl.PushBulletTokenTextBox.Enabled = _mainControl.PushBulletCheckbox.Checked;
            _mainControl.PushBulletDeviceIdTextBox.Enabled = _mainControl.PushBulletCheckbox.Checked;
            _mainControl.DiscordWebhookTextBox.Enabled = _mainControl.DiscordCheckBox.Checked;
            _mainControl.DiscordUsernameTextBox.Enabled = _mainControl.DiscordCheckBox.Checked;

            // force initialization of combobox values, not related a subvalue (like the language)
            LogLevelComboBoxOnSelectedValueChanged(this, new EventArgs());
            TtsVoicesComboBoxOnSelectedValueChanged(this, new EventArgs());
        }

        public void Subscribe()
        {
            if (_subscribed)
                return;

            _subscribed = true;
            _mainControl.CopyrightLink.LinkClicked += CopyrightLinkOnLinkClicked;
            _mainControl.DonateLink.LinkClicked += DonateLinkOnLinkClicked;
            _mainControl.FlashTaskbar.CheckStateChanged += FlashTaskbarOnCheckedChanged;
            _mainControl.DisableToasts.CheckStateChanged += DisableToastsOnCheckedChanged;
            _mainControl.PersistToasts.CheckStateChanged += PersistToastsOnCheckedChanged;
            _mainControl.EnableActToast.CheckStateChanged += EnableActToastsOnCheckedChanged;
            _mainControl.TtsCheckBox.CheckStateChanged += EnableTtsOnCheckedChanged;
            _mainControl.TtsVoicesComboBox.SelectedValueChanged += TtsVoicesComboBoxOnSelectedValueChanged;
            _mainControl.TelegramCheckBox.CheckStateChanged += EnableTelegramOnCheckedChanged;
            _mainControl.DiscordCheckBox.CheckStateChanged += DiscordCheckBoxOnCheckStateChanged;
            _mainControl.PushBulletCheckbox.CheckStateChanged += EnablePushBulletOnCheckedChanged;
            _mainControl.ClearLogButton.Click += ClearLogsButton_Click;
            _mainControl.TestConfigurationButton.Click += TestConfigurationButton_Click;
            _mainControl.LogLevelComboBox.SelectedValueChanged += LogLevelComboBoxOnSelectedValueChanged;
        }

        public void UnSubscribe()
        {
            if (!_subscribed)
                return;

            _subscribed = false;
            _mainControl.CopyrightLink.LinkClicked -= CopyrightLinkOnLinkClicked;
            _mainControl.DonateLink.LinkClicked -= DonateLinkOnLinkClicked;
            _mainControl.FlashTaskbar.CheckStateChanged -= FlashTaskbarOnCheckedChanged;
            _mainControl.DisableToasts.CheckStateChanged -= DisableToastsOnCheckedChanged;
            _mainControl.PersistToasts.CheckStateChanged -= PersistToastsOnCheckedChanged;
            _mainControl.EnableActToast.CheckStateChanged -= EnableActToastsOnCheckedChanged;
            _mainControl.TtsCheckBox.CheckStateChanged -= EnableTtsOnCheckedChanged;
            _mainControl.TtsVoicesComboBox.SelectedValueChanged -= TtsVoicesComboBoxOnSelectedValueChanged;
            _mainControl.TelegramCheckBox.CheckStateChanged -= EnableTelegramOnCheckedChanged;
            _mainControl.DiscordCheckBox.CheckStateChanged -= DiscordCheckBoxOnCheckStateChanged;
            _mainControl.PushBulletCheckbox.CheckStateChanged -= EnablePushBulletOnCheckedChanged;
            _mainControl.ClearLogButton.Click -= ClearLogsButton_Click;
            _mainControl.TestConfigurationButton.Click -= TestConfigurationButton_Click;
            _mainControl.LogLevelComboBox.SelectedValueChanged += LogLevelComboBoxOnSelectedValueChanged;
        }

        private void LogLevelComboBoxOnSelectedValueChanged(object sender, EventArgs e)
        {
            var selectedValue = _mainControl.LogLevelComboBox.SelectedValue as string;
            if(string.IsNullOrWhiteSpace(selectedValue) || !Enum.TryParse(selectedValue, out LogLevel logLevel))
            {
                _logger.Write($"UI: [LogLevel] Unable to change log level", LogLevel.Error);
                return;
            }

            _logger.SetLoggingLevel(logLevel);
            _logger.Write($"UI: [LogLevel] Desired Value: {_mainControl.LogLevelComboBox.SelectedValue}", LogLevel.Debug);
        }

        private void TtsVoicesComboBoxOnSelectedValueChanged(object sender, EventArgs e)
        {
            TTSHelper.Instance.SelectVoice(_mainControl.TtsVoicesComboBox.SelectedValue as string);
            _logger.Write($"UI: [TTS] Selected Voice - Desired Value: {_mainControl.TtsVoicesComboBox.SelectedValue}", LogLevel.Debug);
            TTSHelper.Instance.SendNotification(_localizationRepository.GetText("ui-tts-notification-test-title"), _localizationRepository.GetText("ui-tts-notification-test-message"));
        }

        private void DonateLinkOnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _mainControl.DonateLink.LinkVisited = true;
            System.Diagnostics.Process.Start("https://www.paypal.me/ruggierocarlo");
            _logger.Write("Every little donation counts! Thank you :)", LogLevel.Info);
        }

        private void CopyrightLinkOnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _mainControl.CopyrightLink.LinkVisited = true;
            System.Diagnostics.Process.Start("https://github.com/easly1989/ffxiv_act_dfassist");
            _logger.Write("Opening link to the DFAssist github repository!", LogLevel.Debug);
        }

        private void FlashTaskbarOnCheckedChanged(object sender, EventArgs e)
        {
            _logger.Write($"UI: [FlashTaskbar] Desired Value: {_mainControl.FlashTaskbar.Checked}", LogLevel.Debug);
            TaskbarFlashHelper.Instance.SendNotification();
        }

        private void DisableToastsOnCheckedChanged(object sender, EventArgs e)
        {
            _logger.Write($"UI: [DisableToasts] Desired Value: {_mainControl.DisableToasts.Checked}", LogLevel.Debug);
            _mainControl.EnableActToast.Enabled = !_mainControl.DisableToasts.Checked;
            _mainControl.PersistToasts.Enabled = !_mainControl.DisableToasts.Checked && !_mainControl.EnableActToast.Checked;

            ToastHelper.Instance.SendNotification(_localizationRepository.GetText("ui-toast-notification-test-title"), _localizationRepository.GetText("ui-toast-notification-test-message"));
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
                using (var key = Registry.CurrentUser.OpenSubKey(keyName, true))
                {
                    if (_mainControl.PersistToasts.Checked)
                    {
                        if (key == null)
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
                        if (key == null)
                        {
                            _logger.Write("UI: [PersistentToasts] Key not found in the registry, nothing to do!", LogLevel.Debug);
                            return;
                        }

                        _logger.Write("UI: [PersistentToasts] Key found in the registry, Removing value!", LogLevel.Debug);
                        key.DeleteValue("ShowInActionCenter");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Write(ex, "UI: Unable to remove/add the registry key to make Toasts persistent!", LogLevel.Error);
            }
        }

        private void EnableTtsOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            _mainControl.TtsVoicesComboBox.Enabled = _mainControl.TtsCheckBox.Checked;
            _logger.Write($"UI: [TTS] Desired Value: {_mainControl.TtsCheckBox.Checked}", LogLevel.Debug);
            TTSHelper.Instance.SendNotification(_localizationRepository.GetText("ui-tts-notification-test-title"), _localizationRepository.GetText("ui-tts-notification-test-message"));
        }

        private void EnableTelegramOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            _logger.Write($"UI: [Telegram] Desired Value: {_mainControl.TelegramCheckBox.Checked}", LogLevel.Debug);
            _mainControl.TelegramChatIdTextBox.Enabled = _mainControl.TelegramCheckBox.Checked;
            _mainControl.TelegramTokenTextBox.Enabled = _mainControl.TelegramCheckBox.Checked;
        }

        private void DiscordCheckBoxOnCheckStateChanged(object sender, EventArgs e)
        {
            _logger.Write($"UI: [Discord] Desired Value: {_mainControl.DiscordCheckBox.Checked}", LogLevel.Debug);
            _mainControl.DiscordWebhookTextBox.Enabled = _mainControl.DiscordCheckBox.Checked;
            _mainControl.DiscordUsernameTextBox.Enabled = _mainControl.DiscordCheckBox.Checked;
        }

        private void EnablePushBulletOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            _logger.Write($"UI: [PushBullet] Desired Value: {_mainControl.PushBulletCheckbox.Checked}", LogLevel.Debug);
            _mainControl.PushBulletTokenTextBox.Enabled = _mainControl.PushBulletCheckbox.Checked;
            _mainControl.PushBulletDeviceIdTextBox.Enabled = _mainControl.PushBulletCheckbox.Checked;
        }

        private void ClearLogsButton_Click(object sender, EventArgs e)
        {
            _mainControl.LoggingRichTextBox.Clear();
        }

        private void TestConfigurationButton_Click(object sender, EventArgs e)
        {
            var title = _localizationRepository.GetText("ui-toast-notification-test-title");
            var message = _localizationRepository.GetText("ui-toast-notification-test-message");
            TaskbarFlashHelper.Instance.SendNotification();
            ToastHelper.Instance.SendNotification(title, message);
            TelegramHelper.Instance.SendNotification(title, message);
            PushBulletHelper.Instance.SendNotification(title, message);
            DiscordHelper.Instance.SendNotification(title, message);
            TTSHelper.Instance.SendNotification(_localizationRepository.GetText("ui-tts-notification-test-title"), _localizationRepository.GetText("ui-tts-notification-test-message"));
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
