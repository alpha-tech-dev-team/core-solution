using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Enums;

namespace CoreSolution.Shared.Infrastructure.Localization;

public sealed class Localizer : ILocalizer
{
    private const Language FallbackLanguage = LocalizationConstants.DefaultLanguage;
    private const string MissingPhraseFormat = "[{0}:{1}]";

    private readonly LocalizationProvider _provider;
    private readonly IRequestContext _requestContext;

    public Localizer(LocalizationProvider provider, IRequestContext requestContext)
    {
        _provider = provider;
        _requestContext = requestContext;
    }

    public string GetPhrase(string state, string key)
    {
        var session = _requestContext.Session;

        return GetPhrase(state, key, session is null ? FallbackLanguage : session.Language);
    }

    public string GetPhrase(string state, string key, Language language)
    {
        if (_provider.Dictionary.TryGetValue(language, out var states) &&
            states.TryGetValue(state, out var phrases) &&
            phrases.TryGetValue(key, out var phrase))
        {
            return phrase;
        }

        return string.Format(MissingPhraseFormat, state, key);
    }
}
