using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using Pandora.Core.Interfaces;

namespace Pandora.TgBot.BotHandlers;

internal static class TelegramResultExtensions
{
    public static string GetDisplayText(this Result<TelegramMessageContent> result, ILocalizer localizer)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return result.Value.Text;
        }

        return TelegramErrorTexts.Get(localizer, result.ErrorCode);
    }
}
