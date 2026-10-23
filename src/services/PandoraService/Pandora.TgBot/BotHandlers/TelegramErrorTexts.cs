using Pandora.Core.Constants.Localization;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.TgBot.BotHandlers;

internal static class TelegramErrorTexts
{
    private static readonly IReadOnlyDictionary<ErrorCode, WorkflowStep> Sections =
        new Dictionary<ErrorCode, WorkflowStep>
        {
            [ErrorCode.AccessDenied] = WorkflowStep.ServiceSelection,
            [ErrorCode.SecretNotFound] = WorkflowStep.ServiceSelection
        };

    public static string Get(ILocalizer localizer, string? errorCode)
    {
        return Enum.TryParse<ErrorCode>(errorCode, out var code)
            ? localizer.GetPhrase(Sections.GetValueOrDefault(code, WorkflowStep.Idle), code.ToString())
            : localizer.GetPhrase(WorkflowStep.Idle, LocalizationKeysConstants.Idle.UnexpectedError);
    }
}
