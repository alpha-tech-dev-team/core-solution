using Building.Telegram.Abstractions;
using Building.Telegram.Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Building.Telegram.Internal;

internal sealed class TelegramNotificationService : ITelegramNotificationService
{
    private readonly ITelegramBotClient _client;
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(ITelegramBotClient client, ILogger<TelegramNotificationService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public Task<Message> SendMessageAsync(
        long chatId,
        string text,
        InlineKeyboardMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(chatId, text, true, replyMarkup, cancellationToken);
    }

    public Task<Message> SendAlertAsync(
        long chatId,
        string text,
        InlineKeyboardMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
    {
        return SendAsync(chatId, text, false, replyMarkup, cancellationToken);
    }

    public async Task<Message> EditMessageAsync(
        long chatId,
        int messageId,
        string text,
        InlineKeyboardMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
    {
        var clamped = text.ClampForTelegram();

        if (clamped.Length != text.Length)
        {
            _logger.LogWarning(
                "Текст для редактирования сообщения {MessageId} обрезан до {Length} символов",
                messageId,
                clamped.Length);
        }

        return await _client.EditMessageText(
            chatId: chatId,
            messageId: messageId,
            text: clamped,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteMessageAsync(
        long chatId,
        int messageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.DeleteMessage(chatId, messageId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось удалить сообщение {MessageId} в чате {ChatId}", messageId, chatId);
        }
    }

    private async Task<Message> SendAsync(
        long chatId,
        string text,
        bool silent,
        InlineKeyboardMarkup? replyMarkup,
        CancellationToken cancellationToken)
    {
        var chunks = TelegramTextSplitter.Split(text);
        Message? first = null;

        for (var index = 0; index < chunks.Count; index++)
        {
            var isLast = index == chunks.Count - 1;
            var message = await _client.SendMessage(
                chatId: chatId,
                text: chunks[index],
                replyMarkup: isLast ? replyMarkup : null,
                disableNotification: silent,
                cancellationToken: cancellationToken);

            if (first is null)
            {
                first = message;
            }
        }

        return first!;
    }
}
