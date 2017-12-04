// reference:System.dll
// reference:System.Core.dll
// reference:System.Web.Extensions.dll
using System;
using System.Reflection;
using Advanced_Combat_Tracker;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Linq;

[assembly: AssemblyTitle("FFXIV F.A.T.E")]
//[assembly: AssemblyDescription("FATE Log")]
[assembly: AssemblyCompany("Wana")]
[assembly: AssemblyVersion("1.0.0.1")]

namespace FFXIV_FATE_ACT_Plugin
{
    public class ACTFate : System.Windows.Forms.UserControl, IActPluginV1
    {
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Timer timer;
        private bool active = false;
        private FileInfo fileInfo;
        string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\ACTFate.config.xml");
        SettingsSerializer xmlSettings;

        private class ProcessNet
        {
            public readonly Process process;
            public readonly App.Network network;
            public ProcessNet(Process process, App.Network network)
            {
                this.process = process;
                this.network = network;
            }
        }
        private ConcurrentDictionary<int, ProcessNet> networks = new ConcurrentDictionary<int, ProcessNet>();
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textTelegramToken;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textTelegramChatID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBoxTelegram;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TreeView telegramFateTreeView;
        private System.Windows.Forms.CheckBox checkBoxTelegramDutyFinder;
        private System.Windows.Forms.CheckBox checkBoxToastNotification;
        private static string APP_ID = "Advanced Combat Tracker"; // 아무거나 쓰면 됨 유니크하게
        public ACTFate()
        {
            InitializeComponent();
        }

        public void InitPlugin(System.Windows.Forms.TabPage pluginScreenSpace, System.Windows.Forms.Label pluginStatusText)
        {

            ShortCutCreator.TryCreateShortcut(APP_ID, APP_ID);
            active = true;
            this.lblStatus = pluginStatusText;
            this.lblStatus.Text = "FFXIV F.A.T.E Plugin Started.";
            pluginScreenSpace.Text = "FATE Parser";

            pluginScreenSpace.Controls.Add(this);
            xmlSettings = new SettingsSerializer(this);

            foreach (ActPluginData plugin in ActGlobals.oFormActMain.ActPlugins)
            {
                if (plugin.pluginObj != this) continue;
                fileInfo = plugin.pluginFile;
                break;
            }


            if (timer == null)
            {
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 30 * 1000;
                timer.Tick += Timer_Tick;
            }
            timer.Enabled = true;

            updateFFXIVProcesses();


            loadJSONData();

            LoadSettings();
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            selLng = (string)this.comboBoxLanguage.SelectedValue;
            loadFates();
        }

        void LoadSettings()
        {
            xmlSettings.AddControlSetting(comboBoxLanguage.Name, comboBoxLanguage);
            xmlSettings.AddControlSetting(checkBoxToastNotification.Name, checkBoxToastNotification);

            xmlSettings.AddControlSetting(checkBoxTelegram.Name, checkBoxTelegram);
            xmlSettings.AddControlSetting(textTelegramChatID.Name, textTelegramChatID);
            xmlSettings.AddControlSetting(textTelegramToken.Name, textTelegramToken);
            xmlSettings.AddControlSetting(checkBoxTelegramDutyFinder.Name, checkBoxTelegramDutyFinder);
            xmlSettings.AddStringSetting("telegramChkFates");

            if (File.Exists(settingsFile))
            {
                FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlTextReader xReader = new XmlTextReader(fs);

                try
                {
                    while (xReader.Read())
                    {
                        if (xReader.NodeType == XmlNodeType.Element)
                        {
                            if (xReader.LocalName == "SettingsSerializer")
                            {
                                xmlSettings.ImportFromXml(xReader);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Error loading settings: " + ex.Message;
                }
                xReader.Close();
            }
            isTelegramEnable = checkBoxTelegram.Checked;
            textTelegramChatID.Enabled = !isTelegramEnable;
            textTelegramToken.Enabled = !isTelegramEnable;

            isTelegramDutyAlertEnable = checkBoxTelegramDutyFinder.Checked;
            isToastNotificationEnable = checkBoxToastNotification.Checked;
        }
        void SaveSettings()
        {
            //tree
            telegramChkFates = "";
            List<string> c = new List<string>();
            foreach (System.Windows.Forms.TreeNode area in this.telegramFateTreeView.Nodes)
            {
                if (area.Checked) c.Add((string)area.Tag);
                foreach (System.Windows.Forms.TreeNode fate in area.Nodes)
                {
                    if (fate.Checked) c.Add((string)fate.Tag);
                }
            }
            telegramChkFates = string.Join("|", c);

            FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8);
            xWriter.Formatting = Formatting.Indented;
            xWriter.Indentation = 1;
            xWriter.IndentChar = '\t';
            xWriter.WriteStartDocument(true);
            xWriter.WriteStartElement("Config");    // <Config>
            xWriter.WriteStartElement("SettingsSerializer");    // <Config><SettingsSerializer>
            xmlSettings.ExportToXml(xWriter);   // Fill the SettingsSerializer XML
            xWriter.WriteEndElement();  // </SettingsSerializer>
            xWriter.WriteEndElement();  // </Config>
            xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
            xWriter.Flush();    // Flush the file buffer to disk
            xWriter.Close();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (active == false) return;
    
            updateFFXIVProcesses();
        }
        
        private void updateFFXIVProcesses()
        {
            var processes = new List<Process>();
            processes.AddRange(Process.GetProcessesByName("ffxiv"));
            processes.AddRange(Process.GetProcessesByName("ffxiv_dx11"));

            for (var i = 0; i < processes.Count; i++)
            {
                Process process = processes[i];
                try
                {
                    if (networks.ContainsKey(process.Id)) continue;
                    ProcessNet pn = new ProcessNet(process, new App.Network());
                    pn.network.onReceiveEvent += Network_onReceiveEvent;
                    networks.TryAdd(process.Id, pn);
                }
                catch (Exception e) {
                    Log.Ex(e, "error");
                }
            }

            List<int> toDelete = new List<int>();
            foreach(KeyValuePair<int, ProcessNet> entry in networks)
            {
                if (entry.Value.process.HasExited)
                {
                    entry.Value.network.StopCapture();
                    toDelete.Add(entry.Key);
                }
                else
                {
                    if (entry.Value.network.IsRunning)
                    {
                        entry.Value.network.UpdateGameConnections(entry.Value.process);
                    }
                    else
                    {
                        entry.Value.network.StartCapture(entry.Value.process);
                    }
                }
            }
            for (var i = 0; i < toDelete.Count; i++)
            {
                try
                {
                    ProcessNet pn;
                    networks.TryRemove(toDelete[i], out pn);
                    pn.network.onReceiveEvent -= Network_onReceiveEvent;
                }
                catch (Exception e) {
                    Log.Ex(e, "error");
                }
            }
            

        }

        public void DeInitPlugin()
        {
            active = false;
            Log.richTextBox1 = null;
            if (this.lblStatus != null)
            {
                this.lblStatus.Text = "FFXIV F.A.T.E Plugin Unloaded.";
                this.lblStatus = null;
            }
            
            foreach (KeyValuePair<int, ProcessNet> entry in networks)
            {
                entry.Value.network.StopCapture();
            }
            
            timer.Enabled = false;
            SaveSettings();
        }

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textTelegramToken = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textTelegramChatID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxTelegram = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.telegramFateTreeView = new System.Windows.Forms.TreeView();
            this.checkBoxTelegramDutyFinder = new System.Windows.Forms.CheckBox();
            this.checkBoxToastNotification = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "Language";
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(88, 14);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(121, 20);
            this.comboBoxLanguage.TabIndex = 6;
            this.comboBoxLanguage.SelectedIndexChanged += comboBoxLanguage_SelectedIndexChanged;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textTelegramToken);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textTelegramChatID);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.checkBoxTelegram);
            this.groupBox1.Location = new System.Drawing.Point(23, 49);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(533, 51);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Telegram";
            // 
            // textTelegramToken
            // 
            this.textTelegramToken.Location = new System.Drawing.Point(232, 20);
            this.textTelegramToken.Name = "textTelegramToken";
            this.textTelegramToken.Size = new System.Drawing.Size(291, 21);
            this.textTelegramToken.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(186, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "Token";
            // 
            // textTelegramChatID
            // 
            this.textTelegramChatID.Location = new System.Drawing.Point(67, 20);
            this.textTelegramChatID.Name = "textTelegramChatID";
            this.textTelegramChatID.Size = new System.Drawing.Size(100, 21);
            this.textTelegramChatID.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "Chat ID";
            // 
            // checkBoxTelegram
            // 
            this.checkBoxTelegram.AutoSize = true;
            this.checkBoxTelegram.Location = new System.Drawing.Point(67, 0);
            this.checkBoxTelegram.Name = "checkBoxTelegram";
            this.checkBoxTelegram.Size = new System.Drawing.Size(58, 16);
            this.checkBoxTelegram.TabIndex = 5;
            this.checkBoxTelegram.Text = "Active";
            this.checkBoxTelegram.UseVisualStyleBackColor = true;
            this.checkBoxTelegram.CheckedChanged += new System.EventHandler(this.checkBoxTelegram_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.telegramFateTreeView);
            this.groupBox2.Controls.Add(this.checkBoxTelegramDutyFinder);
            this.groupBox2.Location = new System.Drawing.Point(23, 115);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(533, 457);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Alert";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "F.A.T.E";
            // 
            // telegramFateTreeView
            // 
            this.telegramFateTreeView.CheckBoxes = true;
            this.telegramFateTreeView.Location = new System.Drawing.Point(15, 81);
            this.telegramFateTreeView.Name = "telegramFateTreeView";
            this.telegramFateTreeView.Size = new System.Drawing.Size(508, 370);
            this.telegramFateTreeView.TabIndex = 9;
            this.telegramFateTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.fateTreeView_AfterCheck);
            // 
            // checkBoxTelegramDutyFinder
            // 
            this.checkBoxTelegramDutyFinder.AutoSize = true;
            this.checkBoxTelegramDutyFinder.Checked = true;
            this.checkBoxTelegramDutyFinder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTelegramDutyFinder.Location = new System.Drawing.Point(15, 22);
            this.checkBoxTelegramDutyFinder.Name = "checkBoxTelegramDutyFinder";
            this.checkBoxTelegramDutyFinder.Size = new System.Drawing.Size(88, 16);
            this.checkBoxTelegramDutyFinder.TabIndex = 8;
            this.checkBoxTelegramDutyFinder.Text = "Duty Finder";
            this.checkBoxTelegramDutyFinder.UseVisualStyleBackColor = true;
            this.checkBoxTelegramDutyFinder.CheckedChanged += new System.EventHandler(this.checkBoxTelegramDutyFinder_CheckedChanged);
            // 
            // checkBoxToastNotification
            // 
            this.checkBoxToastNotification.AutoSize = true;
            this.checkBoxToastNotification.Location = new System.Drawing.Point(255, 18);
            this.checkBoxToastNotification.Name = "checkBoxToastNotification";
            this.checkBoxToastNotification.Size = new System.Drawing.Size(160, 16);
            this.checkBoxToastNotification.TabIndex = 11;
            this.checkBoxToastNotification.Text = "Active Toast Notification";
            this.checkBoxToastNotification.UseVisualStyleBackColor = true;
            this.checkBoxToastNotification.CheckedChanged += new System.EventHandler(this.checkBoxToastNotification_CheckedChanged);
            // 
            // ACTFate
            // 
            this.Controls.Add(this.checkBoxToastNotification);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxLanguage);
            this.Name = "ACTFate";
            this.Size = new System.Drawing.Size(1744, 592);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        private string getTextInstance(int code)
        {
            try
            {
                return data["instances"][code.ToString()]["name"][selLng].ToString();
            } catch(Exception e)
            {
                Log.Ex(e, "ignore");
            }
            return code.ToString();
        }
        private string getTextFate(int code)
        {
            try
            {
                var item = data["fates"][code.ToString()]["name"];
                item = item[selLng].ToString() == "" ? item["en"] : item[selLng];
                return item.ToString();
            }
            catch (Exception e)
            {
                Log.Ex(e, "ignore");
            }
            return code.ToString();
        }
        private string getTextFateArea(int code)
        {
            string areaCode = null;
            try
            {
                areaCode = data["fates"][code.ToString()]["area_code"].ToString();
                return data["areas"][areaCode][selLng].ToString();
            }
            catch (Exception e)
            {
                Log.Ex(e, "ignore");
            }
            return areaCode == null ? code.ToString() : areaCode;
        }
        private string getTextRoulette(int code)
        {
            try
            {
                if (code == 0) return "";
                return data["roulettes"][code.ToString()][selLng].ToString();
            }
            catch (Exception e)
            {
                Log.Ex(e, "ignore");
            }
            return code.ToString();
        }


        private void Network_onReceiveEvent(int pid, App.Network.EventType eventType, int[] args)
        {
            string server = (networks[pid].process.MainModule.FileName.Contains("KOREA") ? "KOREA" : "GLOBAL");
            string text = pid + "|" + server + "|" + eventType + "|";

            
            int pos = 0;
            switch (eventType)
            {
                case App.Network.EventType.INSTANCE_ENTER:
                case App.Network.EventType.INSTANCE_EXIT:
                    if (args.Length > 0)
                    {
                        text += getTextInstance(args[0]) + "|"; pos++;
                    }
                    break;
                case App.Network.EventType.FATE_BEGIN:
                case App.Network.EventType.FATE_PROGRESS:
                case App.Network.EventType.FATE_END:
                    text += getTextFate(args[0]) + "|" + getTextFateArea(args[0]) + "|";pos++;
                    break;
                case App.Network.EventType.MATCH_BEGIN:
                    text += (App.Network.MatchType)args[0] + "|"; pos++;
                    switch ((App.Network.MatchType)args[0])
                    {
                        case App.Network.MatchType.ROULETTE:
                            text += getTextRoulette(args[1]) + "|"; pos++;
                            break;
                        case App.Network.MatchType.SELECTIVE:
                            text += args[1] + "|"; pos++;
                            int p = pos;
                            for (int i = p; i < args.Length; i++)
                            {
                                text += getTextInstance(args[i]) + "|"; pos++;
                            }
                            break;
                    }
                    break;
                case App.Network.EventType.MATCH_END:
                    text += (App.Network.MatchEndType)args[0] + "|"; pos++;
                    break;
                case App.Network.EventType.MATCH_PROGRESS:
                    text += getTextInstance(args[0]) + "|"; pos++;
                    break;
                case App.Network.EventType.MATCH_ALERT:
                    text += getTextRoulette(args[0]) + "|"; pos++;
                    text += getTextInstance(args[1]) + "|"; pos++;
                    break;

            }

            for (int i = pos; i < args.Length; i++)
            {
                text += args[i] + "|";
            }

            sendToACT(text);

            postToToastWindowsNotificationIfNeeded(server, eventType, args);
            postToTelegramIfNeeded(server, eventType, args);
        }

        private void sendToACT(string text)
        {
            ActGlobals.oFormActMain.ParseRawLogLine(false, DateTime.Now, "00|" + DateTime.Now.ToString("O") + "|0048|F|" + text);
        }


        private class Language
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }
        private JObject data;
        private string selLng;

        private bool isTelegramEnable = false;
        private string telegramChkFates;
        private ConcurrentStack<string> telegramSelectedFates = new ConcurrentStack<string>();

        private void loadJSONData()
        {
            string jsonString = File.ReadAllText(fileInfo.Directory.FullName + "/data.json");
            var json = JObject.Parse(jsonString);

            List<Language> languages = new List<Language>();
            var l = json["languages"];
            foreach (var item in l)
            {
                string key = ((JProperty)item).Name;
                languages.Add(new Language { Name = l[key].ToString(), Code = key });
            }

            this.comboBoxLanguage.DataSource = languages.ToArray();
            comboBoxLanguage.DisplayMember = "Name";
            comboBoxLanguage.ValueMember = "Code";
            selLng = (string)comboBoxLanguage.SelectedValue;
                        
            data = json;  
            
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            selLng = (string)comboBoxLanguage.SelectedValue;
            loadFates();
        }
        
        private void loadFates()
        {
            this.telegramFateTreeView.Nodes.Clear();

            List<string> c = new List<string>();
            if (telegramChkFates != null && telegramChkFates != "")
            {
                string[] sp = telegramChkFates.Split(new char[] { '|' });
                for (int i = 0; i < sp.Length; i++)
                {
                    c.Add(sp[i]);
                }
            }

            lockTreeEvent = true;
            foreach (JProperty item in data["areas"])
            {
                string key = item.Name;
                System.Windows.Forms.TreeNode areaNode = this.telegramFateTreeView.Nodes.Add(data["areas"][key][selLng].ToString());
                areaNode.Tag = "AREA:" + key;
                if (c.Contains((string)areaNode.Tag)) areaNode.Checked = true;
                foreach (JProperty fate in data["fates"])
                {
                    if (data["fates"][fate.Name]["area_code"].ToString().Equals(key) == false) continue;
                    string text = data["fates"][fate.Name]["name"][selLng].ToString();
                    if (text == null || text == "") text = data["fates"][fate.Name]["name"]["en"].ToString();
                    System.Windows.Forms.TreeNode fateNode = areaNode.Nodes.Add(text);
                    fateNode.Tag = fate.Name;
                    if (c.Contains((string)fateNode.Tag)) fateNode.Checked = true;
                }
            }
            telegramSelectedFates.Clear();
            updateSelectedFates(telegramFateTreeView.Nodes);
            lockTreeEvent = false;

        }

        bool lockTreeEvent = false;
        private bool isTelegramDutyAlertEnable;
        private bool isToastNotificationEnable;

        private void fateTreeView_AfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (lockTreeEvent) return;
            lockTreeEvent = true;
            if (((string)e.Node.Tag).Contains("AREA:"))
            {
                foreach (System.Windows.Forms.TreeNode node in e.Node.Nodes)
                {
                    node.Checked = e.Node.Checked;
                }
            } 
            else
            {
                if (e.Node.Checked == false)
                {
                    e.Node.Parent.Checked = false;
                }
                else
                {
                    bool flag = true;
                    foreach (System.Windows.Forms.TreeNode node in e.Node.Parent.Nodes)
                    {
                        flag &= node.Checked;
                    }
                    e.Node.Parent.Checked = flag;
                }
            }
            telegramSelectedFates.Clear();
            updateSelectedFates(telegramFateTreeView.Nodes);


            lockTreeEvent = false;
        }

        private void updateSelectedFates(System.Windows.Forms.TreeNodeCollection nodes)
        {
            foreach (System.Windows.Forms.TreeNode node in nodes)
            {
                if (node.Checked) telegramSelectedFates.Push((string)node.Tag);
                updateSelectedFates(node.Nodes);
            }
        }

        private void postToTelegramIfNeeded(string server, App.Network.EventType eventType, int[] args)
        {
            if (eventType != App.Network.EventType.FATE_BEGIN && eventType != App.Network.EventType.MATCH_ALERT) return;
            if (isTelegramEnable == false) return;

            string head = networks.Count <= 1 ? "" : "[" + server + "] ";
            switch (eventType)
            {
                case App.Network.EventType.MATCH_ALERT: 
                    //text += getTextRoulette(args[0]) + "|"; pos++;
                    //text += getTextInstance(args[1]) + "|"; pos++;
                    if (isTelegramDutyAlertEnable)
                    {
                        postToTelegram(head + getTextRoulette(args[0]) + " >> " + getTextInstance(args[1]));
                    }
                    break;
                case App.Network.EventType.FATE_BEGIN:
                    //text += getTextFate(args[0]) + "|" + getTextFateArea(args[0]) + "|"; pos++;
                    if (telegramSelectedFates.Contains(args[0].ToString())) {
                        postToTelegram(head + getTextFateArea(args[0]) + " >> " + getTextFate(args[0]));
                    }
                    break;

            }
        }

        private void postToToastWindowsNotificationIfNeeded(string server, App.Network.EventType eventType, int[] args)
        {
            if (eventType != App.Network.EventType.FATE_BEGIN && eventType != App.Network.EventType.MATCH_ALERT) return;
            if (isToastNotificationEnable == false) return;

            string head = networks.Count <= 1 ? "" : "[" + server + "] ";
            switch (eventType)
            {
                case App.Network.EventType.MATCH_ALERT:
                    //text += getTextRoulette(args[0]) + "|"; pos++;
                    //text += getTextInstance(args[1]) + "|"; pos++;
                    if (isTelegramDutyAlertEnable)
                    {
                       toastWindowNotification(head + getTextRoulette(args[0]) + " >> " + getTextInstance(args[1]));
                    }
                    break;
                case App.Network.EventType.FATE_BEGIN:
                    //text += getTextFate(args[0]) + "|" + getTextFateArea(args[0]) + "|"; pos++;
                    if (telegramSelectedFates.Contains(args[0].ToString()))
                    {
                        toastWindowNotification(head + getTextFateArea(args[0]) + " >> " + getTextFate(args[0]));
                    }
                    break;

            }
        }

        private void postToTelegram(string message)
        {
            string chatID = textTelegramChatID.Text, token = textTelegramToken.Text;
            if (chatID == null || chatID == "" || token == null || token == "") return;

            using (WebClient client = new WebClient())
            {
                client.UploadValues("https://api.telegram.org/bot" + token + "/sendMessage", new NameValueCollection()
                {
                    { "chat_id", chatID },
                    { "text", message }
                });
            }
        }

        private void checkBoxTelegram_CheckedChanged(object sender, EventArgs e)
        {
            isTelegramEnable = checkBoxTelegram.Checked;
            textTelegramChatID.Enabled = !isTelegramEnable;
            textTelegramToken.Enabled = !isTelegramEnable;
        }

        private void checkBoxTelegramDutyFinder_CheckedChanged(object sender, EventArgs e)
        {
            isTelegramDutyAlertEnable = checkBoxTelegramDutyFinder.Checked;
        }


        private void toastWindowNotification(string text)
        {
            try
            {
                // Get a toast XML template
                Windows.Data.Xml.Dom.XmlDocument toastXml = Windows.UI.Notifications.ToastNotificationManager.GetTemplateContent(Windows.UI.Notifications.ToastTemplateType.ToastImageAndText03);

                // Fill in the text elements
                Windows.Data.Xml.Dom.XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
                for (int i = 0; i < stringElements.Length; i++)
                {
                    stringElements[i].AppendChild(toastXml.CreateTextNode(text));
                }

                // Specify the absolute path to an image
                String imagePath = "file:///" + Path.GetFullPath("toastImageAndText.png"); // 없으면 기본 아이콘 이미지로 알아서 뜸. ACT있는곳에 넣어야되는듯 dll옆이 아니라
                Windows.Data.Xml.Dom.XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
                imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                // Create the toast and attach event listeners
                Windows.UI.Notifications.ToastNotification toast = new Windows.UI.Notifications.ToastNotification(toastXml);

                // Show the toast. Be sure to specify the AppUserModelId on your application's shortcut!
                Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
            }
            catch (Exception e)
            {
                Log.Ex(e, "error");
            }
            
        }

        private void checkBoxToastNotification_CheckedChanged(object sender, EventArgs e)
        {
            isToastNotificationEnable = checkBoxToastNotification.Checked;
        }
    }

}
