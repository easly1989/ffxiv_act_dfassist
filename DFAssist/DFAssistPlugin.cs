using System;
using System.Reflection;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using DFAssist.Contracts;
using DFAssist.Contracts.DataModel;
using DFAssist.Contracts.Duty;
using DFAssist.Contracts.Repositories;
using DFAssist.Core.Network;
using DFAssist.Core.Repositories;
using DFAssist.Helpers;
using Splat;

namespace DFAssist
{
    /// <summary>
    /// This class is needed because of the way ACT initializes the plugin
    /// To avoid that ACT loads the referenced DLL before the plugin initialization (and thus
    /// before the AssemblyResolve event subscription) we need to move the code away from the
    /// IActPluginV1 implementation.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class DFAssistPlugin
    {
        public const string AppId = "Advanced Combat Tracker";

        private static DFAssistPlugin _instance;
        public static DFAssistPlugin Instance => _instance ?? (_instance = new DFAssistPlugin());

        private IActLogger _logger;
        private ILocalizationRepository _localizationRepository;
        private IDataRepository _dataRepository;
        private MainControl _mainControl;
        private ActPluginData _pluginData;

        private bool _pluginInitializing;

        public bool IsPluginEnabled { get; private set; }

        public DFAssistPlugin()
        {
            _logger = new Logger();
            Locator.CurrentMutable.RegisterConstant(_logger, typeof(ILogger));
            Locator.CurrentMutable.RegisterConstant(_logger, typeof(IActLogger));
            Locator.CurrentMutable.RegisterConstant(new LocalizationRepository(), typeof(ILocalizationRepository));
            Locator.CurrentMutable.RegisterConstant(new DataRepository(), typeof(IDataRepository));
            Locator.CurrentMutable.RegisterConstant(new FFXIVPacketHandler(), typeof(IPacketHandler));
        }

        public void InitPlugin(MainControl mainControl)
        {
            if (_pluginInitializing)
                return;

            InitializePluginVariables(mainControl);
            
            if (!EnsureActMainFormIsLoaded())
                return;

            if (!FFXIVPluginHelper.Instance.Check(_pluginData, ffPluginIsEnabled =>
             {
                 if (ffPluginIsEnabled)
                     return;

                 _pluginData.cbEnabled.Checked = false;
             }))
                return;

            _pluginInitializing = true;
            ActGlobals.oFormActMain.Shown -= ActMainFormOnShown;

            _localizationRepository = Locator.Current.GetService<ILocalizationRepository>();
            _dataRepository = Locator.Current.GetService<IDataRepository>();

            _logger.SetTextBox(_mainControl.LoggingRichTextBox);
            _logger.Write("Plugin Init", LogLevel.Debug);
            _logger.Write($"Plugin Version: {Assembly.GetExecutingAssembly().GetName().Version}", LogLevel.Debug);

            _mainControl.LanguageComboBox.DataSource = new[]
            {
                new Language { Name = "English", Code = "en-us" },
                new Language { Name = "한국어", Code = "ko-kr" },
                new Language { Name = "日本語", Code = "ja-jp" },
                new Language { Name = "Français", Code = "fr-fr" },
                new Language { Name = "Deutsch", Code = "de-de" },
            };
            _mainControl.LanguageComboBox.DisplayMember = "Name";
            _mainControl.LanguageComboBox.ValueMember = "Code";

            _pluginData.tpPluginSpace.Controls.Add(_mainControl);

            ACTPluginSettingsHelper.Instance.LoadSettings();
            var updateTask = DFAssistRepositoriesHelper.Instance.UpdateData();
            updateTask.ContinueWith(_ =>
            {
                DFAssistRepositoriesHelper.Instance.LoadData();
                DFAssistUIInteractionHelper.Instance.Subscribe();

                _pluginData.lblPluginStatus.Text = "Starting...";
                _pluginData.lblPluginStatus.Text = "Plugin Started!";
                _pluginData.tpPluginSpace.Text = nameof(DFAssist);

                _logger.Write("Plugin Started!", LogLevel.Debug);

                FFXIVNetworkProcessHelper.Instance.Subscribe();

                IsPluginEnabled = true;
                _logger.Write("Plugin Enabled", LogLevel.Debug);

                ACTPluginUpdateHelper.Instance.Subscribe();

                _pluginInitializing = false;
            });
            updateTask.Start();
        }

        public void DeInitPlugin()
        {
            if (!IsPluginEnabled)
                return;

            IsPluginEnabled = false;

            ACTPluginSettingsHelper.Instance.SaveSettings();

            _pluginData.lblPluginStatus.Text = "Plugin Stopped!";
            _logger.SetTextBox(null);

            DisposeOwnedObjects();
            SetNullOwnedObjects();
        }

        public void OnNetworkEventReceived(EventType eventType, int[] args)
        {
            if (eventType != EventType.MATCH_ALERT)
                return;

            var defaultTitle = _localizationRepository.GetText("ui-tts-dutyfound");
            var title = args[0] != 0 ? _dataRepository.GetRoulette(args[0]).Name : defaultTitle;
            var testing = _mainControl.EnableTestEnvironment.Checked ? "[Code: " + args[1] + "] " : string.Empty;
            var instanceName = _dataRepository.GetInstance(args[1]).Name;


            TaskbarFlashHelper.Instance.SendNotification();
            ToastHelper.Instance.SendNotification(title, instanceName, testing);
            TTSHelper.Instance.SendNotification(defaultTitle, instanceName, testing);
            TelegramHelper.Instance.SendNotification(title, instanceName, testing);
            PushBulletHelper.Instance.SendNotification(title, instanceName, testing);
            DiscordHelper.Instance.SendNotification(title, instanceName, testing);
        }

        private void InitializePluginVariables(MainControl mainControl)
        {
            if(_mainControl != null)
                return;

            _mainControl = mainControl;
            _pluginData = ActGlobals.oFormActMain.PluginGetSelfData(mainControl.Plugin);

            Locator.CurrentMutable.Register(() => _mainControl);
            Locator.CurrentMutable.Register(() => _pluginData);
        }

        private bool EnsureActMainFormIsLoaded()
        {
            foreach (Form formLoaded in Application.OpenForms)
            {
                if (formLoaded != ActGlobals.oFormActMain)
                    continue;

                return true;
            }

            ActGlobals.oFormActMain.Shown += ActMainFormOnShown;
            return false;
        }

        private void ActMainFormOnShown(object sender, EventArgs e)
        {
            InitPlugin(_mainControl);
        }

        private void DisposeOwnedObjects()
        {
            DiscordHelper.Instance.Dispose();
            ToastHelper.Instance.Dispose();
            TTSHelper.Instance.Dispose();
            TelegramHelper.Instance.Dispose();
            PushBulletHelper.Instance.Dispose();
            DFAssistRepositoriesHelper.Instance.Dispose();
            DFAssistUIInteractionHelper.Instance.Dispose();
            FFXIVNetworkProcessHelper.Instance.Dispose();
            FFXIVPluginHelper.Instance.Dispose();
            ACTPluginUpdateHelper.Instance.Dispose();
            ACTPluginSettingsHelper.Instance.Dispose();

            _logger.Dispose();
        }

        private void SetNullOwnedObjects()
        {
            _logger = null;
            _mainControl = null;
            _pluginData = null;
            _localizationRepository = null;
            _instance = null;
        }
    }
    // ReSharper restore InconsistentNaming
}