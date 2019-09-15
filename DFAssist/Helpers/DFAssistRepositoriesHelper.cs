using System;
using System.Reflection;
using System.Threading.Tasks;
using Advanced_Combat_Tracker;
using DFAssist.Contracts.DataModel;
using DFAssist.Contracts.Repositories;
using Splat;

namespace DFAssist.Helpers
{
    // ReSharper disable InconsistentNaming
    public class DFAssistRepositoriesHelper : IDisposable
    {
        private static DFAssistRepositoriesHelper _instance;
        public static DFAssistRepositoriesHelper Instance => _instance ?? (_instance = new DFAssistRepositoriesHelper());

        private ActPluginData _pluginData;
        private MainControl _mainControl;
        private IActLogger _logger;
        private IDataRepository _dataRepository;
        private ILocalizationRepository _localizationRepository;

        private string _lastSelectedLanguage;

        public DFAssistRepositoriesHelper()
        {
            _pluginData = Locator.Current.GetService<ActPluginData>();
            _mainControl = Locator.Current.GetService<MainControl>();
            _localizationRepository = Locator.Current.GetService<ILocalizationRepository>();
            _dataRepository = Locator.Current.GetService<IDataRepository>();
            _logger = Locator.Current.GetService<IActLogger>();
        }

        public Task UpdateData()
        {
            return new Task(() =>
            {
                var pluginFolder = _pluginData.pluginFile.Directory?.FullName;
                _localizationRepository.WebUpdate(pluginFolder);
                _dataRepository.WebUpdate(pluginFolder);
                LoadData();
            });
        }

        public void LoadData(Language defaultLanguage = null)
        {
            _mainControl.LanguageComboBox.SelectedValueChanged -= LanguageComboBox_SelectedValueChanged;

            var newLanguage = defaultLanguage ?? (Language)_mainControl.LanguageComboBox.SelectedItem;
            if (!string.IsNullOrWhiteSpace(_lastSelectedLanguage) && newLanguage.Code.Equals(_lastSelectedLanguage))
                return;

            _lastSelectedLanguage = newLanguage.Code;
            _mainControl.LanguageValue.Text = newLanguage.Name;

            var pluginFolder = _pluginData.pluginFile.Directory?.FullName;
            _localizationRepository.LocalUpdate(pluginFolder, newLanguage.Code);
            _dataRepository.LocalUpdate(pluginFolder, newLanguage.Code);

            UpdateTranslations();

            _mainControl.LanguageComboBox.SelectedIndexChanged -= LanguageComboBox_SelectedValueChanged;
            _mainControl.LanguageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedValueChanged;
        }

        private void UpdateTranslations()
        {
            _logger.Write("Updating UI...", LogLevel.Debug);

            _mainControl.LanguageLabel.Text = _localizationRepository.GetText("ui-language-display-text");
            _mainControl.ClearLogButton.Text = _localizationRepository.GetText("ui-log-clear-display-text");
            _mainControl.EnableTestEnvironment.Text = _localizationRepository.GetText("ui-enable-test-environment");
            _mainControl.FlashTaskbar.Text = _localizationRepository.GetText("ui-flash-taskbar");
            _mainControl.TtsCheckBox.Text = _localizationRepository.GetText("ui-enable-tts");
            _mainControl.PersistToasts.Text = _localizationRepository.GetText("ui-persist-toasts");
            _mainControl.EnableActToast.Text = _localizationRepository.GetText("ui-enable-act-toasts");
            _mainControl.DisableToasts.Text = _localizationRepository.GetText("ui-disable-toasts");
            _mainControl.AppTitle.Text = $"{nameof(DFAssist)} v{Assembly.GetExecutingAssembly().GetName().Version} | ";
            _mainControl.GeneralSettings.Text = _localizationRepository.GetText("ui-general-settings-group");
            _mainControl.ToastSettings.Text = _localizationRepository.GetText("ui-toast-settings-group");
            _mainControl.TtsSettings.Text = _localizationRepository.GetText("ui-tts-settings-group");
            _mainControl.TelegramSettings.Text = _localizationRepository.GetText("ui-telegram-settings-group");
            _mainControl.TelegramCheckBox.Text = _localizationRepository.GetText("ui-telegram-display-text");
            _mainControl.TelegramChatIdLabel.Text = _localizationRepository.GetText("ui-telegram-chatid-display-text");
            _mainControl.TelegramTokenLabel.Text = _localizationRepository.GetText("ui-telegram-token-display-text");
            _mainControl.PushBulletSettings.Text = _localizationRepository.GetText("ui-pushbullet-settings-group");
            _mainControl.PushBulletCheckbox.Text = _localizationRepository.GetText("ui-pushbullet-display-text");
            _mainControl.PushBulletDeviceIdlabel.Text = _localizationRepository.GetText("ui-pushbullet-deviceid-display-text");
            _mainControl.PushBulletTokenLabel.Text = _localizationRepository.GetText("ui-pushbullet-token-display-text");
            _mainControl.TestSettings.Text = _localizationRepository.GetText("ui-test-settings-group");

            _logger.Write("UI Updated!", LogLevel.Debug);
        }

        private void LanguageComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        public void Dispose()
        {
            if (_mainControl.LanguageComboBox != null)
                _mainControl.LanguageComboBox.SelectedIndexChanged -= LanguageComboBox_SelectedValueChanged;

            _logger = null;
            _pluginData = null;
            _localizationRepository = null;
            _dataRepository = null;
            _mainControl = null;
            _instance = null;
        }
    }
    // ReSharper restore InconsistentNaming
}