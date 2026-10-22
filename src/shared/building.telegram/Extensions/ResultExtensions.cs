using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using Telegram.Bot.Types.ReplyMarkups;

namespace Building.Telegram.Extensions;

public static class ResultExtensions
{
    public static InlineKeyboardMarkup? ToTelegramMarkup(this Result<TelegramMessageContent> result)
    {
        if (!result.IsSuccess)
        {
            return null;
        }

        return result.Value?.ButtonRows.ToInlineKeyboardMarkup();
    }
}
