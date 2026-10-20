using Telegram.Bot.Types;

namespace Building.Telegram.Abstractions;

public interface IBotUpdateHandler
{
    Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
}
