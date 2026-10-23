using Building.Telegram.Extensions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using Pandora.Core.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;

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

    public static ReplyMarkup? GetReplyMarkup(this Result<TelegramMessageContent> result)
    {
        if (!result.IsSuccess || result.Value is null)
        {
            return null;
        }

        return result.Value.RequestContact
            ? result.Value.ToReplyMarkup()
            : result.ToTelegramMarkup();
    }
}
