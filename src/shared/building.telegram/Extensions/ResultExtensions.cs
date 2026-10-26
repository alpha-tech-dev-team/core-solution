using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using Telegram.Bot.Types.ReplyMarkups;

namespace Building.Telegram.Extensions;

public static class ResultExtensions
{
    public static string GetDisplayText(this Result<TelegramMessageContent> result)
    {
        if (result.IsSuccess)
        {
            return result.Value?.Text ?? string.Empty;
        }

        return result.ErrorCode ?? "UnknownError";
    }
    public static InlineKeyboardMarkup? ToTelegramMarkup(this Result<TelegramMessageContent> result)
    {
        if (!result.IsSuccess)
        {
            return null;
        }

        return result.Value?.ButtonRows.ToInlineKeyboardMarkup();
    }
}
