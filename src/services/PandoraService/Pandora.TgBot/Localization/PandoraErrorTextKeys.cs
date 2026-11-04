using System.Collections.Frozen;
using CoreSolution.Shared.Constants;
using Pandora.Domain.Constants.Localization;
using Pandora.Domain.Enums;

namespace Pandora.TgBot.Localization;

public static class PandoraErrorTextKeys
{
    public static readonly (string State, string Key) Fallback = (LocalizationConstants.IdleStateName, LocalizationKeysConstants.Idle.UnexpectedError);

    public static readonly FrozenDictionary<string, (string State, string Key)> ByErrorCode = new Dictionary<string, (string State, string Key)>
    {
        [nameof(ErrorCode.AccessDenied)] = (nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.AccessDenied),
        [ErrorCodes.AccessNotConfirmed] = (LocalizationConstants.IdleStateName, LocalizationKeysConstants.Idle.AccessNotConfirmed),
        [nameof(ErrorCode.SecretNotFound)] = (nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.SecretNotFound),
        [nameof(ErrorCode.VaultwardenUnavailable)] = (nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.VaultwardenUnavailable)
    }.ToFrozenDictionary();
}
