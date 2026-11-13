using Atlas.Domain.Localization;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.Abstractions;

namespace Atlas.TgBot.Localization;

public sealed class AtlasUserErrorMapper : IUserErrorMapper
{
    private readonly ILocalizer _localizer;

    public AtlasUserErrorMapper(ILocalizer localizer)
    {
        _localizer = localizer;
    }

    public string Map(string errorCode)
    {
        var (state, key) = AtlasErrorTextKeys.ByErrorCode.GetValueOrDefault(errorCode, AtlasErrorTextKeys.Fallback);

        return _localizer.GetPhrase(state, key);
    }
}
