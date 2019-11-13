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

            var username = MainControl.DiscordUsernameTextBox.Text;
            if(string.IsNullOrWhiteSpace(username))
            {
                Logger.Write("UI: Specify a Username for the Discord settings", LogLevel.Warn);
                return;
            }

            var webhookUrl = MainControl.DiscordWebhookTextBox.Text;
            if(string.IsNullOrWhiteSpace(webhookUrl))
            {
                Logger.Write("UI: Specify a valid Webhook URL for the Discord settings", LogLevel.Warn);
                return;
            }

            Logger.Write("UI: Sending Discord Notification...", LogLevel.Debug);

            var content = $"@{username} | {title}\n>>>>> {message}";
            if(!string.IsNullOrWhiteSpace(testing))
                content += $" [{testing}]";

            var webhook = new Webhook(webhookUrl);
            var webHookObj = new WebhookObject
            {
                username = username,
                content = content
            };

            webhook.PostData(webHookObj);

            Logger.Write("UI: Discord notification sent!", LogLevel.Debug);
        }
    }
}