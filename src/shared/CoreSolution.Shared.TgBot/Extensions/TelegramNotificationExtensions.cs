using CoreSolution.Shared.TgBot.Abstractions;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoreSolution.Shared.TgBot.Extensions;

public static class TelegramNotificationExtensions
{
    private const string MessageNotModifiedFragment = "message is not modified";

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
            catch (ApiRequestException exception) when (IsMessageNotModified(exception))
            {
                return new Message
                {
                    Id = existingMessageId,
                    Chat = new Chat { Id = chatId },
                    Text = text,
                    ReplyMarkup = replyMarkup
                };
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

    private static bool IsMessageNotModified(ApiRequestException exception)
    {
        return exception.Message.Contains(MessageNotModifiedFragment, StringComparison.OrdinalIgnoreCase);
    }
}
