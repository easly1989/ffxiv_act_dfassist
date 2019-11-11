using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Advanced_Combat_Tracker;
using DFAssist.Contracts.DataModel;
using Splat;

namespace DFAssist.Helpers
{
    // ReSharper disable InconsistentNaming
    public class ACTPluginSettingsHelper : IDisposable
    {
        private static ACTPluginSettingsHelper _instance;
        public static ACTPluginSettingsHelper Instance => _instance ?? (_instance = new ACTPluginSettingsHelper());

        private readonly string _settingsFile;

        private IActLogger _logger;
        private MainControl _mainControl;
        private ActPluginData _pluginData;
        private SettingsSerializer _xmlSettingsSerializer;

        public ACTPluginSettingsHelper()
        {
            _mainControl = Locator.Current.GetService<MainControl>();
            _pluginData = Locator.Current.GetService<ActPluginData>();
            _logger = Locator.Current.GetService<IActLogger>();

            _xmlSettingsSerializer = new SettingsSerializer(_mainControl);
            _settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config", "DFAssist.config.xml");
        }

        public void LoadSettings()
        {
            _logger.Write("Settings Loading...", LogLevel.Debug);
            // All the settings to deserialize
            _xmlSettingsSerializer.AddControlSetting(_mainControl.DisableToasts.Name, _mainControl.DisableToasts);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.LanguageValue.Name, _mainControl.LanguageValue);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.TtsCheckBox.Name, _mainControl.TtsCheckBox);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.TtsVoicesComboBox.Name, _mainControl.TtsVoicesComboBox);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.PersistToasts.Name, _mainControl.PersistToasts);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.EnableTestEnvironment.Name, _mainControl.EnableTestEnvironment);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.LogLevelComboBox.Name, _mainControl.LogLevelComboBox);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.EnableActToast.Name, _mainControl.EnableActToast);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.TelegramCheckBox.Name, _mainControl.TelegramCheckBox);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.TelegramTokenTextBox.Name, _mainControl.TelegramTokenTextBox);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.TelegramChatIdTextBox.Name, _mainControl.TelegramChatIdTextBox);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.PushBulletCheckbox.Name, _mainControl.PushBulletCheckbox);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.PushBulletTokenTextBox.Name, _mainControl.PushBulletTokenTextBox);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.PushBulletDeviceIdTextBox.Name, _mainControl.PushBulletDeviceIdTextBox);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.DiscordCheckBox.Name, _mainControl.DiscordCheckBox);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.DiscordWebhookTextBox.Name, _mainControl.DiscordWebhookTextBox);
            _xmlSettingsSerializer.AddControlSetting(_mainControl.DiscordUsernameTextBox.Name, _mainControl.DiscordUsernameTextBox);

            if (File.Exists(_settingsFile))
            {
                using (var fileStream = new FileStream(_settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var xmlTextReader = new XmlTextReader(fileStream))
                {
                    try
                    {
                        while (xmlTextReader.Read())
                        {
                            if (xmlTextReader.NodeType != XmlNodeType.Element)
                                continue;

                            if (xmlTextReader.LocalName != "SettingsSerializer") 
                                continue;

                            if (_xmlSettingsSerializer.ImportFromXml(xmlTextReader) == 0) 
                                break;

                            InternalCreateDefaultConfiguration();
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        _pluginData.lblPluginStatus.Text = "Error loading settings";
                    }

                    xmlTextReader.Close();
                }
            }
            else
                InternalCreateDefaultConfiguration();

            foreach (var language in _mainControl.LanguageComboBox.Items.OfType<Language>())
            {
                if (!language.Name.Equals(_mainControl.LanguageValue.Text)) 
                    continue;

                _mainControl.LanguageComboBox.SelectedItem = language;
                break;
            }

            _logger.Write($"Language: {_mainControl.LanguageValue.Text}", LogLevel.Debug);
            _logger.Write($"Disable Toasts: {_mainControl.DisableToasts.Checked}", LogLevel.Debug);
            _logger.Write($"Make Toasts Persistent: {_mainControl.PersistToasts.Checked}", LogLevel.Debug);
            _logger.Write($"Enable Legacy Toasts: {_mainControl.EnableActToast.Checked}", LogLevel.Debug);
            _logger.Write($"Enable Text To Speech: {_mainControl.TtsCheckBox.Checked}", LogLevel.Debug);
            if(_mainControl.TtsCheckBox.Checked)
            {
                _logger.Write($"Selected TTS Voice: {_mainControl.TtsVoicesComboBox.SelectedValue}", LogLevel.Debug);
            }
            _logger.Write($"Enable Discord Notifications: {_mainControl.DiscordCheckBox.Checked}", LogLevel.Debug);
            if(_mainControl.DiscordCheckBox.Checked)
            {
                _logger.Write($"Discord Webhook URL: {_mainControl.DiscordWebhookTextBox.Text}", LogLevel.Debug);
                _logger.Write($"Discord Username: {_mainControl.DiscordUsernameTextBox.Text}", LogLevel.Debug);
            }
            _logger.Write($"Enable Telegram Notifications: {_mainControl.TelegramCheckBox.Checked}", LogLevel.Debug);
            if(_mainControl.TelegramCheckBox.Checked)
            {
                _logger.Write($"Telegram Token: {_mainControl.TelegramTokenTextBox.Text}", LogLevel.Debug);
                _logger.Write($"Telegram ChatId: {_mainControl.TelegramChatIdTextBox.Text}", LogLevel.Debug);
            }
            _logger.Write($"Enable Pushbullet Notifications: {_mainControl.PushBulletCheckbox.Checked}", LogLevel.Debug);
            if(_mainControl.TelegramCheckBox.Checked)
            {
                _logger.Write($"Pushbullet Token: {_mainControl.PushBulletTokenTextBox.Text}", LogLevel.Debug);
                _logger.Write($"Pushbullet ChatId: {_mainControl.PushBulletDeviceIdTextBox.Text}", LogLevel.Debug);
            }
            _logger.Write($"Enable Test Environment: {_mainControl.EnableTestEnvironment.Checked}", LogLevel.Debug);
            _logger.Write($"Log Level Selected: {_mainControl.LogLevelComboBox.SelectedValue}", LogLevel.Debug);
            _logger.Write("Settings Loaded!", LogLevel.Debug);
        }

        private void InternalCreateDefaultConfiguration()
        {
            _mainControl.PersistToasts.Checked = true;
            _mainControl.LanguageValue.Text = "English";
            SaveSettings();
        }

        public void SaveSettings()
        {
            try
            {
                _logger.Write("Saving Settings...", LogLevel.Debug);
                using (var fileStream = new FileStream(_settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (var xmlTextWriter = new XmlTextWriter(fileStream, Encoding.UTF8) { Formatting = Formatting.Indented, Indentation = 1, IndentChar = '\t' })
                {
                    xmlTextWriter.WriteStartDocument(true);
                    xmlTextWriter.WriteStartElement("Config"); // <Config>
                    xmlTextWriter.WriteStartElement("SettingsSerializer"); // <Config><SettingsSerializer>
                    _xmlSettingsSerializer.ExportToXml(xmlTextWriter); // Fill the SettingsSerializer XML
                    xmlTextWriter.WriteEndElement(); // </SettingsSerializer>
                    xmlTextWriter.WriteEndElement(); // </Config>
                    xmlTextWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
                    xmlTextWriter.Flush(); // Flush the file buffer to disk
                    xmlTextWriter.Close();

                    _logger.Write("Settings Saved!", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                _logger.Write(ex, "Error saving settings", LogLevel.Error);
            }
        }

        public void Dispose()
        {
            _logger = null;
            _mainControl = null;
            _xmlSettingsSerializer = null;
            _pluginData = null;
            _instance = null;
        }
    }
    // ReSharper restore InconsistentNaming
}