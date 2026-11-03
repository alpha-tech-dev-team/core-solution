using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.TgBot.Abstractions;

namespace Pandora.TgBot.Localization;

public sealed class PandoraUserErrorMapper : IUserErrorMapper
{
    private readonly ILocalizer _localizer;

    public PandoraUserErrorMapper(ILocalizer localizer)
    {
        _localizer = localizer;
    }

    public string Map(string errorCode)
    {
        var (state, key) = PandoraErrorTextKeys.ByErrorCode.GetValueOrDefault(errorCode, PandoraErrorTextKeys.Fallback);

        return _localizer.GetPhrase(state, key);
    }
}
