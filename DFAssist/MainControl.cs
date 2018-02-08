// reference:System.dll
// reference:System.Core.dll
// reference:System.Web.Extensions.dll

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Windows.UI.Notifications;
using Advanced_Combat_Tracker;
using DFAssist.DataModel;
using DFAssist.Shell;
using Newtonsoft.Json.Linq;

namespace DFAssist
{
    public class MainControl : UserControl, IActPluginV1
    {
        private const string AppId = "Advanced Combat Tracker";

        private readonly string _settingsFile;
        private readonly ConcurrentStack<string> _telegramSelectedFates;
        private readonly ConcurrentDictionary<int, ProcessNet> _networks;

        private bool _isPluginEnabled;
        private bool _lockTreeEvent;
        private bool _isDutyAlertEnabled;
        private bool _isTelegramEnabled;
        private bool _isToastNotificationEnabled;
        private string _selectedLanguage;
        private string _checkedFates;
        private IDisposable _initDisposable;
        private Timer _timer;
        private Label _label1;
        private Label _label2;
        private Label _label3;
        private Label _label4;
        private Label _labelStatus;
        private TextBox _telegramChatIdTextBox;
        private TextBox _telegramTokenTextBox;
        private CheckBox _telegramCheckBox;
        private CheckBox _dutyFinderAlertCheckBox;
        private CheckBox _toadNotificationCheckBox;
        private ComboBox _languageComboBox;
        private GroupBox _groupBox1;
        private GroupBox _groupBox2;
        private SettingsSerializer _xmlSettingsSerializer;
        private GroupBox _groupBox3;
        private RichTextBox _richTextBox1;
        private CheckBox _enableLoggingCheckBox;
        private Button _button1;

        public TreeView TelegramFateTreeView;

        #region WinForm Required
        public MainControl()
        {
            InitializeComponent();
            Logger.SetLoggerTextBox(_richTextBox1);

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var name = new AssemblyName(Assembly.GetExecutingAssembly().FullName).Name + "." + new AssemblyName(args.Name).Name + ".dll";
                using (var assemblyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
                {
                    if (assemblyStream != null)
                    {
                        var assemblyBuffer = new byte[assemblyStream.Length];
                        assemblyStream.Read(assemblyBuffer, 0, assemblyBuffer.Length);
                        return Assembly.Load(assemblyBuffer);
                    }
                }

                Logger.LogError($"Unable to load {args.Name} assembly.");
                return null;
            };

            _settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config", "DFAssist.config.xml");

            _networks = new ConcurrentDictionary<int, ProcessNet>();
            _telegramSelectedFates = new ConcurrentStack<string>();
        }

        /// <summary>
        /// This is autmatically generated from the designer
        /// </summary>
        private void InitializeComponent()
        {
            this._label1 = new System.Windows.Forms.Label();
            this._languageComboBox = new System.Windows.Forms.ComboBox();
            this._groupBox1 = new System.Windows.Forms.GroupBox();
            this._telegramTokenTextBox = new System.Windows.Forms.TextBox();
            this._label3 = new System.Windows.Forms.Label();
            this._telegramChatIdTextBox = new System.Windows.Forms.TextBox();
            this._label2 = new System.Windows.Forms.Label();
            this._telegramCheckBox = new System.Windows.Forms.CheckBox();
            this._groupBox2 = new System.Windows.Forms.GroupBox();
            this._label4 = new System.Windows.Forms.Label();
            this.TelegramFateTreeView = new System.Windows.Forms.TreeView();
            this._dutyFinderAlertCheckBox = new System.Windows.Forms.CheckBox();
            this._toadNotificationCheckBox = new System.Windows.Forms.CheckBox();
            this._groupBox3 = new System.Windows.Forms.GroupBox();
            this._enableLoggingCheckBox = new System.Windows.Forms.CheckBox();
            this._button1 = new System.Windows.Forms.Button();
            this._richTextBox1 = new System.Windows.Forms.RichTextBox();
            this._groupBox1.SuspendLayout();
            this._groupBox2.SuspendLayout();
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
            this._languageComboBox.SelectedIndexChanged += new System.EventHandler(this._comboBoxLanguage_SelectedIndexChanged);
            // 
            // _groupBox1
            // 
            this._groupBox1.Controls.Add(this._telegramTokenTextBox);
            this._groupBox1.Controls.Add(this._label3);
            this._groupBox1.Controls.Add(this._telegramChatIdTextBox);
            this._groupBox1.Controls.Add(this._label2);
            this._groupBox1.Controls.Add(this._telegramCheckBox);
            this._groupBox1.Location = new System.Drawing.Point(23, 49);
            this._groupBox1.Name = "_groupBox1";
            this._groupBox1.Size = new System.Drawing.Size(533, 51);
            this._groupBox1.TabIndex = 9;
            this._groupBox1.TabStop = false;
            this._groupBox1.Text = "Telegram";
            // 
            // _telegramTokenTextBox
            // 
            this._telegramTokenTextBox.Location = new System.Drawing.Point(232, 20);
            this._telegramTokenTextBox.Name = "_telegramTokenTextBox";
            this._telegramTokenTextBox.Size = new System.Drawing.Size(291, 20);
            this._telegramTokenTextBox.TabIndex = 9;
            // 
            // _label3
            // 
            this._label3.AutoSize = true;
            this._label3.Location = new System.Drawing.Point(186, 23);
            this._label3.Name = "_label3";
            this._label3.Size = new System.Drawing.Size(38, 13);
            this._label3.TabIndex = 8;
            this._label3.Text = "Token";
            // 
            // _telegramChatIdTextBox
            // 
            this._telegramChatIdTextBox.Location = new System.Drawing.Point(67, 20);
            this._telegramChatIdTextBox.Name = "_telegramChatIdTextBox";
            this._telegramChatIdTextBox.Size = new System.Drawing.Size(100, 20);
            this._telegramChatIdTextBox.TabIndex = 7;
            // 
            // _label2
            // 
            this._label2.AutoSize = true;
            this._label2.Location = new System.Drawing.Point(15, 23);
            this._label2.Name = "_label2";
            this._label2.Size = new System.Drawing.Size(43, 13);
            this._label2.TabIndex = 6;
            this._label2.Text = "Chat ID";
            // 
            // _telegramCheckBox
            // 
            this._telegramCheckBox.AutoSize = true;
            this._telegramCheckBox.Location = new System.Drawing.Point(67, 0);
            this._telegramCheckBox.Name = "_telegramCheckBox";
            this._telegramCheckBox.Size = new System.Drawing.Size(56, 17);
            this._telegramCheckBox.TabIndex = 5;
            this._telegramCheckBox.Text = "Active";
            this._telegramCheckBox.UseVisualStyleBackColor = true;
            this._telegramCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxTelegram_CheckedChanged);
            // 
            // _groupBox2
            // 
            this._groupBox2.Controls.Add(this._label4);
            this._groupBox2.Controls.Add(this.TelegramFateTreeView);
            this._groupBox2.Controls.Add(this._dutyFinderAlertCheckBox);
            this._groupBox2.Location = new System.Drawing.Point(23, 115);
            this._groupBox2.Name = "_groupBox2";
            this._groupBox2.Size = new System.Drawing.Size(533, 457);
            this._groupBox2.TabIndex = 10;
            this._groupBox2.TabStop = false;
            this._groupBox2.Text = "Alert";
            // 
            // _label4
            // 
            this._label4.AutoSize = true;
            this._label4.Location = new System.Drawing.Point(15, 57);
            this._label4.Name = "_label4";
            this._label4.Size = new System.Drawing.Size(43, 13);
            this._label4.TabIndex = 10;
            this._label4.Text = "F.A.T.E";
            // 
            // TelegramFateTreeView
            // 
            this.TelegramFateTreeView.CheckBoxes = true;
            this.TelegramFateTreeView.Location = new System.Drawing.Point(15, 81);
            this.TelegramFateTreeView.Name = "TelegramFateTreeView";
            this.TelegramFateTreeView.Size = new System.Drawing.Size(508, 370);
            this.TelegramFateTreeView.TabIndex = 9;
            this.TelegramFateTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.FateTreeView_AfterCheck);
            // 
            // _dutyFinderAlertCheckBox
            // 
            this._dutyFinderAlertCheckBox.AutoSize = true;
            this._dutyFinderAlertCheckBox.Checked = true;
            this._dutyFinderAlertCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._dutyFinderAlertCheckBox.Location = new System.Drawing.Point(15, 22);
            this._dutyFinderAlertCheckBox.Name = "_dutyFinderAlertCheckBox";
            this._dutyFinderAlertCheckBox.Size = new System.Drawing.Size(80, 17);
            this._dutyFinderAlertCheckBox.TabIndex = 8;
            this._dutyFinderAlertCheckBox.Text = "Duty Finder";
            this._dutyFinderAlertCheckBox.UseVisualStyleBackColor = true;
            this._dutyFinderAlertCheckBox.CheckedChanged += new System.EventHandler(this.CheckBoxTelegramDutyFinder_CheckedChanged);
            // 
            // _toadNotificationCheckBox
            // 
            this._toadNotificationCheckBox.AutoSize = true;
            this._toadNotificationCheckBox.Location = new System.Drawing.Point(255, 18);
            this._toadNotificationCheckBox.Name = "_toadNotificationCheckBox";
            this._toadNotificationCheckBox.Size = new System.Drawing.Size(142, 17);
            this._toadNotificationCheckBox.TabIndex = 11;
            this._toadNotificationCheckBox.Text = "Active Toast Notification";
            this._toadNotificationCheckBox.UseVisualStyleBackColor = true;
            this._toadNotificationCheckBox.CheckedChanged += new System.EventHandler(this._checkBoxToastNotification_CheckedChanged);
            // 
            // _groupBox3
            // 
            this._groupBox3.Controls.Add(this._enableLoggingCheckBox);
            this._groupBox3.Controls.Add(this._button1);
            this._groupBox3.Controls.Add(this._richTextBox1);
            this._groupBox3.Location = new System.Drawing.Point(563, 49);
            this._groupBox3.Name = "_groupBox3";
            this._groupBox3.Size = new System.Drawing.Size(710, 523);
            this._groupBox3.TabIndex = 12;
            this._groupBox3.TabStop = false;
            this._groupBox3.Text = "Log";
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
            this._enableLoggingCheckBox.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // _button1
            // 
            this._button1.Location = new System.Drawing.Point(629, 20);
            this._button1.Name = "_button1";
            this._button1.Size = new System.Drawing.Size(75, 23);
            this._button1.TabIndex = 1;
            this._button1.Text = "Clear Logs";
            this._button1.UseVisualStyleBackColor = true;
            this._button1.Click += new System.EventHandler(this.button1_Click);
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
            // MainControl
            // 
            this.Controls.Add(this._groupBox3);
            this.Controls.Add(this._toadNotificationCheckBox);
            this.Controls.Add(this._groupBox2);
            this.Controls.Add(this._groupBox1);
            this.Controls.Add(this._label1);
            this.Controls.Add(this._languageComboBox);
            this.Name = "MainControl";
            this.Size = new System.Drawing.Size(1744, 592);
            this._groupBox1.ResumeLayout(false);
            this._groupBox1.PerformLayout();
            this._groupBox2.ResumeLayout(false);
            this._groupBox2.PerformLayout();
            this._groupBox3.ResumeLayout(false);
            this._groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region IActPluginV1 Implementations
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            // The shortcut must be created to work with windows 8/10 Toasts
            ShortCutCreator.TryCreateShortcut(AppId, AppId);

            _isPluginEnabled = true;
            _labelStatus = pluginStatusText;
            _labelStatus.Text = @"DFAssist Plugin Started.";
            pluginScreenSpace.Text = @"DFAssist";

            _initDisposable = Task.Factory.StartNew(() =>
            {
                // should languages be online? for the moment i think it is ok for them to be here
                _languageComboBox.DataSource = new[]
                {
                    new Language {Name = "English", Code = "en-us"},
                    new Language {Name = "한국어", Code = "ko-kr"},
                    new Language {Name = "日本語", Code = "ja-jp"},
                    new Language {Name = "Français", Code = "fr-fr"},
                };
                _languageComboBox.DisplayMember = "Name";
                _languageComboBox.ValueMember = "Code";

                pluginStatusText.Invoke(new Action(() =>
                {
                    _labelStatus.Text = @"DFAssist Plugin Started.";

                    pluginScreenSpace.Controls.Add(this);
                    _xmlSettingsSerializer = new SettingsSerializer(this);

                    if (_timer == null)
                    {
                        _timer = new Timer { Interval = 30000 };
                        _timer.Tick += Timer_Tick;
                    }

                    _timer.Enabled = true;

                    UpdateProcesses();

                    LoadData();
                    LoadSettings();
                    LoadFates();
                }));
            });
        }

        public void DeInitPlugin()
        {
            _isPluginEnabled = false;
            Logger.SetLoggerTextBox(null);

            _initDisposable?.Dispose();

            if (_labelStatus != null)
            {
                _labelStatus.Text = @"DFAssist Plugin Unloaded.";
                _labelStatus = null;
            }

            foreach (var entry in _networks)
            {
                entry.Value.Network.StopCapture();
            }

            _timer.Enabled = false;
            SaveSettings();
        }
        #endregion

        #region Getters
        private static string GetInstanceName(int code)
        {
            return Data.GetInstance(code).Name;
        }

        private static string GetFateName(int code)
        {
            return Data.GetFate(code).Name;
        }

        private static string GetAreaNameFromFate(int code)
        {
            return Data.GetFate(code).Area.Name;
        }

        private static string GetRouletteName(int code)
        {
            return Data.GetRoulette(code).Name;
        }
        #endregion

        #region Load Methods
        private void LoadData()
        {
            _selectedLanguage = (string)_languageComboBox.SelectedValue;
            Data.Initialize(_selectedLanguage);
        }

        private void LoadFates()
        {
            TelegramFateTreeView.Nodes.Clear();

            var checkedFates = new List<string>();
            if (!string.IsNullOrEmpty(_checkedFates))
            {
                var split = _checkedFates.Split('|');
                checkedFates.AddRange(split);
            }

            _lockTreeEvent = true;

            foreach (var area in Data.Areas)
            {
                var areaNode = TelegramFateTreeView.Nodes.Add(area.Value.Name);
                areaNode.Tag = "AREA:" + area.Key;

                if (checkedFates.Contains((string)areaNode.Tag))
                    areaNode.Checked = true;

                foreach (var fate in area.Value.Fates)
                {
                    var fateName = fate.Value.Name;
                    var fateNode = areaNode.Nodes.Add(fateName);
                    fateNode.Tag = fate.Key.ToString();

                    if (checkedFates.Contains((string)fateNode.Tag))
                        fateNode.Checked = true;
                }
            }

            _telegramSelectedFates.Clear();
            UpdateSelectedFates(TelegramFateTreeView.Nodes);
            _lockTreeEvent = false;
        }
        #endregion

        #region Update Methods
        private void UpdateProcesses()
        {
            var processes = new List<Process>();
            processes.AddRange(Process.GetProcessesByName("ffxiv"));
            processes.AddRange(Process.GetProcessesByName("ffxiv_dx11"));

            foreach (var process in processes)
            {
                try
                {
                    if (_networks.ContainsKey(process.Id)) continue;
                    var pn = new ProcessNet(process, new Network());
                    FFXIVPacketHandler.OnEventReceived += Network_onReceiveEvent;
                    _networks.TryAdd(process.Id, pn);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "error");
                }
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
                    _networks.TryRemove(t, out var _);
                    FFXIVPacketHandler.OnEventReceived -= Network_onReceiveEvent;
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "error");
                }
            }
        }

        private void UpdateSelectedFates(IEnumerable nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked)
                    _telegramSelectedFates.Push((string)node.Tag);

                UpdateSelectedFates(node.Nodes);
            }
        }
        #endregion

        #region Post Method
        private static void SendToAct(string text)
        {
            ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "00|" + DateTime.Now.ToString("O") + "|0048|F|" + text);
        }

        private void PostToTelegramIfNeeded(string server, EventType eventType, int[] args)
        {
            if (eventType != EventType.FATE_BEGIN && eventType != EventType.MATCH_ALERT) return;
            if (_isTelegramEnabled == false) return;

            var head = _networks.Count <= 1 ? "" : "[" + server + "] ";
            switch (eventType)
            {
                case EventType.MATCH_ALERT:
                    if (_isDutyAlertEnabled)
                        PostToTelegram(head + GetRouletteName(args[0]) + " >> " + GetInstanceName(args[1]));
                    break;
                case EventType.FATE_BEGIN:
                    if (_telegramSelectedFates.Contains(args[0].ToString()))
                        PostToTelegram(head + GetAreaNameFromFate(args[0]) + " >> " + GetFateName(args[0]));
                    break;
            }
        }

        private void PostToToastWindowsNotificationIfNeeded(string server, EventType eventType, int[] args)
        {
            if (eventType != EventType.FATE_BEGIN && eventType != EventType.MATCH_ALERT) return;
            if (_isToastNotificationEnabled == false) return;

            var head = _networks.Count <= 1 ? "" : "[" + server + "] ";
            switch (eventType)
            {
                case EventType.MATCH_ALERT:
                    if (_isDutyAlertEnabled)
                        ToastWindowNotification(head + GetRouletteName(args[0]) + " >> " + GetInstanceName(args[1]));
                    break;
                case EventType.FATE_BEGIN:
                    if (_telegramSelectedFates.Contains(args[0].ToString()))
                        ToastWindowNotification(head + GetAreaNameFromFate(args[0]) + " >> " + GetFateName(args[0]));
                    break;
            }
        }

        private void PostToTelegram(string message)
        {
            string chatId = _telegramChatIdTextBox.Text, token = _telegramTokenTextBox.Text;
            if (string.IsNullOrEmpty(chatId) || token == null || token == "") return;

            using (var client = new WebClient())
            {
                client.UploadValues("https://api.telegram.org/bot" + token + "/sendMessage", new NameValueCollection
                {
                    {"chat_id", chatId},
                    {"text", message}
                });
            }
        }

        private static void ToastWindowNotification(string text)
        {
            try
            {
                // Get a toast XML template
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText03);

                // Fill in the text elements
                var stringElements = toastXml.GetElementsByTagName("text");
                foreach (var t in stringElements)
                {
                    t.AppendChild(toastXml.CreateTextNode(text));
                }

                // Specify the absolute path to an image
                var imagePath = "file:///" + Path.GetFullPath("toastImageAndText.png");

                var imageElements = toastXml.GetElementsByTagName("image");
                var xmlNamedNodeMap = imageElements?[0].Attributes;
                var namedItem = xmlNamedNodeMap?.GetNamedItem("src");
                if (namedItem != null)
                    namedItem.NodeValue = imagePath;

                // Create the toast and attach event listeners
                var toast = new ToastNotification(toastXml);

                // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
                ToastNotificationManager.CreateToastNotifier(AppId).Show(toast);
            }
            catch (Exception e)
            {
                Logger.LogException(e, "error");
            }
        }
        #endregion

        #region Events
        private void CheckBoxTelegram_CheckedChanged(object sender, EventArgs e)
        {
            _isTelegramEnabled = _telegramCheckBox.Checked;
            _telegramChatIdTextBox.Enabled = !_isTelegramEnabled;
            _telegramTokenTextBox.Enabled = !_isTelegramEnabled;
        }

        private void CheckBoxTelegramDutyFinder_CheckedChanged(object sender, EventArgs e)
        {
            _isDutyAlertEnabled = _dutyFinderAlertCheckBox.Checked;
        }

        private void FateTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_lockTreeEvent) return;
            _lockTreeEvent = true;
            if (((string)e.Node.Tag).Contains("AREA:"))
            {
                foreach (TreeNode node in e.Node.Nodes) node.Checked = e.Node.Checked;
            }
            else
            {
                if (e.Node.Checked == false)
                {
                    e.Node.Parent.Checked = false;
                }
                else
                {
                    var flag = true;
                    foreach (TreeNode node in e.Node.Parent.Nodes) flag &= node.Checked;
                    e.Node.Parent.Checked = flag;
                }
            }

            _telegramSelectedFates.Clear();
            UpdateSelectedFates(TelegramFateTreeView.Nodes);

            SaveSettings();

            _lockTreeEvent = false;
        }

        private void Network_onReceiveEvent(int pid, EventType eventType, int[] args)
        {
            var server = _networks[pid].Process.MainModule.FileName.Contains("KOREA") ? "KOREA" : "GLOBAL";
            var text = pid + "|" + server + "|" + eventType + "|";


            var pos = 0;
            var isFate = false;
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
                case EventType.FATE_BEGIN:
                case EventType.FATE_PROGRESS:
                case EventType.FATE_END:
                    isFate = true;
                    text += GetFateName(args[0]) + "|" + GetAreaNameFromFate(args[0]) + "|";
                    pos++;
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

            if (isFate) text += args[0] + "|";

            SendToAct(text);

            PostToToastWindowsNotificationIfNeeded(server, eventType, args);
            PostToTelegramIfNeeded(server, eventType, args);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_isPluginEnabled == false) return;

            UpdateProcesses();
        }

        private void _checkBoxToastNotification_CheckedChanged(object sender, EventArgs e)
        {
            Logger.LogInfo("Test");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _richTextBox1.Clear();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!_enableLoggingCheckBox.Checked)
            {
                Logger.SetLoggerTextBox(null);
                _button1.Enabled = false;
            }
            else
            {
                Logger.SetLoggerTextBox(_richTextBox1);
                _button1.Enabled = true;
            }
        }

        private void _comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData();
            LoadFates();
        }
        #endregion

        #region Settings
        private void LoadSettings()
        {
            // All the settings to deserialize
            _xmlSettingsSerializer.AddControlSetting(_languageComboBox.Name, _languageComboBox);
            _xmlSettingsSerializer.AddControlSetting(_toadNotificationCheckBox.Name, _toadNotificationCheckBox);
            _xmlSettingsSerializer.AddControlSetting(_telegramCheckBox.Name, _telegramCheckBox);
            _xmlSettingsSerializer.AddControlSetting(_telegramChatIdTextBox.Name, _telegramChatIdTextBox);
            _xmlSettingsSerializer.AddControlSetting(_telegramTokenTextBox.Name, _telegramTokenTextBox);
            _xmlSettingsSerializer.AddControlSetting(_dutyFinderAlertCheckBox.Name, _dutyFinderAlertCheckBox);
            _xmlSettingsSerializer.AddControlSetting(_enableLoggingCheckBox.Name, _enableLoggingCheckBox);
            _xmlSettingsSerializer.AddStringSetting("CheckedFates");

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
                        _labelStatus.Text = @"Error loading settings: " + ex.Message;
                    }

                    xmlTextReader.Close();
                }
            }

            _isTelegramEnabled = _telegramCheckBox.Checked;
            _telegramChatIdTextBox.Enabled = !_isTelegramEnabled;
            _telegramTokenTextBox.Enabled = !_isTelegramEnabled;
            _isDutyAlertEnabled = _dutyFinderAlertCheckBox.Checked;
            _isToastNotificationEnabled = _toadNotificationCheckBox.Checked;
            _selectedLanguage = (string)_languageComboBox.SelectedValue;
        }

        private void SaveSettings()
        {
            _checkedFates = string.Empty;

            var fatesList = new List<string>();
            foreach (TreeNode area in TelegramFateTreeView.Nodes)
            {
                if (area.Checked)
                    fatesList.Add((string)area.Tag);

                foreach (TreeNode fate in area.Nodes)
                {
                    if (fate.Checked)
                        fatesList.Add((string)fate.Tag);
                }
            }

            _checkedFates = string.Join("|", fatesList);

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
        #endregion
    }
}