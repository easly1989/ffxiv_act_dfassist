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
using Windows.UI.Notifications;
using Advanced_Combat_Tracker;
using DFAssist.DataModel;
using DFAssist.Shell;
using Microsoft.Win32;
using Timer = System.Windows.Forms.Timer;

namespace DFAssist
{
    public class MainControl : UserControl, IActPluginV1
    {
        private const string AppId = "Advanced Combat Tracker";

        private readonly ConcurrentDictionary<int, ProcessNet> _networks;
        private readonly string _settingsFile;
        private readonly SpeechSynthesizer _synth;

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
            Localization.Initialize(_selectedLanguage.Code);
            Data.Initialize(_selectedLanguage.Code);
        }

        #endregion

        #region WinForm Required

        public MainControl()
        {
            InitializeComponent();

            _synth = new SpeechSynthesizer();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            _settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config", "DFAssist.config.xml");
            _networks = new ConcurrentDictionary<int, ProcessNet>();

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
            Logger.Debug($"[DisableToasts] Desired Value: {_disableToasts.Checked}!");
            _enableLegacyToast.Enabled = !_disableToasts.Checked;
            _persistToasts.Enabled = _enableLegacyToast.Enabled && !_enableLegacyToast.Checked;
        }

        private void EnableLegacyToastsOnCheckedChanged(object sender, EventArgs e)
        {
            Logger.Debug($"[LegacyToasts] Desired Value: {_enableLegacyToast.Checked}!");
            _persistToasts.Enabled = !_enableLegacyToast.Checked;
            ToastWindowNotification(Localization.GetText("ui-toast-notification-test-title"), Localization.GetText("ui-toast-notification-test-message"));
        }

        private void PersistToastsOnCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                Logger.Debug($"[PersistentToasts] Desired Value: {_persistToasts.Checked}!");

                var keyName = $@"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings\{AppId}";
                using(var key = Registry.CurrentUser.OpenSubKey(keyName, true))
                {
                    if(_persistToasts.Checked)
                    {
                        if(key == null)
                        {
                            Logger.Debug("[PersistentToasts] Key not found in the registry, Adding a new one!");
                            Registry.SetValue($@"HKEY_CURRENT_USER\{keyName}", "ShowInActionCenter", 1, RegistryValueKind.DWord);
                        }
                        else
                        {
                            Logger.Debug("[PersistentToasts] Key found in the registry, setting value to 1!");
                            key.SetValue("ShowInActionCenter", 1, RegistryValueKind.DWord);
                        }
                    }
                    else
                    {
                        if(key == null)
                        {
                            Logger.Debug("[PersistentToasts] Key not found in the registry, nothing to do!");
                            return;
                        }

                        Logger.Debug("[PersistentToasts] Key found in the registry, Removing value!");
                        key.DeleteValue("ShowInActionCenter");
                    }

                    MessageBox.Show(Localization.GetText("ui-persistent-toast-warning-message"), Localization.GetText("ui-persistent-toast-warning-title"), MessageBoxButtons.OK);
                }
            }
            catch(Exception ex)
            {
                Logger.Exception(ex, "Unable to remove/add the registry key to make Toasts persistent!");
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

                var result = MessageBox.Show(Localization.GetText("ui-update-available-message"),
                    Localization.GetText("ui-update-available-title"), MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
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
            ActGlobals.oFormActMain.Shown -= ActMainFormOnShown;

            var pluginData = ActGlobals.oFormActMain.PluginGetSelfData(this);
            var enviroment = Path.GetDirectoryName(pluginData.pluginFile.ToString());
            AssemblyResolver.Initialize(enviroment);

            Logger.SetTextBox(_richTextBox1);
            Logger.Debug("----------------------------------------------------------------");
            Logger.Debug("Plugin Init");
            Logger.Debug($"Plugin Version: {Assembly.GetExecutingAssembly().GetName().Version}");

            var defaultLanguage = new Language { Name = "English", Code = "en-us" };
            LoadData(defaultLanguage);

            // The shortcut must be created to work with windows 8/10 Toasts
            Logger.Debug(ShortCutCreator.TryCreateShortcut(AppId, AppId)
                ? "Shortcut for ACT found"
                : "Unable to Create the Shorctut for ACT");

            _isPluginEnabled = true;

            Logger.Debug("Plugin Enabled");

            _languageComboBox.DataSource = new[]
            {
                defaultLanguage,
                new Language {Name = "한국어", Code = "ko-kr"},
                new Language {Name = "日本語", Code = "ja-jp"},
                new Language {Name = "Français", Code = "fr-fr"}
            };
            _languageComboBox.DisplayMember = "Name";
            _languageComboBox.ValueMember = "Code";
            _languageComboBox.SelectedValueChanged += LanguageComboBox_SelectedValueChanged;

            UpdateTranslations();

            _labelStatus.Text = "Starting...";

            _labelStatus.Text = Localization.GetText("l-plugin-started");
            _labelTab.Text = Localization.GetText("app-name");

            Logger.Debug("Plugin Started!");

            _labelTab.Controls.Add(this);
            _xmlSettingsSerializer = new SettingsSerializer(this);

            LoadSettings();

            UpdateProcesses();

            if(_timer == null)
            {
                _timer = new Timer { Interval = 30000 };
                _timer.Tick += Timer_Tick;
            }

            _timer.Enabled = true;

            // shows a test toast
            ToastWindowNotification(Localization.GetText("ui-toast-notification-test-title"), Localization.GetText("ui-toast-notification-test-message"));

            _pluginInitializing = false;

            ActGlobals.oFormActMain.UpdateCheckClicked += FormActMain_UpdateCheckClicked;
            if(ActGlobals.oFormActMain.GetAutomaticUpdatesAllowed())
                new Thread(FormActMain_UpdateCheckClicked).Start();

            Logger.Debug("----------------------------------------------------------------");
        }

        public void DeInitPlugin()
        {
            _isPluginEnabled = false;

            SaveSettings();

            _labelTab = null;

            if(_labelStatus != null)
            {
                _labelStatus.Text = Localization.GetText("l-plugin-stopped");
                _labelStatus = null;
            }

            foreach(var entry in _networks)
                entry.Value.Network.StopCapture();

            _timer.Enabled = false;

            Logger.SetTextBox(null);
        }

        #endregion

        #region Getters

        private static string GetInstanceName(int code)
        {
            return Data.GetInstance(code).Name;
        }

        private static string GetRouletteName(int code)
        {
            return Data.GetRoulette(code).Name;
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
                    var pn = new ProcessNet(process, new Network());
                    FFXIVPacketHandler.OnEventReceived += Network_onReceiveEvent;
                    _networks.TryAdd(process.Id, pn);
                    Logger.Success("l-process-set-success", process.Id);
                }
            }
            catch(Exception e)
            {
                Logger.Exception(e, "l-process-set-failed");
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
                    FFXIVPacketHandler.OnEventReceived -= Network_onReceiveEvent;
                }
                catch(Exception e)
                {
                    Logger.Exception(e, "l-process-remove-failed");
                }
            }
        }

        private void UpdateTranslations()
        {
            Logger.Debug("Updating Localization for UI...");
            SuspendLayout();
            _label1.Text = Localization.GetText("ui-language-display-text");
            _button1.Text = Localization.GetText("ui-log-clear-display-text");
            _enableTestEnvironment.Text = Localization.GetText("ui-enable-test-environment");
            _ttsCheckBox.Text = Localization.GetText("ui-enable-tts");
            _persistToasts.Text = Localization.GetText("ui-persist-toasts");
            _enableLegacyToast.Text = Localization.GetText("ui-enable-legacy-toasts");
            _disableToasts.Text = Localization.GetText("ui-disable-toasts");

            _appTitle.Text = $"{Localization.GetText("app-name")} v{Assembly.GetExecutingAssembly().GetName().Version} | ";
            _generalSettings.Text = Localization.GetText("ui-general-settings-group");
            _toastSettings.Text = Localization.GetText("ui-toast-settings-group");
            _ttsSettings.Text = Localization.GetText("ui-tts-settings-group");
            _testSettings.Text = Localization.GetText("ui-test-settings-group");

            ResumeLayout(false);
            PerformLayout();
            Logger.Debug("Localization for UI Updated!");
        }

        #endregion

        #region Post Method

        private static void SendToAct(string text)
        {
            ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "00|" + DateTime.Now.ToString("O") + "|0048|F|" + text);
        }

        private void PostToToastWindowsNotificationIfNeeded(string server, EventType eventType, int[] args)
        {
            if(eventType != EventType.MATCH_ALERT)
                return;

            var head = _networks.Count <= 1 ? "" : "[" + server + "] ";
            switch(eventType)
            {
                case EventType.MATCH_ALERT:
                    var title = head + (args[0] != 0 ? GetRouletteName(args[0]) : Localization.GetText("app-name"));
                    var testing = _enableTestEnvironment.Checked ? "[Code: " + args[1] + "] " : string.Empty;
                    ToastWindowNotification(title, ">> " + testing + GetInstanceName(args[1]));
                    TtsNotification(GetInstanceName(args[1]));
                    break;
            }
        }

        private void ToastWindowNotification(string title, string message)
        {
            Logger.Debug("Request Showing Taost received...");
            if(_disableToasts.Checked)
            {
                Logger.Debug("... Toasts are disabled!");
                return;
            }

            if(_enableLegacyToast.Checked)
            {
                Logger.Debug("... Legacy Toasts Enabled...");
                try
                {
                    Logger.Debug("... Closing any open Legacy Toast...");
                    _lastToast?.Close();
                    LegacyToastDispose();
                    Application.ThreadException += LegacyToastOnGuiUnhandedException;
                    AppDomain.CurrentDomain.UnhandledException += LegacyToastOnUnhandledException;
                    var toast = new LegacyToast(title, message, _networks) { Text = title };
                    Logger.Debug("... Creating new Legacy Toast...");
                    _lastToast = toast;
                    _lastToast.Closing += LastToastOnClosing;
                    _lastToast.Show();
                    Logger.Debug("... Legacy Toast Showing...");
                    NativeMethods.ShowWindow(_lastToast.Handle, 9);
                    NativeMethods.SetForegroundWindow(_lastToast.Handle);
                    _lastToast.Activate();
                }
                catch(Exception ex)
                {
                    Logger.Debug("Error handling/creating Legacy Toast!");
                    LegacyToastHandleUnhandledException(ex);
                    _lastToast?.Close();
                    LegacyToastDispose();
                }
            }
            else
            {
                Logger.Debug("... Legacy Toasts Disabled...");
                try
                {
                    var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText03);

                    var stringElements = toastXml.GetElementsByTagName("text");
                    if(stringElements.Length < 2)
                    {
                        Logger.Error("l-toast-notification-error");
                        return;
                    }

                    stringElements[0].AppendChild(toastXml.CreateTextNode(title));
                    stringElements[1].AppendChild(toastXml.CreateTextNode(message));

                    var toast = new ToastNotification(toastXml);
                    Logger.Debug("... Creating new Toast...");
                    ToastNotificationManager.CreateToastNotifier(AppId).Show(toast);
                    Logger.Debug("... Toast Showing...");
                }
                catch(Exception e)
                {
                    Logger.Exception(e, "l-toast-notification-error");
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

        private static void LegacyToastOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LegacyToastHandleUnhandledException(e.ExceptionObject as Exception);
        }

        private static void LegacyToastOnGuiUnhandedException(object sender, ThreadExceptionEventArgs e)
        {
            LegacyToastHandleUnhandledException(e.Exception);
        }

        private static void LegacyToastHandleUnhandledException(Exception e)
        {
            if(e == null)
                return;
            Logger.Exception(e, "l-toast-notification-error");
        }

        private void EnableTtsOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            Logger.Debug($"[TTS] Desired Value: {_ttsCheckBox.Checked}!");
            TtsNotification(Localization.GetText("ui-tts-notification-test-message"), Localization.GetText("ui-tts-notification-test-title"));
        }

        private void TtsNotification(string message, string title = "ui-tts-dutyfound")
        {
            if(!_ttsCheckBox.Checked)
                return;

            var dutyFound = Localization.GetText(title);
            _synth.Speak(dutyFound); // duty found
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
                    text += GetRouletteName(args[0]) + "|";
                    pos++;
                    text += GetInstanceName(args[1]) + "|";
                    pos++;
                    break;
            }

            for(var i = pos; i < args.Length; i++)
                text += args[i] + "|";

            SendToAct(text);

            PostToToastWindowsNotificationIfNeeded(server, eventType, args);
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
            UpdateTranslations();
        }

        #endregion

        #region Settings

        private void LoadSettings()
        {
            Logger.Debug("Settings Loading...");
            // All the settings to deserialize
            _xmlSettingsSerializer.AddControlSetting(_disableToasts.Name, _disableToasts);
            _xmlSettingsSerializer.AddControlSetting(_languageComboBox.Name, _languageComboBox);
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
                    catch(Exception ex)
                    {
                        _labelStatus.Text = Localization.GetText("l-settings-load-error", ex.Message);
                    }

                    xmlTextReader.Close();
                }

            _selectedLanguage = (Language)_languageComboBox.SelectedItem;
            Logger.Debug($"Language: {_selectedLanguage.Name}");
            Logger.Debug($"Disable Toasts: {_disableToasts.Checked}");
            Logger.Debug($"Make Toasts Persistent: {_persistToasts.Checked}");
            Logger.Debug($"Enable Legacy Toasts: {_enableLegacyToast.Checked}");
            Logger.Debug($"Enable Text To Speech: {_ttsCheckBox.Checked}");
            Logger.Debug($"Enable Test Environment: {_enableTestEnvironment.Checked}");
            Logger.Debug("Settings Loaded!");
        }

        private void SaveSettings()
        {
            try
            {
                Logger.Debug("Saving Settings...");
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

                    Logger.Debug("Settings Saved!");
                }
            }
            catch(Exception ex)
            {
                Logger.Exception(ex, "l-settings-save-error");
            }
        }

        #endregion
    }
}