using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using CoreSolution.Shared.TgBot.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoreSolution.Shared.TgBot.Extensions;

public static class ResultExtensions
{
    public static string GetUserText(this Result<WorkflowResponse> result, IUserErrorMapper errorMapper)
    {
        if (result.IsSuccess)
        {
            return result.Value?.Text ?? string.Empty;
        }

        return errorMapper.Map(result.ErrorCode ?? string.Empty);
    }

    public static ReplyMarkup? ToTelegramMarkup(this Result<WorkflowResponse> result)
    {
        if (!result.IsSuccess || result.Value is not { } content)
        {
            return null;
        }

        if (content.RequestContact)
        {
            return new ReplyKeyboardMarkup(new KeyboardButton(content.ContactButtonText!) { RequestContact = true })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }

        return content.ButtonRows.ToInlineKeyboardMarkup();
    }

    public static bool HasSensitiveData(this Result<WorkflowResponse> result)
    {
        return result.IsSuccess && result.Value is { ContainsSensitiveData: true };
    }

    public static TelegramSendOptions? ToTelegramSendOptions(this Result<WorkflowResponse> result)
    {
        if (!result.HasSensitiveData())
        {
            return null;
        }

        return new TelegramSendOptions(
            ProtectContent: true,
            Entities: result.Value!.TextMasks?
                .Select(mask => new MessageEntity
                {
                    Type = MessageEntityType.Spoiler,
                    Offset = mask.Offset,
                    Length = mask.Length
                })
                .ToArray());
    }
}
