using Telegram.Bot.Types;

namespace CoreSolution.Shared.TgBot.Abstractions;

public interface IBotHandler
{
    bool CanHandle(Update update);
    Task HandleAsync(Update update, CancellationToken cancellationToken);
}
