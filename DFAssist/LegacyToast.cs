using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace DFAssist
{
    public class LegacyToast : Form
    {
        private readonly FormAnimator _animator;
        private readonly string _message;
        private readonly ConcurrentDictionary<int, ProcessNet> _networks;
        private readonly string _title;
        private IContainer components;
        private Label _label1;
        private Label _label2;
        private Timer _timer1;

        public LegacyToast(string title, string message, ConcurrentDictionary<int, ProcessNet> networks)
        {
            InitializeComponent();
            Disposed += OnDisposed;
            _title = title;
            _message = message;
            _networks = networks;
            _animator = new FormAnimator(this, FormAnimator.AnimationMethod.Slide, FormAnimator.AnimationDirection.Left, 500);
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            _timer1.Interval = 10000;
        }

        private void OnDisposed(object sender, EventArgs eventArgs)
        {
            Disposed -= OnDisposed;
            _timer1.Stop();
        }

        private void PlaceLowerRight()
        {
            var screen = Screen.PrimaryScreen;
            Rectangle workingArea;
            if (!_networks.Any())
                foreach (var allScreen in Screen.AllScreens)
                {
                    workingArea = allScreen.WorkingArea;
                    var right1 = workingArea.Right;
                    workingArea = screen.WorkingArea;
                    var right2 = workingArea.Right;
                    if (right1 > right2)
                        screen = allScreen;
                }
            else
                screen = Screen.FromHandle(_networks.Values.First().Process.MainWindowHandle);

            workingArea = screen.WorkingArea;
            Left = workingArea.Right - Width;
            workingArea = screen.WorkingArea;
            Top = workingArea.Bottom - (Height + 50);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Toast_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Toast_Load(object sender, EventArgs e)
        {
            PlaceLowerRight();
            BackColor = DefaultForeColor;
            ForeColor = DefaultBackColor;
            _label1.Font = new Font("Serif", 18f, FontStyle.Bold);
            _label1.ForeColor = Color.White;
            _label2.Font = new Font("Serif", 16f);
            _label2.ForeColor = Color.Gray;
            _label1.Text = _title;
            _label2.Text = _message;
            SystemSounds.Exclamation.Play();
            _timer1.Start();
        }

        private void Toast_Activated(object sender, EventArgs e)
        {
        }

        private void Toast_Shown(object sender, EventArgs e)
        {
            _animator.Duration = 0;
            _animator.Direction = FormAnimator.AnimationDirection.Right;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            _label1 = new Label();
            _label2 = new Label();
            _timer1 = new Timer(components);
            SuspendLayout();
            _label1.AutoSize = true;
            _label1.Location = new Point(13, 13);
            _label1.Name = "_label1";
            _label1.Size = new Size(35, 13);
            _label1.TabIndex = 0;
            _label1.Text = "label1";
            _label1.Click += label1_Click;
            _label2.AutoSize = true;
            _label2.Location = new Point(16, 46);
            _label2.Name = "_label2";
            _label2.Size = new Size(35, 13);
            _label2.TabIndex = 1;
            _label2.Text = "label2";
            _label2.Click += label2_Click;
            _timer1.Tick += timer1_Tick;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(412, 103);
            Controls.Add(_label2);
            Controls.Add(_label1);
            Name = nameof(LegacyToast);
            Text = "Form1";
            Activated += Toast_Activated;
            Load += Toast_Load;
            Shown += Toast_Shown;
            Click += Toast_Click;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}