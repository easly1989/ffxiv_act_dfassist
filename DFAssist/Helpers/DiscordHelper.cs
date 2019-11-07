using DiscordWebhook;
using Splat;

namespace DFAssist.Helpers
{
    public class DiscordHelper : BaseNotificationHelper<DiscordHelper>
    {
        protected override void OnSendNotification(string title, string message, string testing)
        {
            if (!MainControl.DiscordCheckBox.Checked)
            {
                Logger.Write("UI: Discord Notifications are disabled!", LogLevel.Debug);
                return;
            }

            if(string.IsNullOrWhiteSpace(MainControl.DiscordUsernameTextBox.Text))
            {
                Logger.Write("UI: Specify a Username for the Discord settings", LogLevel.Warn);
                return;
            }

            if(string.IsNullOrWhiteSpace(MainControl.DiscordWebhookTextBox.Text))
            {
                Logger.Write("UI: Specify a valid Webhook URL for the Discord settings", LogLevel.Warn);
                return;
            }

            Logger.Write("UI: Sending Discord Notification...", LogLevel.Debug);

            var content = $"{title}\n>>>>> {message}";
            if(!string.IsNullOrWhiteSpace(testing))
                content += $" [{testing}]";

            var webhook = new Webhook(MainControl.DiscordWebhookTextBox.Text);
            var webHookObj = new WebhookObject
            {
                username = MainControl.DiscordUsernameTextBox.Text,
                content = content
            };

            webhook.PostData(webHookObj);

            Logger.Write("UI: Discord notification sent!", LogLevel.Debug);
        }
    }
}