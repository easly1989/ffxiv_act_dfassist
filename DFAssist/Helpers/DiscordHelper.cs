using System;
using Discord;
using Discord.Webhook;
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

            var userId = MainControl.DiscordUseridTextBox.Text;
            if(string.IsNullOrWhiteSpace(userId))
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

            var content = $">>>>> {message}";
            if(!string.IsNullOrWhiteSpace(testing))
                content += $" [{testing}]";

            using (var client = new DiscordWebhookClient(webhookUrl))
            {
                var embed = new EmbedBuilder()
                    .WithTitle(title)
                    .WithDescription(content)
                    .WithColor(new Color(0xA5DB17))
                    .WithTimestamp(DateTimeOffset.Now)
                    .WithFooter(footer => {
                        footer
                            .WithText("Advanced Combat Tracker")
                            .WithIconUrl("https://advancedcombattracker.com/act_data/act_banner1.png");
                    })
                    .Build();

                client.SendMessageAsync(username: "DFAssist", text: $"<@!{userId}>", embeds: new []{ embed })
                    .Wait();
            }

            Logger.Write("UI: Discord notification sent!", LogLevel.Debug);
        }
    }
}