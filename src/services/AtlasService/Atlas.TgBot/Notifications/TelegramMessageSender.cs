using Atlas.Application.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Atlas.TgBot.Notifications;

public sealed class TelegramMessageSender : ITelegramMessageSender
{
    private readonly ITelegramBotClient _client;
    private readonly ILogger<TelegramMessageSender> _logger;

    public TelegramMessageSender(ITelegramBotClient client, ILogger<TelegramMessageSender> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task SendAsync(
        IReadOnlyCollection<long> chatIds,
        string text,
        CancellationToken cancellationToken = default)
    {
        var chunks = TelegramTextSplitter.Split(text);

        foreach (var chatId in chatIds)
        {
            try
            {
                await SendChunksAsync(chatId, chunks, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Не удалось отправить сообщение в чат {ChatId}", chatId);
            }
        }
    }

    private async Task SendChunksAsync(long chatId, IReadOnlyList<string> chunks, CancellationToken cancellationToken)
    {
        foreach (var chunk in chunks)
        {
            await _client.SendMessage(
                chatId,
                chunk,
                disableNotification: false,
                cancellationToken: cancellationToken);
        }
    }
}
