using Building.Telegram.Abstractions;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Building.Telegram.Internal;

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
            _logger.LogDebug("Обработчик для апдейта {UpdateId} не найден", update.Id);
            return;
        }

        await handler.HandleAsync(update, cancellationToken);
    }
}
