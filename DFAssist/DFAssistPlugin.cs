using System;
using System.Reflection;
using System.Speech.Synthesis;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using DFAssist.Contracts;
using DFAssist.Contracts.DataModel;
using DFAssist.Contracts.Repositories;
using DFAssist.Core.Network;
using DFAssist.Core.Repositories;
using DFAssist.Helpers;
using Splat;
using static Splat.Locator;

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
        private MainControl _mainControl;
        private ActPluginData _pluginData;
        
        private bool _pluginInitializing;
        
        public bool IsPluginEnabled { get; private set; }

        public DFAssistPlugin()
        {
            CurrentMutable.RegisterConstant(new Logger(), typeof(IActLogger));
            CurrentMutable.RegisterConstant(new LocalizationRepository(), typeof(ILocalizationRepository));
            CurrentMutable.RegisterConstant(new DataRepository(), typeof(IDataRepository));
            CurrentMutable.RegisterConstant(new FFXIVPacketHandler(), typeof(IPacketHandler));
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
            
            _logger = Current.GetService<IActLogger>();
            _localizationRepository = Current.GetService<ILocalizationRepository>();
            
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

        private void InitializePluginVariables(IActPluginV1 plugin)
        {
            if(!CurrentMutable.HasRegistration(typeof(MainControl)))
            {
                _mainControl = plugin as MainControl;
                CurrentMutable.Register(() => _mainControl);
            }

            if(!CurrentMutable.HasRegistration(typeof(ActPluginData)))
            {
                _pluginData = ActGlobals.oFormActMain.PluginGetSelfData(plugin);
                CurrentMutable.Register(() => _pluginData);
            }
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
    }
    // ReSharper restore InconsistentNaming
}