using System;
using PushbulletSharp;
using PushbulletSharp.Models.Requests;
using Splat;

namespace DFAssist.Helpers
{
    public class PushBulletHelper : BaseNotificationHelper<PushBulletHelper>
    {
        protected override void OnSendNotification(string title, string message, string testing)
        {
            if (!MainControl.PushBulletCheckbox.Checked)
            {
                Logger.Write("UI: Pushbullet notifications are disabled", LogLevel.Debug);
                return;
            }

            if (string.IsNullOrWhiteSpace(MainControl.PushBulletTokenTextBox.Text))
            {
                Logger.Write("UI: Pushbullet Token is missing", LogLevel.Error);
                return;
            }

            try
            {
                var client = new PushbulletClient(MainControl.PushBulletTokenTextBox.Text);
                var request = new PushNoteRequest { Body = message, Title = title };
                if (!string.IsNullOrWhiteSpace(MainControl.PushBulletDeviceIdTextBox.Text)) 
                    request.DeviceIden = MainControl.PushBulletDeviceIdTextBox.Text;

                var response = client.PushNote(request);

                Logger.Write($"UI: Message pushed to Pushbullet with Id {response.ReceiverIden}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Write(ex, "UI: Unable to push Pushbullet notification", LogLevel.Error);
            }
        }
    }
}