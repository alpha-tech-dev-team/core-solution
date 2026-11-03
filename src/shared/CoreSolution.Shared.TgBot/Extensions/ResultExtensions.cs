using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoreSolution.Shared.TgBot.Extensions;

public static class ResultExtensions
{
    private const string UnknownErrorCode = "UnknownError";

    public static string GetDisplayText(this Result<WorkflowResponse> result)
    {
        if (result.IsSuccess)
        {
            return result.Value?.Text ?? string.Empty;
        }

        return result.ErrorCode ?? UnknownErrorCode;
    }

    public static ReplyMarkup? ToTelegramMarkup(this Result<WorkflowResponse> result)
    {
        return result.Value?.ButtonRows.ToInlineKeyboardMarkup();
    }

    public static ReplyMarkup? ToTelegramMarkup(this Result<WorkflowResponse> result, string contactButtonText)
    {
        if (!result.IsSuccess || result.Value is not { } content)
        {
            return null;
        }

        if (content.RequestContact)
        {
            return new ReplyKeyboardMarkup(new KeyboardButton(contactButtonText) { RequestContact = true })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }

        return result.ToTelegramMarkup();
    }
}
