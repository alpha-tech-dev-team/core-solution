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

    public static InlineKeyboardMarkup? ToTelegramMarkup(this Result<WorkflowResponse> result)
    {
        if (!result.IsSuccess)
        {
            return null;
        }

        return result.Value?.ButtonRows.ToInlineKeyboardMarkup();
    }
}
