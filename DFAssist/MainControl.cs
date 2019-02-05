// reference:System.dll
// reference:System.Core.dll
// reference:System.Web.Extensions.dll
// reference:Newtonsoft.Json.dll
// reference:Overlay.NET.dll
// reference:Process.NET.dll
// reference:Microsoft.WindowsAPICodePack.dll
// reference:Microsoft.WindowsAPICodePack.Shell.dll
// reference:Microsoft.WindowsAPICodePack.ShellExtensions.dll
// reference:SharpDX.Direct2D1.dll
// reference:SharpDX.dll
// reference:SharpDX.DXGI.dll
// reference:SharpDX.Mathematics.dll

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Windows.UI.Notifications;
using Advanced_Combat_Tracker;
using DFAssist.DataModel;
using DFAssist.DirectX;
using DFAssist.Shell;

namespace DFAssist
{
    public class MainControl : UserControl, IActPluginV1
    {
        private const string AppId = "Advanced Combat Tracker";

        private readonly string _settingsFile;
        private readonly ConcurrentDictionary<int, ProcessNet> _networks;

        private bool _isPluginEnabled;
        private bool _pluginInitializing;

        private bool _mainFormIsLoaded;
        private bool _isTestEnvironmentEnabled;
        private Timer _timer;
        private Label _label1;
        private Label _labelStatus;
        private TabPage _labelTab;
        private Language _selectedLanguage;
        private CheckBox _enableTestEnvironment;
        private ComboBox _languageComboBox;
        private SettingsSerializer _xmlSettingsSerializer;
        private GroupBox _groupBox3;
        private RichTextBox _richTextBox1;
        private CheckBox _enableLoggingCheckBox;
        private Button _button1;

        #region WinForm Required
        public MainControl()
        {
            InitializeComponent();

            _settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config", "DFAssist.config.xml");
            _networks = new ConcurrentDictionary<int, ProcessNet>();

            foreach (Form formLoaded in Application.OpenForms)
            {
                if (formLoaded != ActGlobals.oFormActMain)
                    continue;

                _mainFormIsLoaded = true;
                break;
            }
        }

        /// <summary>
        /// This is autmatically generated from the designer
        /// </summary>
        private void InitializeComponent()
        {
            this._label1 = new System.Windows.Forms.Label();
            this._languageComboBox = new System.Windows.Forms.ComboBox();
            this._groupBox3 = new System.Windows.Forms.GroupBox();
            this._enableLoggingCheckBox = new System.Windows.Forms.CheckBox();
            this._button1 = new System.Windows.Forms.Button();
            this._richTextBox1 = new System.Windows.Forms.RichTextBox();
            this._enableTestEnvironment = new System.Windows.Forms.CheckBox();
            this._groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // _label1
            // 
            this._label1.AutoSize = true;
            this._label1.Location = new System.Drawing.Point(21, 17);
            this._label1.Name = "_label1";
            this._label1.Size = new System.Drawing.Size(55, 13);
            this._label1.TabIndex = 7;
            this._label1.Text = "Language";
            // 
            // _languageComboBox
            // 
            this._languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._languageComboBox.FormattingEnabled = true;
            this._languageComboBox.Location = new System.Drawing.Point(88, 14);
            this._languageComboBox.Name = "_languageComboBox";
            this._languageComboBox.Size = new System.Drawing.Size(121, 21);
            this._languageComboBox.TabIndex = 6;
            this._languageComboBox.SelectedValueChanged += new System.EventHandler(this.LanguageComboBox_SelectedValueChanged);
            // 
            // _groupBox3
            // 
            this._groupBox3.Controls.Add(this._enableLoggingCheckBox);
            this._groupBox3.Controls.Add(this._button1);
            this._groupBox3.Controls.Add(this._richTextBox1);
            this._groupBox3.Location = new System.Drawing.Point(24, 52);
            this._groupBox3.Name = "_groupBox3";
            this._groupBox3.Size = new System.Drawing.Size(710, 523);
            this._groupBox3.TabIndex = 12;
            this._groupBox3.TabStop = false;
            this._groupBox3.Text = "Logs";
            // 
            // _enableLoggingCheckBox
            // 
            this._enableLoggingCheckBox.AutoSize = true;
            this._enableLoggingCheckBox.Checked = true;
            this._enableLoggingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._enableLoggingCheckBox.Location = new System.Drawing.Point(7, 22);
            this._enableLoggingCheckBox.Name = "_enableLoggingCheckBox";
            this._enableLoggingCheckBox.Size = new System.Drawing.Size(100, 17);
            this._enableLoggingCheckBox.TabIndex = 2;
            this._enableLoggingCheckBox.Text = "Enable Logging";
            this._enableLoggingCheckBox.UseVisualStyleBackColor = true;
            this._enableLoggingCheckBox.CheckStateChanged += new System.EventHandler(this.EnableLoggingCheckBox_CheckedChanged);
            // 
            // _button1
            // 
            this._button1.AutoSize = true;
            this._button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._button1.Location = new System.Drawing.Point(201, 16);
            this._button1.Name = "_button1";
            this._button1.Size = new System.Drawing.Size(67, 23);
            this._button1.TabIndex = 1;
            this._button1.Text = "Clear Logs";
            this._button1.UseVisualStyleBackColor = true;
            this._button1.Click += new System.EventHandler(this.ClearLogsButton_Click);
            // 
            // _richTextBox1
            // 
            this._richTextBox1.Location = new System.Drawing.Point(6, 52);
            this._richTextBox1.Name = "_richTextBox1";
            this._richTextBox1.ReadOnly = true;
            this._richTextBox1.Size = new System.Drawing.Size(698, 465);
            this._richTextBox1.TabIndex = 0;
            this._richTextBox1.Text = "";
            // 
            // _enableTestEnvironment
            // 
            this._enableTestEnvironment.AutoSize = true;
            this._enableTestEnvironment.Location = new System.Drawing.Point(225, 16);
            this._enableTestEnvironment.Name = "_enableTestEnvironment";
            this._enableTestEnvironment.Size = new System.Drawing.Size(145, 17);
            this._enableTestEnvironment.TabIndex = 13;
            this._enableTestEnvironment.Text = "Enable Test Environment";
            this._enableTestEnvironment.UseVisualStyleBackColor = true;
            this._enableTestEnvironment.CheckStateChanged += new System.EventHandler(this.EnableTestEnvironmentOnCheckedChanged);
            // 
            // MainControl
            // 
            this.Controls.Add(this._enableTestEnvironment);
            this.Controls.Add(this._groupBox3);
            this.Controls.Add(this._label1);
            this.Controls.Add(this._languageComboBox);
            this.Name = "MainControl";
            this.Size = new System.Drawing.Size(1744, 592);
            this._groupBox3.ResumeLayout(false);
            this._groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        #region IActPluginV1 Implementations
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            _labelStatus = pluginStatusText;
            _labelTab = pluginScreenSpace;

            var pluginData = ActGlobals.oFormActMain.PluginGetSelfData(this);
            var enviroment = Path.GetDirectoryName(pluginData.pluginFile.ToString());

            // if any of the assembly cannot be loaded, then the plugin cannot be started
            if (!AssemblyResolver.LoadAssemblies(enviroment, _labelStatus))
                return;

            if (_mainFormIsLoaded)
                OnInit();
            else
                ActGlobals.oFormActMain.Shown += ActMainFormOnShown;
        }

        private void OnInit()
        {
            if (_pluginInitializing)
                return;

            _pluginInitializing = true;

            Logger.SetTextBox(_richTextBox1);
            ActGlobals.oFormActMain.Shown -= ActMainFormOnShown;

            var defaultLanguage = new Language { Name = "English", Code = "en-us" };
            LoadData(defaultLanguage);

            // The shortcut must be created to work with windows 8/10 Toasts
            ShortCutCreator.TryCreateShortcut(AppId, AppId);

            _isPluginEnabled = true;

            _languageComboBox.DataSource = new[]
            {
                defaultLanguage,
                new Language {Name = "한국어", Code = "ko-kr"},
                new Language {Name = "日本語", Code = "ja-jp"},
                new Language {Name = "Français", Code = "fr-fr"}
            };
            _languageComboBox.DisplayMember = "Name";
            _languageComboBox.ValueMember = "Code";

            _labelStatus.Text = "Starting...";

            UpdateTranslations();

            _labelStatus.Text = Localization.GetText("l-plugin-started");
            _labelTab.Text = Localization.GetText("app-name");

            _labelTab.Controls.Add(this);
            _xmlSettingsSerializer = new SettingsSerializer(this);

            LoadSettings();

            UpdateProcesses();

            if (_timer == null)
            {
                _timer = new Timer { Interval = 30000 };
                _timer.Tick += Timer_Tick;
            }

            _timer.Enabled = true;

            // show a test toast
            ToastWindowNotification(Localization.GetText("ui-toast-notification-test-title"), Localization.GetText("ui-toast-notification-test-message"));
            new DirectXToastManager().Show("title", "message", _networks.Keys.FirstOrDefault());

            _pluginInitializing = false;
        }

        public void DeInitPlugin()
        {
            _isPluginEnabled = false;

            SaveSettings();

            _labelTab = null;

            if (_labelStatus != null)
            {
                _labelStatus.Text = Localization.GetText("l-plugin-stopped");
                _labelStatus = null;
            }

            foreach (var entry in _networks)
            {
                entry.Value.Network.StopCapture();
            }

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

        #region Load Methods
        private void LoadData(Language defaultLanguage = null)
        {
            var newLanguage = defaultLanguage ?? (Language)_languageComboBox.SelectedItem;
            if (_selectedLanguage != null && newLanguage.Code.Equals(_selectedLanguage.Code))
                return;

            _selectedLanguage = newLanguage;
            Localization.Initialize(_selectedLanguage.Code);
            Data.Initialize(_selectedLanguage.Code);
        }
        #endregion

        #region Update Methods
        private void UpdateProcesses()
        {
            var process = System.Diagnostics.Process.GetProcessesByName("notepad++").FirstOrDefault();
            if(process == null)
                return;
            try
            {
                if (_networks.ContainsKey(process.Id))
                    return;

                var pn = new ProcessNet(process, new Network());
                FFXIVPacketHandler.OnEventReceived += Network_onReceiveEvent;
                _networks.TryAdd(process.Id, pn);
                Logger.Success("l-process-set-success", process.Id);
            }
            catch (Exception e)
            {
                Logger.Exception(e, "l-process-set-failed");
            }


            var toDelete = new List<int>();
            foreach (var entry in _networks)
            {
                if (entry.Value.Process.HasExited)
                {
                    entry.Value.Network.StopCapture();
                    toDelete.Add(entry.Key);
                }
                else
                {
                    if (entry.Value.Network.IsRunning)
                        entry.Value.Network.UpdateGameConnections(entry.Value.Process);
                    else
                        entry.Value.Network.StartCapture(entry.Value.Process);
                }
            }

            foreach (var t in toDelete)
            {
                try
                {
                    _networks.TryRemove(t, out _);
                    FFXIVPacketHandler.OnEventReceived -= Network_onReceiveEvent;
                }
                catch (Exception e)
                {
                    Logger.Exception(e, "l-process-remove-failed");
                }
            }
        }

        private void UpdateTranslations()
        {
            _label1.Text = Localization.GetText("ui-language-display-text");
            _groupBox3.Text = Localization.GetText("ui-log-display-text");
            _enableLoggingCheckBox.Text = Localization.GetText("ui-log-enable-display-text");
            _button1.Text = Localization.GetText("ui-log-clear-display-text");
            _enableTestEnvironment.Text = Localization.GetText("ui-enable-test-environment");
        }
        #endregion

        #region Post Method
        private static void SendToAct(string text)
        {
            ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "00|" + DateTime.Now.ToString("O") + "|0048|F|" + text);
        }

        private void PostToToastWindowsNotificationIfNeeded(string server, EventType eventType, int[] args)
        {
            if (eventType != EventType.MATCH_ALERT) return;

            var head = _networks.Count <= 1 ? "" : "[" + server + "] ";
            switch (eventType)
            {
                case EventType.MATCH_ALERT:
                    var title = head + (args[0] != 0 ? GetRouletteName(args[0]) : Localization.GetText("app-name"));
                    var testing = _isTestEnvironmentEnabled ? "[Code: " + args[1] + "] " : string.Empty;
                    ToastWindowNotification(title, ">> " + testing + GetInstanceName(args[1]));

                    break;
            }
        }

        private static void ToastWindowNotification(string title, string message)
        {
            try
            {
                // Get a toast XML template
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText03);


                var stringElements = toastXml.GetElementsByTagName("text");
                if (stringElements.Length < 2)
                {
                    Logger.Error("l-toast-notification-error");
                    return;
                }

                stringElements[0].AppendChild(toastXml.CreateTextNode(title));
                stringElements[1].AppendChild(toastXml.CreateTextNode(message));

                var toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier(AppId).Show(toast);
            }
            catch (Exception e)
            {
                Logger.Exception(e, "l-toast-notification-error");
            }
        }
        #endregion

        #region Events
        private void EnableTestEnvironmentOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            _isTestEnvironmentEnabled = _enableTestEnvironment.Checked;
        }

        private void Network_onReceiveEvent(int pid, EventType eventType, int[] args)
        {
            var server = _networks[pid].Process.MainModule.FileName.Contains("KOREA") ? "KOREA" : "GLOBAL";
            var text = pid + "|" + server + "|" + eventType + "|";
            var pos = 0;

            switch (eventType)
            {
                case EventType.INSTANCE_ENTER:
                case EventType.INSTANCE_EXIT:
                    if (args.Length > 0)
                    {
                        text += GetInstanceName(args[0]) + "|";
                        pos++;
                    }

                    break;
                case EventType.MATCH_BEGIN:
                    text += (MatchType)args[0] + "|";
                    pos++;
                    switch ((MatchType)args[0])
                    {
                        case MatchType.ROULETTE:
                            text += GetRouletteName(args[1]) + "|";
                            pos++;
                            break;
                        case MatchType.SELECTIVE:
                            text += args[1] + "|";
                            pos++;
                            var p = pos;
                            for (var i = p; i < args.Length; i++)
                            {
                                text += GetInstanceName(args[i]) + "|";
                                pos++;
                            }

                            break;
                    }

                    break;
                case EventType.MATCH_END:
                    text += (MatchEndType)args[0] + "|";
                    pos++;
                    break;
                case EventType.MATCH_PROGRESS:
                    text += GetInstanceName(args[0]) + "|";
                    pos++;
                    break;
                case EventType.MATCH_ALERT:
                    text += GetRouletteName(args[0]) + "|";
                    pos++;
                    text += GetInstanceName(args[1]) + "|";
                    pos++;
                    break;
            }

            for (var i = pos; i < args.Length; i++) text += args[i] + "|";

            SendToAct(text);

            PostToToastWindowsNotificationIfNeeded(server, eventType, args);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_isPluginEnabled == false)
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

        private void EnableLoggingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!_enableLoggingCheckBox.Checked)
            {
                Logger.SetTextBox(null);
                _button1.Enabled = false;
            }
            else
            {
                Logger.SetTextBox(_richTextBox1);
                _button1.Enabled = true;
            }
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
            // All the settings to deserialize
            _xmlSettingsSerializer.AddControlSetting(_languageComboBox.Name, _languageComboBox);
            _xmlSettingsSerializer.AddControlSetting(_enableLoggingCheckBox.Name, _enableLoggingCheckBox);

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

                            if (xmlTextReader.LocalName == "SettingsSerializer")
                                _xmlSettingsSerializer.ImportFromXml(xmlTextReader);
                        }

                    }
                    catch (Exception ex)
                    {
                        _labelStatus.Text = Localization.GetText("l-settings-load-error", ex.Message);
                    }

                    xmlTextReader.Close();
                }
            }

            _selectedLanguage = (Language)_languageComboBox.SelectedItem;
        }

        private void SaveSettings()
        {
            try
            {
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
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "l-settings-save-error");
            }

        }
        #endregion
    }
}