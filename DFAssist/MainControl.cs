using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Advanced_Combat_Tracker;
using DFAssist.Contracts;
using DFAssist.Contracts.DataModel;
using DFAssist.Contracts.Duty;
using DFAssist.Contracts.Repositories;
using DFAssist.Core.Network;
using DFAssist.Core.Toast;
using DFAssist.LegacyToasts;
using Microsoft.Win32;
using Splat;
using Application = System.Windows.Forms.Application;
using FontStyle = System.Drawing.FontStyle;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Timer = System.Windows.Forms.Timer;

namespace DFAssist
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// This class is needed because of the way ACT initializes the plugin
    /// To avoid that ACT loads the referenced DLL before the plugin initialization (and thus
    /// before the AssemblyResolve event subscription) we need to move the code await from the
    /// IActPluginV1 implementation.
    ///
    /// As suggested by EQEditu, this class may become static
    /// </summary>
    public class DFAssistPlugin
    {
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            throw new NotImplementedException();
        }

        public void DeInitPlugin()
        {
            throw new NotImplementedException();
        }
    }
    // ReSharper restore InconsistentNaming

    public class MainControl : UserControl, IActPluginV1
    {
        private const string AppId = "Advanced Combat Tracker";

        private readonly ConcurrentDictionary<int, ProcessNetwork> _networks;
        private readonly string _settingsFile;
        private readonly SpeechSynthesizer _synth;
        private readonly Logger _logger;
        private readonly ILocalizationRepository _localizationRepository;
        private readonly IDataRepository _dataRepository;
        private readonly IPacketHandler _packetHandler;

        private ActPluginData _ffxivPlugin;

        private TabControl _appTabControl;
        private Label _appTitle;
        private Button _button1;
        private LinkLabel _copyrightLink;
        private CheckBox _disableToasts;
        private CheckBox _enableLegacyToast;
        private CheckBox _enableTestEnvironment;
        private GroupBox _generalSettings;
        private LegacyToast _lastToast;

        private bool _isPluginEnabled;
        private Label _label1;
        private Label _labelStatus;
        private TabPage _labelTab;
        private ComboBox _languageComboBox;
        private TextBox _languageValue;
        private bool _mainFormIsLoaded;
        private TableLayoutPanel _mainTableLayout;
        private TabPage _mainTabPage;
        private CheckBox _persistToasts;
        private bool _pluginInitializing;
        private RichTextBox _richTextBox1;
        private Language _selectedLanguage;
        private TabPage _settingsPage;
        private TableLayoutPanel _settingsTableLayout;
        private GroupBox _testSettings;
        private Timer _timer;
        private GroupBox _toastSettings;
        private CheckBox _ttsCheckBox;
        private GroupBox _ttsSettings;
        private SettingsSerializer _xmlSettingsSerializer;
        private Panel _settingsPanel;

        #region Load Methods

        private void LoadData(Language defaultLanguage = null)
        {
            var newLanguage = defaultLanguage ?? (Language)_languageComboBox.SelectedItem;
            if(_selectedLanguage != null && newLanguage.Code.Equals(_selectedLanguage.Code))
                return;

            _selectedLanguage = newLanguage;
            _languageValue.Text = _selectedLanguage.Name;

            _localizationRepository.LocalUpdate(_selectedLanguage.Code);
            _dataRepository.LocalUpdate(_selectedLanguage.Code);

            UpdateTranslations();
        }

        #endregion

        #region WinForm Required

        public MainControl()
        {
            InitializeComponent();

            _synth = new SpeechSynthesizer();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            var bootstrapper = new Bootstrapper();
            bootstrapper.Register();

            _logger = Locator.Current.GetService<ILogger>() as Logger;
            _localizationRepository = Locator.Current.GetService<ILocalizationRepository>();
            _dataRepository = Locator.Current.GetService<IDataRepository>();
            _packetHandler = Locator.Current.GetService<IPacketHandler>();

            _settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config", "DFAssist.config.xml");
            _networks = new ConcurrentDictionary<int, ProcessNetwork>();

            foreach(Form formLoaded in Application.OpenForms)
            {
                if(formLoaded != ActGlobals.oFormActMain)
                    continue;

                _mainFormIsLoaded = true;
                break;
            }
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs e)
        {
            // if any of the assembly cannot be loaded, then the plugin cannot be started
            if(!AssemblyResolver.LoadAssembly(e, _labelStatus, out var result))
                throw new Exception("Assembly load failed.");

            return result;
        }

        private void InitializeComponent()
        {
            _label1 = new Label();
            _languageValue = new TextBox();
            _languageComboBox = new ComboBox();
            _enableTestEnvironment = new CheckBox();
            _ttsCheckBox = new CheckBox();
            _persistToasts = new CheckBox();
            _enableLegacyToast = new CheckBox();
            _disableToasts = new CheckBox();
            _appTabControl = new TabControl();
            _mainTabPage = new TabPage();
            _mainTableLayout = new TableLayoutPanel();
            _button1 = new Button();
            _richTextBox1 = new RichTextBox();
            _appTitle = new Label();
            _copyrightLink = new LinkLabel();
            _settingsPage = new TabPage();
            _settingsPanel = new Panel();
            _settingsTableLayout = new TableLayoutPanel();
            _ttsSettings = new GroupBox();
            _toastSettings = new GroupBox();
            _generalSettings = new GroupBox();
            _testSettings = new GroupBox();
            _appTabControl.SuspendLayout();
            _mainTabPage.SuspendLayout();
            _mainTableLayout.SuspendLayout();
            _settingsPage.SuspendLayout();
            _settingsPanel.SuspendLayout();
            _settingsTableLayout.SuspendLayout();
            _ttsSettings.SuspendLayout();
            _toastSettings.SuspendLayout();
            _generalSettings.SuspendLayout();
            _testSettings.SuspendLayout();
            SuspendLayout();
            // 
            // _label1
            // 
            _label1.AutoSize = true;
            _label1.Location = new Point(3, 23);
            _label1.Name = "_label1";
            _label1.TabStop = false;
            _label1.Text = "Language";
            //
            // _languageValue
            //
            _languageValue.Visible = false;
            _languageValue.Name = "_languageValue";
            _languageValue.TabStop = false;
            // 
            // _languageComboBox
            // 
            _languageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _languageComboBox.FormattingEnabled = true;
            _languageComboBox.Location = new Point(80, 23);
            _languageComboBox.Name = "_languageComboBox";
            _languageComboBox.Size = new Size(130, 25);
            _languageComboBox.TabIndex = 0;
            // 
            // _disableToasts
            // 
            _disableToasts.AutoSize = true;
            _disableToasts.Location = new Point(6, 22);
            _disableToasts.Name = "_disableToasts";
            _disableToasts.TabIndex = 1;
            _disableToasts.Text = "Disable Toasts";
            _disableToasts.UseVisualStyleBackColor = true;
            _disableToasts.CheckStateChanged += DisableToastsOnCheckedChanged;
            // 
            // _persistToasts
            // 
            _persistToasts.AutoSize = true;
            _persistToasts.Location = new Point(6, 45);
            _persistToasts.Name = "_persistToasts";
            _persistToasts.TabIndex = 2;
            _persistToasts.Text = "Make Toasts Persistent";
            _persistToasts.UseVisualStyleBackColor = true;
            _persistToasts.CheckStateChanged += PersistToastsOnCheckedChanged;
            // 
            // _enableLegacyToast
            // 
            _enableLegacyToast.AutoSize = true;
            _enableLegacyToast.Location = new Point(6, 68);
            _enableLegacyToast.Name = "_enableLegacyToast";
            _enableLegacyToast.TabIndex = 3;
            _enableLegacyToast.Text = "Enable Legacy Toasts";
            _enableLegacyToast.UseVisualStyleBackColor = true;
            _enableLegacyToast.CheckStateChanged += EnableLegacyToastsOnCheckedChanged;
            // 
            // _ttsCheckBox
            // 
            _ttsCheckBox.AutoSize = true;
            _ttsCheckBox.Location = new Point(6, 22);
            _ttsCheckBox.Name = "_ttsCheckBox";
            _ttsCheckBox.TabIndex = 4;
            _ttsCheckBox.Text = "Enable Text To Speech";
            _ttsCheckBox.UseVisualStyleBackColor = true;
            _ttsCheckBox.CheckStateChanged += EnableTtsOnCheckedChanged;
            // 
            // _enableTestEnvironment
            // 
            _enableTestEnvironment.AutoSize = true;
            _enableTestEnvironment.Location = new Point(6, 20);
            _enableTestEnvironment.Name = "_enableTestEnvironment";
            _enableTestEnvironment.TabIndex = 5;
            _enableTestEnvironment.Text = "Enable Test Environment";
            _enableTestEnvironment.UseVisualStyleBackColor = true;
            // 
            // _appTabControl
            //
            _appTabControl.Dock = DockStyle.Fill;
            _appTabControl.Controls.Add(_mainTabPage);
            _appTabControl.Controls.Add(_settingsPage);
            _appTabControl.Location = new Point(4, 4);
            _appTabControl.Name = "_appTabControl";
            _appTabControl.SelectedIndex = 0;
            _appTabControl.TabStop = false;
            // 
            // _mainTabPage
            // 
            _mainTabPage.Dock = DockStyle.Fill;
            _mainTabPage.Controls.Add(_mainTableLayout);
            _mainTabPage.Location = new Point(4, 22);
            _mainTabPage.Name = "_mainTabPage";
            _mainTabPage.Padding = new Padding(3);
            _mainTabPage.TabStop = false;
            _mainTabPage.Text = "Main";
            _mainTabPage.ToolTipText = "Shows main info and logs";
            _mainTabPage.UseVisualStyleBackColor = true;
            // 
            // _mainTableLayout
            // 
            _mainTableLayout.Dock = DockStyle.Fill;
            _mainTableLayout.ColumnCount = 3;
            _mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _mainTableLayout.Controls.Add(_button1, 2, 0);
            _mainTableLayout.Controls.Add(_richTextBox1, 0, 1);
            _mainTableLayout.Controls.Add(_appTitle, 0, 0);
            _mainTableLayout.Controls.Add(_copyrightLink, 1, 0);
            _mainTableLayout.Location = new Point(0, 3);
            _mainTableLayout.Name = "_mainTableLayout";
            _mainTableLayout.RowCount = 2;
            _mainTableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _mainTableLayout.TabStop = false;
            // 
            // _button1
            // 
            _button1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _button1.Name = "_button1";
            _button1.MinimumSize = new Size(100, 25);
            _button1.TabIndex = 0;
            _button1.Text = "Clear Logs";
            _button1.UseVisualStyleBackColor = true;
            _button1.Click += ClearLogsButton_Click;
            _button1.AutoSize = true;
            // 
            // _richTextBox1
            //
            _richTextBox1.Dock = DockStyle.Fill;
            _richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _mainTableLayout.SetColumnSpan(_richTextBox1, 3);
            _richTextBox1.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _richTextBox1.Location = new Point(3, 32);
            _richTextBox1.Name = "_richTextBox1";
            _richTextBox1.ReadOnly = true;
            _richTextBox1.TabIndex = 1;
            _richTextBox1.Text = "";
            // 
            // _appTitle
            // 
            _appTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            _appTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            _appTitle.Location = new Point(3, 0);
            _appTitle.Name = "_appTitle";
            _appTitle.TabStop = false;
            _appTitle.Text = "DFAssist ~";
            _appTitle.TextAlign = ContentAlignment.MiddleLeft;
            _appTitle.AutoSize = true;
            // 
            // _copyrightLink
            // 
            _copyrightLink.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            _copyrightLink.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            _copyrightLink.LinkBehavior = LinkBehavior.HoverUnderline;
            _copyrightLink.Location = new Point(106, 0);
            _copyrightLink.Name = "_copyrightLink";
            _copyrightLink.TabIndex = 2;
            _copyrightLink.TabStop = true;
            _copyrightLink.Text = "© easly1989";
            _copyrightLink.TextAlign = ContentAlignment.MiddleLeft;
            _copyrightLink.AutoSize = true;
            // 
            // _settingsPage
            // 
            _settingsPage.Dock = DockStyle.Fill;
            _settingsPage.Controls.Add(_settingsPanel);
            _settingsPage.Location = new Point(4, 22);
            _settingsPage.Name = "_settingsPage";
            _settingsPage.Padding = new Padding(3);
            _settingsPage.TabStop = false;
            _settingsPage.Text = "Settings";
            _settingsPage.ToolTipText = "Change Settings for DFAssist";
            _settingsPage.UseVisualStyleBackColor = true;
            // 
            // _settingsPanel
            //
            _settingsPanel.Dock = DockStyle.Fill;
            _settingsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _settingsPanel.Controls.Add(_settingsTableLayout);
            _settingsPanel.Name = "_settingsPanel";
            _settingsPanel.TabStop = false;
            _settingsPanel.AutoScroll = true;
            // 
            // _settingsTableLayout
            // 
            _settingsTableLayout.Dock = DockStyle.Fill;
            _settingsTableLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _settingsTableLayout.AutoSize = true;
            _settingsTableLayout.AutoSizeMode = AutoSizeMode.GrowOnly;
            _settingsTableLayout.ColumnCount = 1;
            _settingsTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _settingsTableLayout.Controls.Add(_generalSettings, 0, 0);
            _settingsTableLayout.Controls.Add(_toastSettings, 0, 1);
            _settingsTableLayout.Controls.Add(_ttsSettings, 0, 2);
            _settingsTableLayout.Controls.Add(_testSettings, 0, 3);
            _settingsTableLayout.Location = new Point(0, 3);
            _settingsTableLayout.Name = "_settingsTableLayout";
            _settingsTableLayout.RowCount = 5;
            _settingsTableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _settingsTableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _settingsTableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _settingsTableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _settingsTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _settingsTableLayout.TabStop = false;
            // 
            // _generalSettings
            // 
            Dock = DockStyle.Top;
            _generalSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _generalSettings.Controls.Add(_label1);
            _generalSettings.Controls.Add(_languageComboBox);
            _generalSettings.Name = "_generalSettings";
            _generalSettings.TabStop = false;
            _generalSettings.Text = "General Settings";
            // 
            // _toastSettings
            // 
            Dock = DockStyle.Top;
            _toastSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _toastSettings.Controls.Add(_disableToasts);
            _toastSettings.Controls.Add(_enableLegacyToast);
            _toastSettings.Controls.Add(_persistToasts);
            _toastSettings.Name = "_toastSettings";
            _toastSettings.TabStop = false;
            _toastSettings.Text = "Toasts Settings";
            // 
            // _ttsSettings
            // 
            Dock = DockStyle.Top;
            _ttsSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _ttsSettings.Controls.Add(_ttsCheckBox);
            _ttsSettings.Name = "_ttsSettings";
            _ttsSettings.TabStop = false;
            _ttsSettings.Text = "Text To Speech Settings";
            // 
            // _testSettings
            // 
            Dock = DockStyle.Top;
            _testSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _testSettings.Controls.Add(_enableTestEnvironment);
            _testSettings.Name = "_testSettings";
            _testSettings.TabStop = false;
            _testSettings.Text = "Test Settings";
            // 
            // MainControl
            // 
            Dock = DockStyle.Fill;
            Controls.Add(_appTabControl);
            Name = "MainControl";
            _appTabControl.ResumeLayout(false);
            _mainTabPage.ResumeLayout(false);
            _mainTableLayout.ResumeLayout(false);
            _mainTableLayout.PerformLayout();
            _settingsPage.ResumeLayout(false);
            _settingsPage.PerformLayout();
            _settingsPanel.ResumeLayout(false);
            _settingsPanel.PerformLayout();
            _settingsTableLayout.ResumeLayout(false);
            _settingsTableLayout.PerformLayout();
            _ttsSettings.ResumeLayout(false);
            _ttsSettings.PerformLayout();
            _toastSettings.ResumeLayout(false);
            _toastSettings.PerformLayout();
            _generalSettings.ResumeLayout(false);
            _generalSettings.PerformLayout();
            _testSettings.ResumeLayout(false);
            _testSettings.PerformLayout();
            ResumeLayout(false);
        }

        private void DisableToastsOnCheckedChanged(object sender, EventArgs e)
        {
            _logger.Write($"UI: [DisableToasts] Desired Value: {_disableToasts.Checked}", LogLevel.Debug);
            _enableLegacyToast.Enabled = !_disableToasts.Checked;
            _persistToasts.Enabled = _enableLegacyToast.Enabled && !_enableLegacyToast.Checked;
        }

        private void EnableLegacyToastsOnCheckedChanged(object sender, EventArgs e)
        {
            _logger.Write($"UI: [LegacyToasts] Desired Value: {_enableLegacyToast.Checked}", LogLevel.Debug);
            _persistToasts.Enabled = !_enableLegacyToast.Checked;
            ToastWindowNotification(_localizationRepository.GetText("ui-toast-notification-test-title"), _localizationRepository.GetText("ui-toast-notification-test-message"));
        }

        private void PersistToastsOnCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _logger.Write($"UI: [PersistentToasts] Desired Value: {_persistToasts.Checked}!", LogLevel.Debug);

                var keyName = $@"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\{AppId}";
                using(var key = Registry.CurrentUser.OpenSubKey(keyName, true))
                {
                    if(_persistToasts.Checked)
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

                    MessageBox.Show(_localizationRepository.GetText("ui-persistent-toast-warning-message"), _localizationRepository.GetText("ui-persistent-toast-warning-title"), MessageBoxButtons.OK);
                }
            }
            catch(Exception ex)
            {
                _logger.Write(ex, $"UI: Unable to remove/add the registry key to make Toasts persistent!", LogLevel.Error);
            }
        }

        #endregion

        #region IActPluginV1 Implementations

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            _labelStatus = pluginStatusText;
            _labelTab = pluginScreenSpace;

            if(_mainFormIsLoaded)
                OnInit();
            else
                ActGlobals.oFormActMain.Shown += ActMainFormOnShown;
        }

        private void FormActMain_UpdateCheckClicked()
        {
            const int pluginId = 71;
            try
            {
                var localDate = ActGlobals.oFormActMain.PluginGetSelfDateUtc(this);
                var remoteDate = ActGlobals.oFormActMain.PluginGetRemoteDateUtc(pluginId);
                if(localDate.AddHours(2) >= remoteDate)
                    return;

                var result = MessageBox.Show(_localizationRepository.GetText("ui-update-available-message"), _localizationRepository.GetText("ui-update-available-title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if(result != DialogResult.Yes)
                    return;

                var updatedFile = ActGlobals.oFormActMain.PluginDownload(pluginId);
                var pluginData = ActGlobals.oFormActMain.PluginGetSelfData(this);
                if(pluginData.pluginFile.Directory != null)
                    ActGlobals.oFormActMain.UnZip(updatedFile.FullName, pluginData.pluginFile.Directory.FullName);

                ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, false);
                Application.DoEvents();
                ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, true);
            }
            catch(Exception ex)
            {
                ActGlobals.oFormActMain.WriteExceptionLog(ex, "Plugin Update Check");
            }
        }

        private void OnInit()
        {
            if(_pluginInitializing)
                return;

            _pluginInitializing = true;

            if(_ffxivPlugin != null)
                _ffxivPlugin.cbEnabled.CheckedChanged -= FFXIVParsingPlugin_IsEnabledChanged;

            // Before anything else, if the FFXIV Parsing Plugin is not already initialized
            // than this plugin cannot start
            var plugins = ActGlobals.oFormActMain.ActPlugins;
            _ffxivPlugin = plugins.FirstOrDefault(x => x.lblPluginTitle.Text == "FFXIV_ACT_Plugin.dll");
            if(_ffxivPlugin == null)
            {
                _pluginInitializing = false;
                ActGlobals.oFormActMain.PluginGetSelfData(this).cbEnabled.Checked = false;
                _labelStatus.Text = "FFXIV_ACT_Plugin must be installed BEFORE DFAssist!";
                return;
            }

            if(!_ffxivPlugin.cbEnabled.Checked)
            {
                _pluginInitializing = false;
                ActGlobals.oFormActMain.PluginGetSelfData(this).cbEnabled.Checked = false;
                _labelStatus.Text = "FFXIV_ACT_Plugin must be enabled";
                return;
            }

            _ffxivPlugin.cbEnabled.CheckedChanged += FFXIVParsingPlugin_IsEnabledChanged;
            ActGlobals.oFormActMain.Shown -= ActMainFormOnShown;

            var pluginData = ActGlobals.oFormActMain.PluginGetSelfData(this);
            var enviroment = Path.GetDirectoryName(pluginData.pluginFile.ToString());
            AssemblyResolver.Initialize(enviroment);

            _logger.SetTextBox(_richTextBox1);
            _logger.Write("Plugin Init", LogLevel.Debug);
            _logger.Write($"Plugin Version: {Assembly.GetExecutingAssembly().GetName().Version}", LogLevel.Debug);

            var defaultLanguage = new Language { Name = "English", Code = "en-us" };
            _isPluginEnabled = true;

            _logger.Write("Plugin Enabled", LogLevel.Debug);

            _languageComboBox.DataSource = new[]
            {
                defaultLanguage,
                new Language {Name = "한국어", Code = "ko-kr"},
                new Language {Name = "日本語", Code = "ja-jp"},
                new Language {Name = "Français", Code = "fr-fr"}
            };
            _languageComboBox.DisplayMember = "Name";
            _languageComboBox.ValueMember = "Code";
            
            _labelTab.Controls.Add(this);
            _xmlSettingsSerializer = new SettingsSerializer(this);

            LoadSettings();
            LoadData();

            _labelStatus.Text = "Starting...";
            _labelStatus.Text = _localizationRepository.GetText("l-plugin-started");
            _labelTab.Text = _localizationRepository.GetText("app-name");

            _logger.Write("Plugin Started!", LogLevel.Debug);

            UpdateProcesses();

            _languageComboBox.SelectedValueChanged += LanguageComboBox_SelectedValueChanged;

            if(_timer == null)
            {
                _timer = new Timer { Interval = 30000 };
                _timer.Tick += Timer_Tick;
            }

            _timer.Enabled = true;
            _pluginInitializing = false;

            ActGlobals.oFormActMain.UpdateCheckClicked += FormActMain_UpdateCheckClicked;
            if(ActGlobals.oFormActMain.GetAutomaticUpdatesAllowed())
                new Thread(FormActMain_UpdateCheckClicked).Start();
        }

        private void FFXIVParsingPlugin_IsEnabledChanged(object sender, EventArgs e)
        {
            if (_ffxivPlugin.cbEnabled.Checked) 
                return;

            ActGlobals.oFormActMain.PluginGetSelfData(this).cbEnabled.Checked = false;
            DeInitPlugin();
        }

        public void DeInitPlugin()
        {
            if(!_isPluginEnabled)
                return;

            _isPluginEnabled = false;

            if(_ffxivPlugin != null)
                _ffxivPlugin.cbEnabled.CheckedChanged -= FFXIVParsingPlugin_IsEnabledChanged;

            SaveSettings();

            _labelTab = null;

            if(_labelStatus != null)
            {
                _labelStatus.Text = _localizationRepository.GetText("l-plugin-stopped");
                _labelStatus = null;
            }

            foreach(var entry in _networks)
                entry.Value.Network.StopCapture();

            _timer.Enabled = false;

            _logger.SetTextBox(null);
        }

        #endregion

        #region Update Methods

        private void UpdateProcesses()
        {
            var process = Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();
            if(process == null)
                return;
            try
            {
                if(!_networks.ContainsKey(process.Id))
                {
                    var pn = new ProcessNetwork(process, new Network());
                    _packetHandler.OnEventReceived += Network_onReceiveEvent;
                    _networks.TryAdd(process.Id, pn);
                    _logger.Write("P: FFXIV Process Selected: {process.Id}", LogLevel.Info);
                }
            }
            catch(Exception e)
            {
                _logger.Write(e, "P: Failed to set FFXIV Process", LogLevel.Error);
            }

            var toDelete = new List<int>();
            foreach(var entry in _networks)
            {
                if(entry.Value.Process.HasExited)
                {
                    entry.Value.Network.StopCapture();
                    toDelete.Add(entry.Key);
                }
                else
                {
                    if(entry.Value.Network.IsRunning)
                        entry.Value.Network.UpdateGameConnections(entry.Value.Process);
                    else
                    {
                        if(!entry.Value.Network.StartCapture(entry.Value.Process))
                            toDelete.Add(entry.Key);
                    }
                }
            }

            foreach(var t in toDelete)
            {
                try
                {
                    _networks.TryRemove(t, out _);
                    _packetHandler.OnEventReceived -= Network_onReceiveEvent;
                }
                catch(Exception e)
                {
                    _logger.Write(e, "P: Failed to remove FFXIV Process", LogLevel.Error);
                }
            }
        }

        private void UpdateTranslations()
        {
            _logger.Write("Updating UI...", LogLevel.Debug);

            _label1.Text = _localizationRepository.GetText("ui-language-display-text");
            _button1.Text = _localizationRepository.GetText("ui-log-clear-display-text");
            _enableTestEnvironment.Text = _localizationRepository.GetText("ui-enable-test-environment");
            _ttsCheckBox.Text = _localizationRepository.GetText("ui-enable-tts");
            _persistToasts.Text = _localizationRepository.GetText("ui-persist-toasts");
            _enableLegacyToast.Text = _localizationRepository.GetText("ui-enable-legacy-toasts");
            _disableToasts.Text = _localizationRepository.GetText("ui-disable-toasts");
            _appTitle.Text = $"{_localizationRepository.GetText("app-name")} v{Assembly.GetExecutingAssembly().GetName().Version} | ";
            _generalSettings.Text = _localizationRepository.GetText("ui-general-settings-group");
            _toastSettings.Text = _localizationRepository.GetText("ui-toast-settings-group");
            _ttsSettings.Text = _localizationRepository.GetText("ui-tts-settings-group");
            _testSettings.Text = _localizationRepository.GetText("ui-test-settings-group");

            _logger.Write("UI Updated!", LogLevel.Debug);
        }

        #endregion

        #region Post Method

        private static void SendToAct(string text)
        {
            ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "00|" + DateTime.Now.ToString("O") + "|0048|F|" + text);
        }

        private void PostToToastWindowsNotificationIfNeeded(EventType eventType, int[] args)
        {
            if(eventType != EventType.MATCH_ALERT)
                return;

            var title = args[0] != 0 ? _dataRepository.GetRoulette(args[0]).Name : _localizationRepository.GetText("ui-dutyfound");
            var testing = _enableTestEnvironment.Checked ? "[Code: " + args[1] + "] " : string.Empty;

            switch(eventType)
            {
                case EventType.MATCH_ALERT:

                    var instanceName = _dataRepository.GetInstance(args[1]).Name;
                    ToastWindowNotification(title, instanceName, testing, args[0] != 0);
                    TtsNotification(instanceName);
                    break;
            }
        }

        private void ToastWindowNotification(string title, string message, string testing = "", bool isRoulette = false)
        {
            _logger.Write("Request Showing Taost received...", LogLevel.Debug);
            if(_disableToasts.Checked)
            {
                _logger.Write("... Toasts are disabled!", LogLevel.Debug);
                return;
            }

            if(_enableLegacyToast.Checked)
            {
                _logger.Write("... Legacy Toasts Enabled...", LogLevel.Debug);
                try
                {
                    _logger.Write("... Closing any open Legacy Toast...", LogLevel.Debug);
                    _lastToast?.Close();
                    LegacyToastDispose();
                    Application.ThreadException += LegacyToastOnGuiUnhandedException;
                    AppDomain.CurrentDomain.UnhandledException += LegacyToastOnUnhandledException;
                    var toast = new LegacyToast(title, message, _networks) { Text = title };
                    _logger.Write("... Creating new Legacy Toast...", LogLevel.Debug);
                    _lastToast = toast;
                    _lastToast.Closing += LastToastOnClosing;
                    _lastToast.Show();
                    _logger.Write("... Legacy Toast Showing...", LogLevel.Debug);
                    _lastToast.Activate();
                }
                catch(Exception ex)
                {
                    _logger.Write(ex, "Error handling/creating Legacy Toast!", LogLevel.Error);
                    LegacyToastHandleUnhandledException(ex);
                    _lastToast?.Close();
                    LegacyToastDispose();
                }
            }
            else
            {
                _logger.Write("... Legacy Toasts Disabled...", LogLevel.Debug);
                try
                {
                    _logger.Write("... Creating new Toast...", LogLevel.Debug);
                    var toastImagePath = isRoulette ? "images/roulette.png" : "images/dungeon.png";//todo handle instance type from data
                    var attribution = _localizationRepository.GetText("app-name");
                    void ToastCallback(int code)
                    {
                        //todo handle all the return types and log it
                    }

                    if(string.IsNullOrWhiteSpace(testing))
                    {
                        WinToastWrapper.CreateToast(
                        AppId, 
                        AppId,
                        title,
                        message,
                        toastImagePath, 
                        ToastCallback,
                        attribution,
                        true);
                    }
                    else
                    {
                        WinToastWrapper.CreateToast(
                            AppId, 
                            AppId,
                            title,
                            message,
                            $"Code [{testing}]",
                            toastImagePath,
                            ToastCallback,
                            attribution);
                    }
                    _logger.Write("... Toast Showing...", LogLevel.Debug);
                }
                catch(Exception e)
                {
                    _logger.Write(e, "UI: Unable to show toast notification", LogLevel.Error);
                }
            }
        }

        private void LegacyToastDispose()
        {
            Application.ThreadException -= LegacyToastOnGuiUnhandedException;
            AppDomain.CurrentDomain.UnhandledException -= LegacyToastOnUnhandledException;
            if(_lastToast == null || _lastToast.IsDisposed)
                return;
            _lastToast.Closing -= LastToastOnClosing;
            _lastToast.Dispose();
        }

        #endregion

        #region Events

        private void LastToastOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            LegacyToastDispose();
        }

        private void LegacyToastOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LegacyToastHandleUnhandledException(e.ExceptionObject as Exception);
        }

        private void LegacyToastOnGuiUnhandedException(object sender, ThreadExceptionEventArgs e)
        {
            LegacyToastHandleUnhandledException(e.Exception);
        }

        private void LegacyToastHandleUnhandledException(Exception e)
        {
            if(e == null)
                return;
            
            _logger.Write(e, "UI: Unable to show legacy toast notification", LogLevel.Error);
        }

        private void EnableTtsOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            _logger.Write($"UI: [TTS] Desired Value: {_ttsCheckBox.Checked}", LogLevel.Debug);
            TtsNotification(_localizationRepository.GetText("ui-tts-notification-test-message"), _localizationRepository.GetText("ui-tts-notification-test-title"));
        }

        private void TtsNotification(string message, string title = "ui-dutyfound")
        {
            if(!_ttsCheckBox.Checked)
                return;

            var dutyFound = _localizationRepository.GetText(title);
            _synth.Speak(dutyFound);
            _synth.Speak(message);
        }

        private void Network_onReceiveEvent(int pid, EventType eventType, int[] args)
        {
            var server = _networks[pid].Process.MainModule.FileName.Contains("KOREA") ? "KOREA" : "GLOBAL";
            var text = pid + "|" + server + "|" + eventType + "|";
            var pos = 0;

            switch(eventType)
            {
                case EventType.MATCH_ALERT:
                    text += _dataRepository.GetRoulette(args[0]).Name + "|";
                    pos++;
                    text += _dataRepository.GetInstance(args[1]).Name + "|";
                    pos++;
                    break;
            }

            for(var i = pos; i < args.Length; i++)
                text += args[i] + "|";

            SendToAct(text);
            PostToToastWindowsNotificationIfNeeded(eventType, args);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if(!_isPluginEnabled)
                return;

            UpdateProcesses();
        }

        private void ClearLogsButton_Click(object sender, EventArgs e)
        {
            _richTextBox1.Clear();
        }

        private void ActMainFormOnShown(object sender, EventArgs e)
        {
            _mainFormIsLoaded = true;
            OnInit();
        }

        private void LanguageComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        #endregion

        #region Settings

        private void LoadSettings()
        {
            _logger.Write("Settings Loading...", LogLevel.Debug);
            // All the settings to deserialize
            _xmlSettingsSerializer.AddControlSetting(_disableToasts.Name, _disableToasts);
            _xmlSettingsSerializer.AddControlSetting(_languageValue.Name, _languageValue);
            _xmlSettingsSerializer.AddControlSetting(_ttsCheckBox.Name, _ttsCheckBox);
            _xmlSettingsSerializer.AddControlSetting(_persistToasts.Name, _persistToasts);
            _xmlSettingsSerializer.AddControlSetting(_enableTestEnvironment.Name, _enableTestEnvironment);
            _xmlSettingsSerializer.AddControlSetting(_enableLegacyToast.Name, _enableLegacyToast);

            if(File.Exists(_settingsFile))
                using(var fileStream = new FileStream(_settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using(var xmlTextReader = new XmlTextReader(fileStream))
                {
                    try
                    {
                        while(xmlTextReader.Read())
                        {
                            if(xmlTextReader.NodeType != XmlNodeType.Element)
                                continue;

                            if(xmlTextReader.LocalName == "SettingsSerializer")
                                _xmlSettingsSerializer.ImportFromXml(xmlTextReader);
                        }
                    }
                    catch(Exception)
                    {
                        _labelStatus.Text = "Error loading settings";
                    }

                    xmlTextReader.Close();
                }

            foreach(var language in _languageComboBox.Items.OfType<Language>())
            {
                if(language.Name.Equals(_languageValue.Text))
                {
                    _languageComboBox.SelectedItem = language;
                }
            }

            _logger.Write($"Language: {_languageValue.Text}", LogLevel.Debug);
            _logger.Write($"Disable Toasts: {_disableToasts.Checked}", LogLevel.Debug);
            _logger.Write($"Make Toasts Persistent: {_persistToasts.Checked}", LogLevel.Debug);
            _logger.Write($"Enable Legacy Toasts: {_enableLegacyToast.Checked}", LogLevel.Debug);
            _logger.Write($"Enable Text To Speech: {_ttsCheckBox.Checked}", LogLevel.Debug);
            _logger.Write($"Enable Test Environment: {_enableTestEnvironment.Checked}", LogLevel.Debug);
            _logger.Write("Settings Loaded!", LogLevel.Debug);
        }

        private void SaveSettings()
        {
            try
            {
                _logger.Write("Saving Settings...", LogLevel.Debug);
                using(var fileStream = new FileStream(_settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using(var xmlTextWriter = new XmlTextWriter(fileStream, Encoding.UTF8) { Formatting = Formatting.Indented, Indentation = 1, IndentChar = '\t' })
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
            catch(Exception ex)
            {
                _logger.Write(ex, "Error saving settings", LogLevel.Error);
            }
        }

        #endregion
    }
}