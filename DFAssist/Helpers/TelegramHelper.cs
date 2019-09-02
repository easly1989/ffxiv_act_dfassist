using System;
using Splat;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DFAssist.Helpers
{
    public class TelegramHelper : BaseNotificationHelper<TelegramHelper>
    {
        protected override void OnSendNotification(string title, string message, string testing)
        {
            if(!MainControl.TelegramCheckBox.Checked)
            {
                Logger.Write("UI: Telegram notifications are disabled", LogLevel.Debug);
                return;
            }

            if(string.IsNullOrWhiteSpace(MainControl.TelegramTokenTextBox.Text))
            {
                Logger.Write("UI: Telegram Token is missing", LogLevel.Error);
                return;
            }

            var chatIdValue = MainControl.TelegramChatIdTextBox.Text;
            if(string.IsNullOrWhiteSpace(chatIdValue))
            {
                Logger.Write("UI: Telegram Chat-Id is missing", LogLevel.Error);
                return;
            }

            try
            {
                var botClient = new TelegramBotClient(MainControl.TelegramTokenTextBox.Text);
                ChatId chatId;

                if (int.TryParse(chatIdValue, out var chatIdInt))
                    chatId = new ChatId(chatIdInt);
                else if (long.TryParse(chatIdValue, out var chatIdentifier))
                    chatId = new ChatId(chatIdentifier);
                else
                    chatId = new ChatId(chatIdValue);

                var result = botClient.SendTextMessageAsync(chatId, title + " - " + message).Result;
                Logger.Write($"UI: Telegram notification sent with message Id {result.MessageId}", LogLevel.Info);
            }
            catch(Exception ex)
            {
                Logger.Write(ex, "UI: Unable to send Telegram notification", LogLevel.Error);
            }
        }
    }
}