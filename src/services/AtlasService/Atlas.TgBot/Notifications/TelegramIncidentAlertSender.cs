using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Atlas.TgBot.Notifications;

public sealed class TelegramIncidentAlertSender(ITelegramBotClient client) : IIncidentAlertSender
{
    public async Task SendAlertAsync(long chatId, string text, CancellationToken cancellationToken = default)
    {
        await client.SendMessage(
            chatId,
            text,
            disableNotification: false,
            cancellationToken: cancellationToken);
    }
}
