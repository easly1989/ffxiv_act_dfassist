using System;
using System.Reflection;
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

        private MainControl _mainControl;
        private IActLogger _logger;
        private IDataRepository _dataRepository;
        private ILocalizationRepository _localizationRepository;

        private string _lastSelectedLanguage;

        public DFAssistRepositoriesHelper()
        {
            _mainControl = Locator.Current.GetService<MainControl>();
            _localizationRepository = Locator.Current.GetService<ILocalizationRepository>();
            _dataRepository = Locator.Current.GetService<IDataRepository>();
            _logger = Locator.Current.GetService<IActLogger>();
        }

        public void LoadData(Language defaultLanguage = null)
        {
            _mainControl.LanguageComboBox.SelectedValueChanged -= LanguageComboBox_SelectedValueChanged;

            var newLanguage = defaultLanguage ?? (Language)_mainControl.LanguageComboBox.SelectedItem;
            if (!string.IsNullOrWhiteSpace(_lastSelectedLanguage) && newLanguage.Code.Equals(_lastSelectedLanguage))
                return;

            _lastSelectedLanguage = newLanguage.Code;
            _mainControl.LanguageValue.Text = newLanguage.Name;

            _localizationRepository.LocalUpdate(newLanguage.Code);
            _dataRepository.LocalUpdate(newLanguage.Code);

            UpdateTranslations();

            _mainControl.LanguageComboBox.SelectedValueChanged += LanguageComboBox_SelectedValueChanged;
        }

        private void UpdateTranslations()
        {
            _logger.Write("Updating UI...", LogLevel.Debug);

            _mainControl.LanguageLabel.Text = _localizationRepository.GetText("ui-language-display-text");
            _mainControl.ClearLogButton.Text = _localizationRepository.GetText("ui-log-clear-display-text");
            _mainControl.EnableTestEnvironment.Text = _localizationRepository.GetText("ui-enable-test-environment");
            _mainControl.TtsCheckBox.Text = _localizationRepository.GetText("ui-enable-tts");
            _mainControl.PersistToasts.Text = _localizationRepository.GetText("ui-persist-toasts");
            _mainControl.EnableLegacyToast.Text = _localizationRepository.GetText("ui-enable-legacy-toasts");
            _mainControl.DisableToasts.Text = _localizationRepository.GetText("ui-disable-toasts");
            _mainControl.AppTitle.Text = $"{_localizationRepository.GetText("app-name")} v{Assembly.GetExecutingAssembly().GetName().Version} | ";
            _mainControl.GeneralSettings.Text = _localizationRepository.GetText("ui-general-settings-group");
            _mainControl.ToastSettings.Text = _localizationRepository.GetText("ui-toast-settings-group");
            _mainControl.TtsSettings.Text = _localizationRepository.GetText("ui-tts-settings-group");
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
                _mainControl.LanguageComboBox.SelectedValueChanged -= LanguageComboBox_SelectedValueChanged;

            _logger = null;
            _localizationRepository = null;
            _dataRepository = null;
            _mainControl = null;
            _instance = null;
        }
    }
    // ReSharper restore InconsistentNaming
}