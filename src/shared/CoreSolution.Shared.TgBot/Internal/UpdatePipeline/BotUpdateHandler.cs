using CoreSolution.Shared.TgBot.Abstractions;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace CoreSolution.Shared.TgBot.Internal.UpdatePipeline;

internal sealed class BotUpdateHandler : IBotUpdateHandler
{
    private readonly IEnumerable<IBotHandler> _handlers;
    private readonly ILogger<BotUpdateHandler> _logger;

    public BotUpdateHandler(IEnumerable<IBotHandler> handlers, ILogger<BotUpdateHandler> logger)
    {
        _handlers = handlers;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        var handler = _handlers.FirstOrDefault(candidate => candidate.CanHandle(update));

        if (handler is null)
        {
            var (tgUserId, messageId) = UpdateContext.Extract(update);

            _logger.LogDebug("Обработчик для апдейта {UpdateId} не найден, пользователь {TgUserId}, сообщение {MessageId}", update.Id, tgUserId, messageId);
            return;
        }

        await handler.HandleAsync(update, cancellationToken);
    }
}
