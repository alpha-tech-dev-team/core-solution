using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using Pandora.TgBot.Constants;

namespace Pandora.TgBot.BotHandlers;

internal static class TelegramResultExtensions
{
    public static string GetDisplayText(this Result<TelegramMessageContent> result)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return result.Value.Text;
        }

        return TelegramErrorMessages.Get(result.ErrorCode);
    }
}
