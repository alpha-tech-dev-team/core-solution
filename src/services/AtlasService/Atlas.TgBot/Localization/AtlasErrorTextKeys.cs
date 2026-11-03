using System.Collections.Frozen;
using CoreSolution.Shared.Constants;

namespace Atlas.TgBot.Localization;

public static class AtlasErrorTextKeys
{
    public static readonly (string State, string Key) Fallback = (LocalizationConstants.IdleStateName, LocalizationConstants.Idle.UnexpectedError);

    public static readonly FrozenDictionary<string, (string State, string Key)> ByErrorCode = new Dictionary<string, (string State, string Key)>
    {
        [ErrorCodes.AccessNotConfirmed] = (LocalizationConstants.IdleStateName, LocalizationConstants.Idle.AccessNotConfirmed)
    }.ToFrozenDictionary();
}
