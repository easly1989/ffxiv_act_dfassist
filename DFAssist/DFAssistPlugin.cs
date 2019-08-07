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
    /// before the AssemblyResolve event subscription) we need to move the code await from the
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
            Locator.CurrentMutable.RegisterConstant(new Logger(), typeof(IActLogger));
            Locator.CurrentMutable.RegisterConstant(new LocalizationRepository(), typeof(ILocalizationRepository));
            Locator.CurrentMutable.RegisterConstant(new DataRepository(), typeof(IDataRepository));
            Locator.CurrentMutable.RegisterConstant(new FFXIVPacketHandler(), typeof(IPacketHandler));
        }

        public void InitPlugin(IActPluginV1 plugin)
        {
            if (_pluginInitializing)
                return;

            if (!EnsureActMainFormIsLoaded())
                return;

            if (!FFXIVPluginHelper.Instance.Check(_pluginData, ffPluginIsEnabled =>
             {
                 if (ffPluginIsEnabled)
                     return;

                 _pluginData.cbEnabled.Checked = false;
                 // todo: check if the next call is really needed
                 //_pluginData.pluginObj.DeInitPlugin();))
             }))
                return;

            _pluginInitializing = true;
            ActGlobals.oFormActMain.Shown -= ActMainFormOnShown;

            InitializePluginVariables(plugin);
            
            _logger = Locator.Current.GetService<IActLogger>();
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
                new Language { Name = "Français", Code = "fr-fr" }
            };
            _mainControl.LanguageComboBox.DisplayMember = "Name";
            _mainControl.LanguageComboBox.ValueMember = "Code";

            _pluginData.tpPluginSpace.Controls.Add(_mainControl);

            ACTPluginSettingsHelper.Instance.LoadSettings();
            DFAssistRepositoriesHelper.Instance.LoadData();
            DFAssistUIInteractionHelper.Instance.Subscribe();

            _pluginData.lblPluginStatus.Text = "Starting...";
            _pluginData.lblPluginStatus.Text = _localizationRepository.GetText("l-plugin-started");
            _pluginData.tpPluginSpace.Text = _localizationRepository.GetText("app-name");

            _logger.Write("Plugin Started!", LogLevel.Debug);

            FFXIVNetworkProcessHelper.Instance.Subscribe();

            IsPluginEnabled = true;
            _logger.Write("Plugin Enabled", LogLevel.Debug);

            ACTPluginUpdateHelper.Instance.Subscribe();

            _pluginInitializing = false;
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
            CleanLocatorMutable();
        }

        public void OnNetworkEventReceived(EventType eventType, int[] args)
        {
            if(eventType != EventType.MATCH_ALERT)
                return;

            var isRoulette = args[0] != 0;
            var title = isRoulette ? _dataRepository.GetRoulette(args[0]).Name : _localizationRepository.GetText("ui-dutyfound");
            var testing = _mainControl.EnableTestEnvironment.Checked ? "[Code: " + args[1] + "] " : string.Empty;
            var instanceName = _dataRepository.GetInstance(args[1]).Name;

            ToastHelper.Instance.SendNotification(title, instanceName, testing, isRoulette);
            TTSHelper.Instance.SendNotification(instanceName);
        }

        private void InitializePluginVariables(IActPluginV1 plugin)
        {
            if(!Locator.CurrentMutable.HasRegistration(typeof(MainControl)))
            {
                _mainControl = plugin as MainControl;
                Locator.CurrentMutable.Register(() => _mainControl);
            }

            if(!Locator.CurrentMutable.HasRegistration(typeof(ActPluginData)))
            {
                _pluginData = ActGlobals.oFormActMain.PluginGetSelfData(plugin);
                Locator.CurrentMutable.Register(() => _pluginData);
            }
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
            ToastHelper.Instance.Dispose();
            TTSHelper.Instance.Dispose();
            DFAssistRepositoriesHelper.Instance.Dispose();
            DFAssistUIInteractionHelper.Instance.Dispose();
            FFXIVNetworkProcessHelper.Instance.Dispose();
            FFXIVPluginHelper.Instance.Dispose();
            ACTPluginUpdateHelper.Instance.Dispose();
            ACTPluginSettingsHelper.Instance.Dispose();
        }

        private void SetNullOwnedObjects()
        {
            _logger = null;
            _pluginData = null;
            _localizationRepository = null;
            _instance = null;
        }

        private void CleanLocatorMutable()
        {
            Locator.CurrentMutable.UnregisterAll(typeof(IActLogger));
            Locator.CurrentMutable.UnregisterAll(typeof(ILocalizationRepository));
            Locator.CurrentMutable.UnregisterAll(typeof(IDataRepository));
            Locator.CurrentMutable.UnregisterAll(typeof(IPacketHandler));
            Locator.CurrentMutable.UnregisterAll(typeof(MainControl));
            Locator.CurrentMutable.UnregisterAll(typeof(ActPluginData));
        }
    }
    // ReSharper restore InconsistentNaming
}