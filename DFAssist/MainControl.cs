using System.Drawing;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using FontStyle = System.Drawing.FontStyle;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace DFAssist
{
    public class MainControl : UserControl, IActPluginV1
    {
        private LinkLabel _copyrightLink;
        private TableLayoutPanel _mainTableLayout;
        private TableLayoutPanel _settingsTableLayout;
        private TabControl _appTabControl;
        private TabPage _mainTabPage;
        private TabPage _settingsPage;
        private Panel _settingsPanel;

        public GroupBox TestSettings;
        public GroupBox ToastSettings;
        public GroupBox TtsSettings;
        public GroupBox GeneralSettings;

        public Button ClearLogButton;
        public Label LanguageLabel;
        public Label AppTitle;
        public CheckBox TtsCheckBox;
        public CheckBox PersistToasts;
        public TextBox LanguageValue;
        public CheckBox DisableToasts;
        public CheckBox EnableLegacyToast;
        public CheckBox EnableTestEnvironment;
        public ComboBox LanguageComboBox;
        public RichTextBox LoggingRichTextBox;

        public MainControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            LanguageLabel = new Label();
            LanguageValue = new TextBox();
            LanguageComboBox = new ComboBox();
            EnableTestEnvironment = new CheckBox();
            TtsCheckBox = new CheckBox();
            PersistToasts = new CheckBox();
            EnableLegacyToast = new CheckBox();
            DisableToasts = new CheckBox();
            _appTabControl = new TabControl();
            _mainTabPage = new TabPage();
            _mainTableLayout = new TableLayoutPanel();
            ClearLogButton = new Button();
            LoggingRichTextBox = new RichTextBox();
            AppTitle = new Label();
            _copyrightLink = new LinkLabel();
            _settingsPage = new TabPage();
            _settingsPanel = new Panel();
            _settingsTableLayout = new TableLayoutPanel();
            TtsSettings = new GroupBox();
            ToastSettings = new GroupBox();
            GeneralSettings = new GroupBox();
            TestSettings = new GroupBox();
            _appTabControl.SuspendLayout();
            _mainTabPage.SuspendLayout();
            _mainTableLayout.SuspendLayout();
            _settingsPage.SuspendLayout();
            _settingsPanel.SuspendLayout();
            _settingsTableLayout.SuspendLayout();
            TtsSettings.SuspendLayout();
            ToastSettings.SuspendLayout();
            GeneralSettings.SuspendLayout();
            TestSettings.SuspendLayout();
            SuspendLayout();
            // 
            // LanguageLabel
            // 
            LanguageLabel.AutoSize = true;
            LanguageLabel.Location = new Point(3, 23);
            LanguageLabel.Name = "LanguageLabel";
            LanguageLabel.TabStop = false;
            LanguageLabel.Text = "Language";
            //
            // _languageValue
            //
            LanguageValue.Visible = false;
            LanguageValue.Name = "LanguageValue";
            LanguageValue.TabStop = false;
            // 
            // _languageComboBox
            // 
            LanguageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            LanguageComboBox.FormattingEnabled = true;
            LanguageComboBox.Location = new Point(80, 23);
            LanguageComboBox.Name = "LanguageComboBox";
            LanguageComboBox.Size = new Size(130, 25);
            LanguageComboBox.TabIndex = 0;
            // 
            // _disableToasts
            // 
            DisableToasts.AutoSize = true;
            DisableToasts.Location = new Point(6, 22);
            DisableToasts.Name = "DisableToasts";
            DisableToasts.TabIndex = 1;
            DisableToasts.Text = "Disable Toasts";
            DisableToasts.UseVisualStyleBackColor = true;
            // 
            // _persistToasts
            // 
            PersistToasts.AutoSize = true;
            PersistToasts.Location = new Point(6, 45);
            PersistToasts.Name = "PersistToasts";
            PersistToasts.TabIndex = 2;
            PersistToasts.Text = "Make Toasts Persistent";
            PersistToasts.UseVisualStyleBackColor = true;
            // 
            // _enableLegacyToast
            // 
            EnableLegacyToast.AutoSize = true;
            EnableLegacyToast.Location = new Point(6, 68);
            EnableLegacyToast.Name = "EnableLegacyToast";
            EnableLegacyToast.TabIndex = 3;
            EnableLegacyToast.Text = "Enable Legacy Toasts";
            EnableLegacyToast.UseVisualStyleBackColor = true;
            // 
            // _ttsCheckBox
            // 
            TtsCheckBox.AutoSize = true;
            TtsCheckBox.Location = new Point(6, 22);
            TtsCheckBox.Name = "TtsCheckBox";
            TtsCheckBox.TabIndex = 4;
            TtsCheckBox.Text = "Enable Text To Speech";
            TtsCheckBox.UseVisualStyleBackColor = true;
            // 
            // _enableTestEnvironment
            // 
            EnableTestEnvironment.AutoSize = true;
            EnableTestEnvironment.Location = new Point(6, 20);
            EnableTestEnvironment.Name = "EnableTestEnvironment";
            EnableTestEnvironment.TabIndex = 5;
            EnableTestEnvironment.Text = "Enable Test Environment";
            EnableTestEnvironment.UseVisualStyleBackColor = true;
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
            _mainTableLayout.Controls.Add(ClearLogButton, 2, 0);
            _mainTableLayout.Controls.Add(LoggingRichTextBox, 0, 1);
            _mainTableLayout.Controls.Add(AppTitle, 0, 0);
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
            ClearLogButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            ClearLogButton.Name = "ClearLogButton";
            ClearLogButton.MinimumSize = new Size(100, 25);
            ClearLogButton.TabIndex = 0;
            ClearLogButton.Text = "Clear Logs";
            ClearLogButton.UseVisualStyleBackColor = true;
            ClearLogButton.AutoSize = true;
            // 
            // _richTextBox1
            //
            LoggingRichTextBox.Dock = DockStyle.Fill;
            LoggingRichTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _mainTableLayout.SetColumnSpan(LoggingRichTextBox, 3);
            LoggingRichTextBox.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LoggingRichTextBox.Location = new Point(3, 32);
            LoggingRichTextBox.Name = "LoggingRichTextBox";
            LoggingRichTextBox.ReadOnly = true;
            LoggingRichTextBox.TabIndex = 1;
            LoggingRichTextBox.Text = "";
            // 
            // _appTitle
            // 
            AppTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
            AppTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            AppTitle.Location = new Point(3, 0);
            AppTitle.Name = "AppTitle";
            AppTitle.TabStop = false;
            AppTitle.Text = "DFAssist ~";
            AppTitle.TextAlign = ContentAlignment.MiddleLeft;
            AppTitle.AutoSize = true;
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
            _settingsTableLayout.Controls.Add(GeneralSettings, 0, 0);
            _settingsTableLayout.Controls.Add(ToastSettings, 0, 1);
            _settingsTableLayout.Controls.Add(TtsSettings, 0, 2);
            _settingsTableLayout.Controls.Add(TestSettings, 0, 3);
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
            GeneralSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            GeneralSettings.Controls.Add(LanguageLabel);
            GeneralSettings.Controls.Add(LanguageComboBox);
            GeneralSettings.Name = "GeneralSettings";
            GeneralSettings.TabStop = false;
            GeneralSettings.Text = "General Settings";
            // 
            // _toastSettings
            // 
            Dock = DockStyle.Top;
            ToastSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ToastSettings.Controls.Add(DisableToasts);
            ToastSettings.Controls.Add(EnableLegacyToast);
            ToastSettings.Controls.Add(PersistToasts);
            ToastSettings.Name = "ToastSettings";
            ToastSettings.TabStop = false;
            ToastSettings.Text = "Toasts Settings";
            // 
            // _ttsSettings
            // 
            Dock = DockStyle.Top;
            TtsSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TtsSettings.Controls.Add(TtsCheckBox);
            TtsSettings.Name = "TtsSettings";
            TtsSettings.TabStop = false;
            TtsSettings.Text = "Text To Speech Settings";
            // 
            // _testSettings
            // 
            Dock = DockStyle.Top;
            TestSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TestSettings.Controls.Add(EnableTestEnvironment);
            TestSettings.Name = "TestSettings";
            TestSettings.TabStop = false;
            TestSettings.Text = "Test Settings";
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
            TtsSettings.ResumeLayout(false);
            TtsSettings.PerformLayout();
            ToastSettings.ResumeLayout(false);
            ToastSettings.PerformLayout();
            GeneralSettings.ResumeLayout(false);
            GeneralSettings.PerformLayout();
            TestSettings.ResumeLayout(false);
            TestSettings.PerformLayout();
            ResumeLayout(false);
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            AssemblyResolver.Instance.Attach(this);
            DFAssistPlugin.Instance.InitPlugin(this);
        }

        public void DeInitPlugin()
        {
            DFAssistPlugin.Instance.DeInitPlugin();
        }
    }
}