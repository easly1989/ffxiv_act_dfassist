// reference:System.dll
// reference:System.Core.dll
// reference:System.Web.Extensions.dll

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
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
        private FileInfo _fileInfo;
        private CheckBox _checkBoxTelegram;
        private CheckBox _checkBoxTelegramDutyFinder;
        private CheckBox _checkBoxToastNotification;
        private ComboBox _comboBoxLanguage;
        private GroupBox _groupBox1;
        private GroupBox _groupBox2;
        private SettingsSerializer _xmlSettingsSerializer;

        public TreeView TelegramFateTreeView { get; private set; }

        public MainControl()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                var name = new AssemblyName(Assembly.GetExecutingAssembly().FullName).Name + "." + new AssemblyName(args.Name).Name + ".dll";
                using (var assemblyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
                {
                    byte[] assemblyBuffer = new byte[assemblyStream.Length];
                    assemblyStream.Read(assemblyBuffer, 0, assemblyBuffer.Length);
                    return Assembly.Load(assemblyBuffer);
                }
            };

            _networks = new ConcurrentDictionary<int, ProcessNet>();
            _settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\ACTFate.config.xml");
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
                _labelStatus.Text = @"Downloading Data.json";
                LoadData();
                _labelStatus.Text = @"Downloaded Data.json";

                pluginStatusText.Invoke(new Action(delegate
                {
                    _labelStatus.Text = "DFAssist Plugin Started.";

                    pluginScreenSpace.Controls.Add(this);
                    _xmlSettingsSerializer = new SettingsSerializer(this);

                    foreach (var plugin in ActGlobals.oFormActMain.ActPlugins)
                    {
                        if (plugin.pluginObj != this)
                            continue;
                        _fileInfo = plugin.pluginFile;
                        break;
                    }

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
            Logger.RichTextBox = null;
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
                    _labelStatus.Text = "Error loading settings: " + ex.Message;
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

            for (var i = 0; i < processes.Count; i++)
            {
                var process = processes[i];
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

            for (var i = 0; i < toDelete.Count; i++)
            {
                try
                {
                    _networks.TryRemove(toDelete[i], out ProcessNet pn);
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
            _label1 = new Label();
            _comboBoxLanguage = new ComboBox();
            _groupBox1 = new GroupBox();
            _textTelegramToken = new TextBox();
            _label3 = new Label();
            _textTelegramChatId = new TextBox();
            _label2 = new Label();
            _checkBoxTelegram = new CheckBox();
            _groupBox2 = new GroupBox();
            _label4 = new Label();
            TelegramFateTreeView = new TreeView();
            _checkBoxTelegramDutyFinder = new CheckBox();
            _checkBoxToastNotification = new CheckBox();
            _groupBox1.SuspendLayout();
            _groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            _label1.AutoSize = true;
            _label1.Location = new Point(21, 17);
            _label1.Name = "_label1";
            _label1.Size = new Size(61, 12);
            _label1.TabIndex = 7;
            _label1.Text = "Language";
            // 
            // comboBoxLanguage
            // 
            _comboBoxLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            _comboBoxLanguage.FormattingEnabled = true;
            _comboBoxLanguage.Location = new Point(88, 14);
            _comboBoxLanguage.Name = "_comboBoxLanguage";
            _comboBoxLanguage.Size = new Size(121, 20);
            _comboBoxLanguage.TabIndex = 6;
            _comboBoxLanguage.SelectedIndexChanged += ComboBoxLanguage_SelectedIndexChanged;
            // 
            // groupBox1
            // 
            _groupBox1.Controls.Add(_textTelegramToken);
            _groupBox1.Controls.Add(_label3);
            _groupBox1.Controls.Add(_textTelegramChatId);
            _groupBox1.Controls.Add(_label2);
            _groupBox1.Controls.Add(_checkBoxTelegram);
            _groupBox1.Location = new Point(23, 49);
            _groupBox1.Name = "_groupBox1";
            _groupBox1.Size = new Size(533, 51);
            _groupBox1.TabIndex = 9;
            _groupBox1.TabStop = false;
            _groupBox1.Text = "Telegram";
            // 
            // textTelegramToken
            // 
            _textTelegramToken.Location = new Point(232, 20);
            _textTelegramToken.Name = "_textTelegramToken";
            _textTelegramToken.Size = new Size(291, 21);
            _textTelegramToken.TabIndex = 9;
            // 
            // label3
            // 
            _label3.AutoSize = true;
            _label3.Location = new Point(186, 23);
            _label3.Name = "_label3";
            _label3.Size = new Size(40, 12);
            _label3.TabIndex = 8;
            _label3.Text = "Token";
            // 
            // textTelegramChatID
            // 
            _textTelegramChatId.Location = new Point(67, 20);
            _textTelegramChatId.Name = "_textTelegramChatId";
            _textTelegramChatId.Size = new Size(100, 21);
            _textTelegramChatId.TabIndex = 7;
            // 
            // label2
            // 
            _label2.AutoSize = true;
            _label2.Location = new Point(15, 23);
            _label2.Name = "_label2";
            _label2.Size = new Size(46, 12);
            _label2.TabIndex = 6;
            _label2.Text = "Chat ID";
            // 
            // checkBoxTelegram
            // 
            _checkBoxTelegram.AutoSize = true;
            _checkBoxTelegram.Location = new Point(67, 0);
            _checkBoxTelegram.Name = "_checkBoxTelegram";
            _checkBoxTelegram.Size = new Size(58, 16);
            _checkBoxTelegram.TabIndex = 5;
            _checkBoxTelegram.Text = "Active";
            _checkBoxTelegram.UseVisualStyleBackColor = true;
            _checkBoxTelegram.CheckedChanged += CheckBoxTelegram_CheckedChanged;
            // 
            // groupBox2
            // 
            _groupBox2.Controls.Add(_label4);
            _groupBox2.Controls.Add(TelegramFateTreeView);
            _groupBox2.Controls.Add(_checkBoxTelegramDutyFinder);
            _groupBox2.Location = new Point(23, 115);
            _groupBox2.Name = "_groupBox2";
            _groupBox2.Size = new Size(533, 457);
            _groupBox2.TabIndex = 10;
            _groupBox2.TabStop = false;
            _groupBox2.Text = "Alert";
            // 
            // label4
            // 
            _label4.AutoSize = true;
            _label4.Location = new Point(15, 57);
            _label4.Name = "_label4";
            _label4.Size = new Size(48, 12);
            _label4.TabIndex = 10;
            _label4.Text = "F.A.T.E";
            // 
            // telegramFateTreeView
            // 
            TelegramFateTreeView.CheckBoxes = true;
            TelegramFateTreeView.Location = new Point(15, 81);
            TelegramFateTreeView.Name = "TelegramFateTreeView";
            TelegramFateTreeView.Size = new Size(508, 370);
            TelegramFateTreeView.TabIndex = 9;
            TelegramFateTreeView.AfterCheck += FateTreeView_AfterCheck;
            // 
            // checkBoxTelegramDutyFinder
            // 
            _checkBoxTelegramDutyFinder.AutoSize = true;
            _checkBoxTelegramDutyFinder.Checked = true;
            _checkBoxTelegramDutyFinder.CheckState = CheckState.Checked;
            _checkBoxTelegramDutyFinder.Location = new Point(15, 22);
            _checkBoxTelegramDutyFinder.Name = "_checkBoxTelegramDutyFinder";
            _checkBoxTelegramDutyFinder.Size = new Size(88, 16);
            _checkBoxTelegramDutyFinder.TabIndex = 8;
            _checkBoxTelegramDutyFinder.Text = "Duty Finder";
            _checkBoxTelegramDutyFinder.UseVisualStyleBackColor = true;
            _checkBoxTelegramDutyFinder.CheckedChanged += CheckBoxTelegramDutyFinder_CheckedChanged;
            // 
            // checkBoxToastNotification
            // 
            _checkBoxToastNotification.AutoSize = true;
            _checkBoxToastNotification.Location = new Point(255, 18);
            _checkBoxToastNotification.Name = "_checkBoxToastNotification";
            _checkBoxToastNotification.Size = new Size(160, 16);
            _checkBoxToastNotification.TabIndex = 11;
            _checkBoxToastNotification.Text = "Active Toast Notification";
            _checkBoxToastNotification.UseVisualStyleBackColor = true;
            _checkBoxToastNotification.CheckedChanged += CheckBoxToastNotification_CheckedChanged;
            // 
            // ACTFate
            // 
            Controls.Add(_checkBoxToastNotification);
            Controls.Add(_groupBox2);
            Controls.Add(_groupBox1);
            Controls.Add(_label1);
            Controls.Add(_comboBoxLanguage);
            Name = "MainControl";
            Size = new Size(1744, 592);
            _groupBox1.ResumeLayout(false);
            _groupBox1.PerformLayout();
            _groupBox2.ResumeLayout(false);
            _groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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

            return areaCode == null ? code.ToString() : areaCode;
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
            var jsonString = DownloadData("https://raw.githubusercontent.com/easly1989/ffxiv_act_dfassist/master/data.json") ??
                             File.ReadAllText(_fileInfo.Directory?.FullName + "/data.json");

            var json = JObject.Parse(jsonString);
            var langs = json["languages"];

            _comboBoxLanguage.DataSource = langs.Select(language => ((JProperty)language).Name).Select(key => new Language { Name = langs[key].ToString(), Code = key }).ToArray();
            _comboBoxLanguage.DisplayMember = "Name";
            _comboBoxLanguage.ValueMember = "Code";
            _selectedLanguage = (string)_comboBoxLanguage.SelectedValue;

            _data = json;
        }


        private void LoadFates()
        {
            TelegramFateTreeView.Nodes.Clear();

            var c = new List<string>();
            if (_telegramChkFates != null && _telegramChkFates != "")
            {
                var sp = _telegramChkFates.Split('|');
                for (var i = 0; i < sp.Length; i++) c.Add(sp[i]);
            }

            _lockTreeEvent = true;
            foreach (JProperty item in _data["areas"])
            {
                var key = item.Name;
                var areaNode = TelegramFateTreeView.Nodes.Add(_data["areas"][key][_selectedLanguage].ToString());
                areaNode.Tag = "AREA:" + key;
                if (c.Contains((string)areaNode.Tag)) areaNode.Checked = true;
                foreach (JProperty fate in _data["fates"])
                {
                    if (_data["fates"][fate.Name]["area_code"].ToString().Equals(key) == false) continue;
                    var text = _data["fates"][fate.Name]["name"][_selectedLanguage].ToString();
                    if (text == null || text == "") text = _data["fates"][fate.Name]["name"]["en"].ToString();
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
            string chatID = _textTelegramChatId.Text, token = _textTelegramToken.Text;
            if (chatID == null || chatID == "" || token == null || token == "") return;

            using (var client = new WebClient())
            {
                client.UploadValues("https://api.telegram.org/bot" + token + "/sendMessage", new NameValueCollection
                {
                    {"chat_id", chatID},
                    {"text", message}
                });
            }
        }

        private void ToastWindowNotification(string text)
        {
            try
            {
                // Get a toast XML template
                Windows.Data.Xml.Dom.XmlDocument toastXml = Windows.UI.Notifications.ToastNotificationManager.GetTemplateContent(Windows.UI.Notifications.ToastTemplateType.ToastImageAndText03);

                // Fill in the text elements
                Windows.Data.Xml.Dom.XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
                for (var i = 0; i < stringElements.Length; i++)
                    stringElements[i].AppendChild(toastXml.CreateTextNode(text));

                // Specify the absolute path to an image
                var imagePath = "file:///" + Path.GetFullPath("toastImageAndText.png");

                Windows.Data.Xml.Dom.XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
                imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                // Create the toast and attach event listeners
                Windows.UI.Notifications.ToastNotification toast = new Windows.UI.Notifications.ToastNotification(toastXml);

                // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
                Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier(AppId).Show(toast);
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
    }
}