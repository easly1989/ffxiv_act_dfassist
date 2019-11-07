using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using DFAssist.Core.Toast;
using Splat;

namespace DFAssist.Helpers
{
    public class ToastHelper : BaseNotificationHelper<ToastHelper>
    {
        private WinToastWrapper.ToastEventCallback _toastEventCallback;

        public ToastHelper()
        {
            // we need to force the dll folder for the DFAssist.WinToast c++ library
            // should be necessary just once; 
            // we can also avoid any check, because at this point all the libraries should be already loaded
            // and all the check should have been done in the AssemblyResolver
            // ReSharper disable AssignNullToNotNullAttribute
            var lpPathName = Path.Combine(Path.GetDirectoryName(Locator.Current.GetService<ActPluginData>().pluginFile.ToString()), "libs");
            if(WinToastWrapper.SetDllDirectory(lpPathName))
                Logger.Write($"UI: Toast library path: {lpPathName}", LogLevel.Debug);
            // ReSharper restore AssignNullToNotNullAttribute

            _toastEventCallback = delegate (int code)
            {
                if(code == 0)
                    Logger.Write("UI: Toast Clicked", LogLevel.Debug);
                else if(code == 1)
                    Logger.Write("UI: Toast Dismissed", LogLevel.Debug);
                else if(code == 2)
                    Logger.Write("UI: Toast Timed out", LogLevel.Debug);
                else if(code == 3)
                    Logger.Write("UI: Toast Hidden by application", LogLevel.Debug);
                else if(code == 4)
                    Logger.Write("UI: Toast was not activated", LogLevel.Warn);
                else if(code == 11)
                    Logger.Write("UI: Toast showing, waiting for interaction...", LogLevel.Debug);
                else if(code > 4 && code < 11)
                    Logger.Write($"UI: An Error occurred, code:[{code}]", LogLevel.Error);
                else
                    Logger.Write($"UI: Interacted with the toast, using a button, code:[{code}]", LogLevel.Debug);
            };
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
                var traySlider = new TraySlider
                {
                    Font = new Font(FontFamily.GenericSerif, 16, FontStyle.Bold),
                    ShowDurationMs = 30000
                };
                traySlider.ButtonSE.Visible = false;
                traySlider.ButtonNE.Visible = false;
                traySlider.ButtonNW.Visible = false;
                traySlider.ButtonSW.Visible = true;
                traySlider.ButtonSW.Text = LocalizationRepository.GetText("ui-close-act-toast");
                traySlider.ShowTraySlider($"{message}\n{testing}", title);
            }
            else
            {
                Logger.Write("UI: Using Windows Toasts", LogLevel.Debug);
                try
                {
                    Logger.Write("UI: Creating new Toast...", LogLevel.Debug);
                    var attribution = nameof(DFAssist);

                    var icon = new NotifyIcon
                    {
                        Icon = SystemIcons.WinLogo,
                        Visible = true,
                        BalloonTipTitle = title,
                        BalloonTipText = message
                    };
                    icon.ShowBalloonTip(3000);
                    icon.Dispose();

                    if (string.IsNullOrWhiteSpace(testing))
                    {
                        WinToastWrapper.CreateToast(
                            DFAssistPlugin.AppId,
                            DFAssistPlugin.AppId,
                            title,
                            message,
                            _toastEventCallback,
                            attribution,
                            true,
                            Duration.Long);
                    }
                    else
                    {
                        WinToastWrapper.CreateToast(
                            DFAssistPlugin.AppId,
                            DFAssistPlugin.AppId,
                            title,
                            message,
                            $"Code [{testing}]",
                            _toastEventCallback,
                            attribution,
                            Duration.Long);
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(e, "UI: Unable to use DFAssist.WinToast, using built in notifier...", LogLevel.Error);
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

        protected override void OnSetNullOwnedObjects()
        {
            _toastEventCallback = null;

            base.OnSetNullOwnedObjects();
        }
    }
}
