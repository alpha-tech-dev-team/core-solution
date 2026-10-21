using Building.Telegram.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Building.Telegram.Extensions;

public static class TelegramNotificationExtensions
{
    public static async Task<Message> SendOrEditAsync(
        this ITelegramNotificationService telegram,
        long chatId,
        int? messageId,
        string text,
        InlineKeyboardMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
    {
        if (messageId is int existingMessageId)
        {
            try
            {
                return await telegram.EditMessageAsync(
                    chatId,
                    existingMessageId,
                    text,
                    replyMarkup,
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                await telegram.DeleteMessageAsync(chatId, existingMessageId, cancellationToken);
            }
        }

        return await telegram.SendMessageAsync(chatId, text, replyMarkup, cancellationToken);
    }

    public static async Task<Message> SendAndDeleteAsync(
        this ITelegramNotificationService telegram,
        long chatId,
        int? messageId,
        string text,
        InlineKeyboardMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
    {
        if (messageId is int existingMessageId)
        {
            await telegram.DeleteMessageAsync(chatId, existingMessageId, cancellationToken);
        }

        return await telegram.SendMessageAsync(chatId, text, replyMarkup, cancellationToken);
    }
}
