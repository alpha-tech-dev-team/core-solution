using Building.Telegram.Models;
using Building.Telegram.Results;
using Telegram.Bot.Types.ReplyMarkups;

namespace Building.Telegram.Extensions;

public static class ResultExtensions
{
    public static string GetTextOrErrors(this Result<TelegramMessageContent> result)
    {
        if (result.IsSuccess)
        {
            return result.Value!.Text;
        }

        return string.Join(", ", result.ErrorsList);
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
