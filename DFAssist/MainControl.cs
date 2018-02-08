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

        private bool _active;
        private bool _lockTreeEvent;
        private bool _isTelegramDutyAlertEnable;
        private bool _isTelegramEnable;
        private bool _isToastNotificationEnable;
        private string _selectedLanguage;
        private string _telegramChkFates;
        private Timer _timer;
        private Label _label1;
        private Label _label2;
        private Label _label3;
        private Label _label4;
        private Label _labelStatus;
        private JObject _data;
        private TextBox _textTelegramChatId;
        private TextBox _textTelegramToken;
        private CheckBox _checkBoxTelegram;
        private CheckBox _checkBoxTelegramDutyFinder;
        private CheckBox _checkBoxToastNotification;
        private ComboBox _comboBoxLanguage;
        private GroupBox _groupBox1;
        private GroupBox _groupBox2;
        private SettingsSerializer _xmlSettingsSerializer;
        private GroupBox _groupBox3;
        private RichTextBox _richTextBox1;
        private CheckBox _checkBox1;
        private Button _button1;
        public TreeView TelegramFateTreeView;

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

            _networks = new ConcurrentDictionary<int, ProcessNet>();
            _settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\DFAssist.config.xml");
            _telegramSelectedFates = new ConcurrentStack<string>();
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            ShortCutCreator.TryCreateShortcut(AppId, AppId);

            _active = true;
            _labelStatus = pluginStatusText;
            _labelStatus.Text = @"DFAssist Plugin Started.";
            pluginScreenSpace.Text = @"DFAssist";

            Task.Factory.StartNew(() =>
            {
                _comboBoxLanguage.DataSource = new[]
                {
                    new Language {Name = "English", Code = "en-us"},
                    new Language {Name = "한국어", Code = "ko-kr"},
                    new Language {Name = "日本語", Code = "ja-jp"},
                    new Language {Name = "Français", Code = "fr-fr"},
                };
                _comboBoxLanguage.DisplayMember = "Name";
                _comboBoxLanguage.ValueMember = "Code";

                pluginStatusText.Invoke(new Action(delegate
                {
                    _labelStatus.Text = @"DFAssist Plugin Started.";

                    pluginScreenSpace.Controls.Add(this);
                    _xmlSettingsSerializer = new SettingsSerializer(this);

                    if (_timer == null)
                    {
                        _timer = new Timer
                        {
                            Interval = 30000
                        };
                        _timer.Tick += Timer_Tick;
                    }

                    _timer.Enabled = true;

                    UpdateFfxivProcesses();

                    LoadSettings();
                    _comboBoxLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
                    _selectedLanguage = (string)_comboBoxLanguage.SelectedValue;
                    LoadFates();
                }));
            });
        }

        public void DeInitPlugin()
        {
            _active = false;
            Logger.SetLoggerTextBox(null);

            if (_labelStatus != null)
            {
                _labelStatus.Text = @"DFAssist Plugin Unloaded.";
                _labelStatus = null;
            }

            foreach (var entry in _networks) entry.Value.Network.StopCapture();

            _timer.Enabled = false;
            SaveSettings();
        }

        private void LoadSettings()
        {
            _xmlSettingsSerializer.AddControlSetting(_comboBoxLanguage.Name, _comboBoxLanguage);
            _xmlSettingsSerializer.AddControlSetting(_checkBoxToastNotification.Name, _checkBoxToastNotification);

            _xmlSettingsSerializer.AddControlSetting(_checkBoxTelegram.Name, _checkBoxTelegram);
            _xmlSettingsSerializer.AddControlSetting(_textTelegramChatId.Name, _textTelegramChatId);
            _xmlSettingsSerializer.AddControlSetting(_textTelegramToken.Name, _textTelegramToken);
            _xmlSettingsSerializer.AddControlSetting(_checkBoxTelegramDutyFinder.Name, _checkBoxTelegramDutyFinder);
            _xmlSettingsSerializer.AddStringSetting("telegramChkFates");

            if (File.Exists(_settingsFile))
            {
                var fs = new FileStream(_settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var xReader = new XmlTextReader(fs);

                try
                {
                    while (xReader.Read())
                        if (xReader.NodeType == XmlNodeType.Element)
                            if (xReader.LocalName == "SettingsSerializer")
                                _xmlSettingsSerializer.ImportFromXml(xReader);
                }
                catch (Exception ex)
                {
                    _labelStatus.Text = @"Error loading settings: " + ex.Message;
                }

                xReader.Close();
            }

            _isTelegramEnable = _checkBoxTelegram.Checked;
            _textTelegramChatId.Enabled = !_isTelegramEnable;
            _textTelegramToken.Enabled = !_isTelegramEnable;

            _isTelegramDutyAlertEnable = _checkBoxTelegramDutyFinder.Checked;
            _isToastNotificationEnable = _checkBoxToastNotification.Checked;
        }

        private void SaveSettings()
        {
            _telegramChkFates = "";
            var c = new List<string>();
            foreach (TreeNode area in TelegramFateTreeView.Nodes)
            {
                if (area.Checked) c.Add((string)area.Tag);
                foreach (TreeNode fate in area.Nodes)
                    if (fate.Checked)
                        c.Add((string)fate.Tag);
            }

            _telegramChkFates = string.Join("|", c);

            var fs = new FileStream(_settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            var xWriter = new XmlTextWriter(fs, Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                Indentation = 1,
                IndentChar = '\t'
            };
            xWriter.WriteStartDocument(true);
            xWriter.WriteStartElement("Config"); // <Config>
            xWriter.WriteStartElement("SettingsSerializer"); // <Config><SettingsSerializer>
            _xmlSettingsSerializer.ExportToXml(xWriter); // Fill the SettingsSerializer XML
            xWriter.WriteEndElement(); // </SettingsSerializer>
            xWriter.WriteEndElement(); // </Config>
            xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
            xWriter.Flush(); // Flush the file buffer to disk
            xWriter.Close();
        }

        private void UpdateFfxivProcesses()
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

        private void InitializeComponent()
        {
            this._label1 = new System.Windows.Forms.Label();
            this._comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this._groupBox1 = new System.Windows.Forms.GroupBox();
            this._textTelegramToken = new System.Windows.Forms.TextBox();
            this._label3 = new System.Windows.Forms.Label();
            this._textTelegramChatId = new System.Windows.Forms.TextBox();
            this._label2 = new System.Windows.Forms.Label();
            this._checkBoxTelegram = new System.Windows.Forms.CheckBox();
            this._groupBox2 = new System.Windows.Forms.GroupBox();
            this._label4 = new System.Windows.Forms.Label();
            this.TelegramFateTreeView = new System.Windows.Forms.TreeView();
            this._checkBoxTelegramDutyFinder = new System.Windows.Forms.CheckBox();
            this._checkBoxToastNotification = new System.Windows.Forms.CheckBox();
            this._groupBox3 = new System.Windows.Forms.GroupBox();
            this._checkBox1 = new System.Windows.Forms.CheckBox();
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
            // _comboBoxLanguage
            // 
            this._comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comboBoxLanguage.FormattingEnabled = true;
            this._comboBoxLanguage.Location = new System.Drawing.Point(88, 14);
            this._comboBoxLanguage.Name = "_comboBoxLanguage";
            this._comboBoxLanguage.Size = new System.Drawing.Size(121, 21);
            this._comboBoxLanguage.TabIndex = 6;
            this._comboBoxLanguage.SelectedIndexChanged += new System.EventHandler(this._comboBoxLanguage_SelectedIndexChanged);
            // 
            // _groupBox1
            // 
            this._groupBox1.Controls.Add(this._textTelegramToken);
            this._groupBox1.Controls.Add(this._label3);
            this._groupBox1.Controls.Add(this._textTelegramChatId);
            this._groupBox1.Controls.Add(this._label2);
            this._groupBox1.Controls.Add(this._checkBoxTelegram);
            this._groupBox1.Location = new System.Drawing.Point(23, 49);
            this._groupBox1.Name = "_groupBox1";
            this._groupBox1.Size = new System.Drawing.Size(533, 51);
            this._groupBox1.TabIndex = 9;
            this._groupBox1.TabStop = false;
            this._groupBox1.Text = "Telegram";
            // 
            // _textTelegramToken
            // 
            this._textTelegramToken.Location = new System.Drawing.Point(232, 20);
            this._textTelegramToken.Name = "_textTelegramToken";
            this._textTelegramToken.Size = new System.Drawing.Size(291, 20);
            this._textTelegramToken.TabIndex = 9;
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
            // _textTelegramChatId
            // 
            this._textTelegramChatId.Location = new System.Drawing.Point(67, 20);
            this._textTelegramChatId.Name = "_textTelegramChatId";
            this._textTelegramChatId.Size = new System.Drawing.Size(100, 20);
            this._textTelegramChatId.TabIndex = 7;
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
            // _checkBoxTelegram
            // 
            this._checkBoxTelegram.AutoSize = true;
            this._checkBoxTelegram.Location = new System.Drawing.Point(67, 0);
            this._checkBoxTelegram.Name = "_checkBoxTelegram";
            this._checkBoxTelegram.Size = new System.Drawing.Size(56, 17);
            this._checkBoxTelegram.TabIndex = 5;
            this._checkBoxTelegram.Text = "Active";
            this._checkBoxTelegram.UseVisualStyleBackColor = true;
            // 
            // _groupBox2
            // 
            this._groupBox2.Controls.Add(this._label4);
            this._groupBox2.Controls.Add(this.TelegramFateTreeView);
            this._groupBox2.Controls.Add(this._checkBoxTelegramDutyFinder);
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
            // 
            // _checkBoxTelegramDutyFinder
            // 
            this._checkBoxTelegramDutyFinder.AutoSize = true;
            this._checkBoxTelegramDutyFinder.Checked = true;
            this._checkBoxTelegramDutyFinder.CheckState = System.Windows.Forms.CheckState.Checked;
            this._checkBoxTelegramDutyFinder.Location = new System.Drawing.Point(15, 22);
            this._checkBoxTelegramDutyFinder.Name = "_checkBoxTelegramDutyFinder";
            this._checkBoxTelegramDutyFinder.Size = new System.Drawing.Size(80, 17);
            this._checkBoxTelegramDutyFinder.TabIndex = 8;
            this._checkBoxTelegramDutyFinder.Text = "Duty Finder";
            this._checkBoxTelegramDutyFinder.UseVisualStyleBackColor = true;
            // 
            // _checkBoxToastNotification
            // 
            this._checkBoxToastNotification.AutoSize = true;
            this._checkBoxToastNotification.Location = new System.Drawing.Point(255, 18);
            this._checkBoxToastNotification.Name = "_checkBoxToastNotification";
            this._checkBoxToastNotification.Size = new System.Drawing.Size(142, 17);
            this._checkBoxToastNotification.TabIndex = 11;
            this._checkBoxToastNotification.Text = "Active Toast Notification";
            this._checkBoxToastNotification.UseVisualStyleBackColor = true;
            this._checkBoxToastNotification.CheckedChanged += new System.EventHandler(this._checkBoxToastNotification_CheckedChanged);
            // 
            // _groupBox3
            // 
            this._groupBox3.Controls.Add(this._checkBox1);
            this._groupBox3.Controls.Add(this._button1);
            this._groupBox3.Controls.Add(this._richTextBox1);
            this._groupBox3.Location = new System.Drawing.Point(563, 49);
            this._groupBox3.Name = "_groupBox3";
            this._groupBox3.Size = new System.Drawing.Size(710, 523);
            this._groupBox3.TabIndex = 12;
            this._groupBox3.TabStop = false;
            this._groupBox3.Text = "Log";
            // 
            // _checkBox1
            // 
            this._checkBox1.AutoSize = true;
            this._checkBox1.Checked = true;
            this._checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this._checkBox1.Location = new System.Drawing.Point(7, 22);
            this._checkBox1.Name = "_checkBox1";
            this._checkBox1.Size = new System.Drawing.Size(100, 17);
            this._checkBox1.TabIndex = 2;
            this._checkBox1.Text = "Enable Logging";
            this._checkBox1.UseVisualStyleBackColor = true;
            this._checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
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
            this.Controls.Add(this._checkBoxToastNotification);
            this.Controls.Add(this._groupBox2);
            this.Controls.Add(this._groupBox1);
            this.Controls.Add(this._label1);
            this.Controls.Add(this._comboBoxLanguage);
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

        private string GetTextInstance(int code)
        {
            try
            {
                return _data["instances"][code.ToString()]["name"][_selectedLanguage].ToString();
            }
            catch (Exception e)
            {
                Logger.LogException(e, "ignore");
            }

            return code.ToString();
        }

        private string GetTextFate(int code)
        {
            try
            {
                var item = _data["fates"][code.ToString()]["name"];
                item = item[_selectedLanguage].ToString() == "" ? item["en"] : item[_selectedLanguage];
                return item.ToString();
            }
            catch (Exception e)
            {
                Logger.LogException(e, "ignore");
            }

            return code.ToString();
        }

        private string GetTextFateArea(int code)
        {
            string areaCode = null;
            try
            {
                areaCode = _data["fates"][code.ToString()]["area_code"].ToString();
                return _data["areas"][areaCode][_selectedLanguage].ToString();
            }
            catch (Exception e)
            {
                Logger.LogException(e, "ignore");
            }

            return areaCode ?? code.ToString();
        }

        private string GetTextRoulette(int code)
        {
            try
            {
                return code == 0 ? "" : _data["roulettes"][code.ToString()][_selectedLanguage].ToString();
            }
            catch (Exception e)
            {
                Logger.LogException(e, "ignore");
            }

            return code.ToString();
        }

        private void SendToAct(string text)
        {
            ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "00|" + DateTime.Now.ToString("O") + "|0048|F|" + text);
        }

        private string DownloadData(string url)
        {
            var webClient = new WebClient();
            webClient.Headers.Add("user-agent", "avoid 403");
            var downloadString = webClient.DownloadString(url);
            webClient.Dispose();
            return downloadString;
        }

        private void LoadData()
        {
            _selectedLanguage = (string)_comboBoxLanguage.SelectedValue;
            var jsonString = File.ReadAllText(@"D:\GIT\ffxiv_act_dfassist\data\en-us.json"); //DownloadData($"https://raw.githubusercontent.com/easly1989/ffxiv_act_dfassist/master/data/{_selectedLanguage}.json");

            var json = JObject.Parse(jsonString);        
            _data = json;
        }

        private void LoadFates()
        {
            TelegramFateTreeView.Nodes.Clear();

            var c = new List<string>();
            if (!string.IsNullOrEmpty(_telegramChkFates))
            {
                var sp = _telegramChkFates.Split('|');
                c.AddRange(sp);
            }

            _lockTreeEvent = true;
            foreach (var jToken in _data["areas"])
            {
                var item = (JProperty)jToken;
                var key = item.Name;
                var areaNode = TelegramFateTreeView.Nodes.Add(_data["areas"][key][_selectedLanguage].ToString());
                areaNode.Tag = "AREA:" + key;
                if (c.Contains((string)areaNode.Tag)) areaNode.Checked = true;
                foreach (var jToken1 in _data["fates"])
                {
                    var fate = (JProperty)jToken1;
                    if (_data["fates"][fate.Name]["area_code"].ToString().Equals(key) == false) continue;
                    var text = _data["fates"][fate.Name]["name"][_selectedLanguage].ToString();
                    if (string.IsNullOrEmpty(text)) text = _data["fates"][fate.Name]["name"]["en"].ToString();
                    var fateNode = areaNode.Nodes.Add(text);
                    fateNode.Tag = fate.Name;
                    if (c.Contains((string)fateNode.Tag)) fateNode.Checked = true;
                }
            }

            _telegramSelectedFates.Clear();
            UpdateSelectedFates(TelegramFateTreeView.Nodes);
            _lockTreeEvent = false;
        }

        private void UpdateSelectedFates(IEnumerable nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked) _telegramSelectedFates.Push((string)node.Tag);
                UpdateSelectedFates(node.Nodes);
            }
        }

        private void PostToTelegramIfNeeded(string server, EventType eventType, int[] args)
        {
            if (eventType != EventType.FATE_BEGIN && eventType != EventType.MATCH_ALERT) return;
            if (_isTelegramEnable == false) return;

            var head = _networks.Count <= 1 ? "" : "[" + server + "] ";
            switch (eventType)
            {
                case EventType.MATCH_ALERT:
                    //text += getTextRoulette(args[0]) + "|"; pos++;
                    //text += getTextInstance(args[1]) + "|"; pos++;
                    if (_isTelegramDutyAlertEnable)
                        PostToTelegram(head + GetTextRoulette(args[0]) + " >> " + GetTextInstance(args[1]));
                    break;
                case EventType.FATE_BEGIN:
                    //text += getTextFate(args[0]) + "|" + getTextFateArea(args[0]) + "|"; pos++;
                    if (_telegramSelectedFates.Contains(args[0].ToString()))
                        PostToTelegram(head + GetTextFateArea(args[0]) + " >> " + GetTextFate(args[0]));
                    break;
            }
        }

        private void PostToToastWindowsNotificationIfNeeded(string server, EventType eventType, int[] args)
        {
            if (eventType != EventType.FATE_BEGIN && eventType != EventType.MATCH_ALERT) return;
            if (_isToastNotificationEnable == false) return;

            var head = _networks.Count <= 1 ? "" : "[" + server + "] ";
            switch (eventType)
            {
                case EventType.MATCH_ALERT:
                    if (_isTelegramDutyAlertEnable)
                        ToastWindowNotification(head + GetTextRoulette(args[0]) + " >> " + GetTextInstance(args[1]));
                    break;
                case EventType.FATE_BEGIN:
                    if (_telegramSelectedFates.Contains(args[0].ToString()))
                        ToastWindowNotification(head + GetTextFateArea(args[0]) + " >> " + GetTextFate(args[0]));
                    break;
            }
        }

        private void PostToTelegram(string message)
        {
            string chatId = _textTelegramChatId.Text, token = _textTelegramToken.Text;
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

        private void CheckBoxTelegram_CheckedChanged(object sender, EventArgs e)
        {
            _isTelegramEnable = _checkBoxTelegram.Checked;
            _textTelegramChatId.Enabled = !_isTelegramEnable;
            _textTelegramToken.Enabled = !_isTelegramEnable;
        }

        private void CheckBoxTelegramDutyFinder_CheckedChanged(object sender, EventArgs e)
        {
            _isTelegramDutyAlertEnable = _checkBoxTelegramDutyFinder.Checked;
        }

        private void CheckBoxToastNotification_CheckedChanged(object sender, EventArgs e)
        {
            _isToastNotificationEnable = _checkBoxToastNotification.Checked;
        }
        private void ComboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedLanguage = (string)_comboBoxLanguage.SelectedValue;
            LoadFates();
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
                        text += GetTextInstance(args[0]) + "|";
                        pos++;
                    }

                    break;
                case EventType.FATE_BEGIN:
                case EventType.FATE_PROGRESS:
                case EventType.FATE_END:
                    isFate = true;
                    text += GetTextFate(args[0]) + "|" + GetTextFateArea(args[0]) + "|";
                    pos++;
                    break;
                case EventType.MATCH_BEGIN:
                    text += (MatchType)args[0] + "|";
                    pos++;
                    switch ((MatchType)args[0])
                    {
                        case MatchType.ROULETTE:
                            text += GetTextRoulette(args[1]) + "|";
                            pos++;
                            break;
                        case MatchType.SELECTIVE:
                            text += args[1] + "|";
                            pos++;
                            var p = pos;
                            for (var i = p; i < args.Length; i++)
                            {
                                text += GetTextInstance(args[i]) + "|";
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
                    text += GetTextInstance(args[0]) + "|";
                    pos++;
                    break;
                case EventType.MATCH_ALERT:
                    text += GetTextRoulette(args[0]) + "|";
                    pos++;
                    text += GetTextInstance(args[1]) + "|";
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
            if (_active == false) return;

            UpdateFfxivProcesses();
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
            if (!_checkBox1.Checked)
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
        }
    }
}