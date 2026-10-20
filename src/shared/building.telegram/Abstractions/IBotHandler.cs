using Telegram.Bot.Types;

namespace Building.Telegram.Abstractions;

public interface IBotHandler
{
    bool CanHandle(Update update);

    Task HandleAsync(Update update, CancellationToken cancellationToken);
}
