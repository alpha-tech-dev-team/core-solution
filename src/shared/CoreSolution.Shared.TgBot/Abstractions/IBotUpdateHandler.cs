using Telegram.Bot.Types;

namespace CoreSolution.Shared.TgBot.Abstractions;

public interface IBotUpdateHandler
{
    Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
}
