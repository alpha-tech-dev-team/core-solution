using Atlas.Application.Abstractions;
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
        foreach (var chatId in chatIds)
        {
            try
            {
                await _client.SendMessage(
                    chatId,
                    text,
                    disableNotification: false,
                    cancellationToken: cancellationToken);
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
}
