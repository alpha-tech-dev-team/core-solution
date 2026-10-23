using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Building.Telegram.Abstractions;

public interface ITelegramNotificationService
{
    Task<Message> SendMessageAsync(
        long chatId,
        string text,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default);

    Task<Message> SendAlertAsync(
        long chatId,
        string text,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default);

    Task<Message> EditMessageAsync(
        long chatId,
        int messageId,
        string text,
        InlineKeyboardMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default);

    Task DeleteMessageAsync(
        long chatId,
        int messageId,
        CancellationToken cancellationToken = default);
}
