using System;
using System.Drawing;
using Overlay.NET.Common;
using Overlay.NET.Directx;
using Process.NET.Windows;

namespace DFAssist.DirectX
{
    [RegisterPlugin("DirectXToastNotifier", "Carlo Ruggiero", "DirectXToastNotifier", "1.0", "A Toast notify for DFAssist that works with fullscreen game")]
    public class DirectxOverlayPlugin : DirectXOverlayPlugin
    {
        private readonly string _title;
        private readonly string _message;

        private readonly TickEngine _tickEngine = new TickEngine();
        public readonly ISettings<OverlaySettings> Settings = new SerializableSettings<OverlaySettings>();
        private int _redBrush;
        private int _whiteBrush;
        private int _blackBrush;
        private int _titleFont;
        private int _messageFont;

        public DirectxOverlayPlugin(string title, string message)
        {
            _title = title;
            _message = message;
        }

        public override void Initialize(IWindow targetWindow)
        {
            // Set target window by calling the base method
            base.Initialize(targetWindow);

            // For demo, show how to use settings
            var current = Settings.Current;
            var type = GetType();

            if (current.UpdateRate == 0)
                current.UpdateRate = 1000 / 60;

            current.Author = GetAuthor(type);
            current.Description = GetDescription(type);
            current.Identifier = GetIdentifier(type);
            current.Name = GetName(type);
            current.Version = GetVersion(type);

            // File is made from above info
            Settings.Save();
            Settings.Load();
            
            OverlayWindow = new DirectXOverlayWindow(targetWindow.Handle, false);

            _redBrush = OverlayWindow.Graphics.CreateBrush(0x7FFF0000);
            _whiteBrush = OverlayWindow.Graphics.CreateBrush(Color.White);
            _blackBrush = OverlayWindow.Graphics.CreateBrush(Color.FromArgb(76, 0, 0, 0));
            _titleFont = OverlayWindow.Graphics.CreateFont("Arial", 22, true);
            _messageFont = OverlayWindow.Graphics.CreateFont("Arial", 18);

            _tickEngine.PreTick += OnPreTick;
            _tickEngine.Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!OverlayWindow.IsVisible)
            {
                return;
            }

            OverlayWindow.Update();
            InternalRender();
        }

        private void OnPreTick(object sender, EventArgs e)
        {
            var targetWindowIsActivated = TargetWindow.IsActivated;
            if (!targetWindowIsActivated && OverlayWindow.IsVisible)
            {
                ClearScreen();
                OverlayWindow.Hide();
            }
            else if (targetWindowIsActivated && !OverlayWindow.IsVisible)
            {
                OverlayWindow.Show();
            }
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void Enable()
        {
            _tickEngine.Interval = Settings.Current.UpdateRate.Milliseconds();
            _tickEngine.IsTicking = true;
            base.Enable();
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void Disable()
        {
            _tickEngine.IsTicking = false;
            base.Disable();
        }

        public override void Update() => _tickEngine.Pulse();

        protected void InternalRender()
        {
            OverlayWindow.Graphics.BeginScene();
            OverlayWindow.Graphics.ClearScene();

            OverlayWindow.Graphics.FillRectangle(950, 250, 150, 200, _blackBrush);
            OverlayWindow.Graphics.DrawText(_title, _titleFont, _redBrush, 950, 250);
            OverlayWindow.Graphics.DrawText(_message, _messageFont, _whiteBrush, 950, 300);

            OverlayWindow.Graphics.EndScene();
        }

        public override void Dispose()
        {
            OverlayWindow.Dispose();
            base.Dispose();
        }

        public void ClearScreen()
        {
            OverlayWindow.Graphics.BeginScene();
            OverlayWindow.Graphics.ClearScene();
            OverlayWindow.Graphics.EndScene();
        }
    }
}