using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using DFAssist.Core.Toast.Base;
using Splat;

namespace DFAssist.Core.Toast
{
    public class ToastManager
    {
        public static void ShowToast(string title, string message, string testing = null)
        {
            var logger = Locator.Current.GetService<ILogger>();
            try
            {
                if (!string.IsNullOrWhiteSpace(testing))
                    testing = $"\nCode [{testing}]";

                var toastXmlString =
                    $@"<toast>
                <visual>
                <binding template='ToastGeneric'>
                <text>{title}</text>
                <text>{message}</text>
                <text>{testing}</text>
                <text placement='attribution'>DFAssist</text>
                </binding>
                </visual>
                </toast>"
                        .Replace("\r\n", string.Empty)
                        .Replace("\t", string.Empty);

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(toastXmlString);
                var toast = new ToastNotification(xmlDoc);

                var toastNotifier = DesktopNotificationManagerCompat.CreateToastNotifier();
                toastNotifier.Show(toast);
                logger.Write("UI: Toast Showing!", LogLevel.Debug);
            }
            catch (Exception e)
            {
                logger.Write(e, "UI: Unable to show the toast...", LogLevel.Error);
                throw;
            }
        }
    }
}
