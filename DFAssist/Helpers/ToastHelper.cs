using System;
using System.Drawing;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using DFAssist.Core.Toast;
using DFAssist.Core.Toast.Base;
using Splat;

namespace DFAssist.Helpers
{
    public class ToastHelper : BaseNotificationHelper<ToastHelper>
    {
        public ToastHelper()
        {
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<ToastNotificationActivator>(DFAssistPlugin.AppId);
            DesktopNotificationManagerCompat.RegisterActivator<ToastNotificationActivator>();
        }

        protected override void OnSendNotification(string title, string message, string testing)
        {
            Logger.Write("UI: Request Showing Taost received...", LogLevel.Debug);
            if (MainControl.DisableToasts.Checked)
            {
                Logger.Write("UI: Toasts are disabled!", LogLevel.Debug);
                return;
            }

            if (MainControl.EnableActToast.Checked)
            {
                Logger.Write("UI: Using ACT Toasts", LogLevel.Debug);
                var traySlider = new TraySlider();
                traySlider.ShowDurationMs = 30000;
                traySlider.ButtonSE.Visible = false;
                traySlider.ButtonNE.Visible = false;
                traySlider.ButtonNW.Visible = false;
                traySlider.ButtonSW.Visible = true;
                traySlider.ButtonSW.Text = LocalizationRepository.GetText("ui-close-act-toast");
                traySlider.TrayTitle.Font = new Font(FontFamily.GenericSerif, 16, FontStyle.Bold);
                traySlider.TrayText.Font = new Font(FontFamily.GenericSerif, 12, FontStyle.Regular);
                if(!string.IsNullOrWhiteSpace(testing))
                    message += $"\nCode [{testing}]";

                traySlider.ShowTraySlider(message, title);
            }
            else
            {
                Logger.Write("UI: Using Windows Toasts", LogLevel.Debug);
                try
                {
                    // clearing old toasts if needed
                    DesktopNotificationManagerCompat.History.Clear();

                    Logger.Write("UI: Creating new Toast...", LogLevel.Debug);
                    ToastManager.ShowToast(title, message, testing);
                }
                catch (Exception e)
                {
                    Logger.Write(e, "UI: Using built in notifier...", LogLevel.Error);
                    var icon = new NotifyIcon
                    {
                        Icon = SystemIcons.WinLogo,
                        Text = "DFAssist",
                        Visible = true,
                        BalloonTipTitle = title,
                        BalloonTipText = message
                    };
                    icon.ShowBalloonTip(3000);
                    icon.Dispose();
                }
            }
        }
    }
}
