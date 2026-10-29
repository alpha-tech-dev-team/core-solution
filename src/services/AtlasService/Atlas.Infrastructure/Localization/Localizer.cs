using Atlas.Core.Constants.Localization;
using Atlas.Core.Enums;
using Atlas.Core.Interfaces;

namespace Atlas.Infrastructure.Localization;

public class Localizer : ILocalizer
{
    private readonly LocalizationProvider _provider;
    private readonly IRequestContext _requestContext;

    public Localizer(LocalizationProvider provider, IRequestContext requestContext)
    {
        _provider = provider;
        _requestContext = requestContext;
    }

    public string GetPhrase(WorkflowStep state, string key)
    {
        var session = _requestContext.Session;

        return GetPhrase(state, key, session is null ? LocalizationConstants.DefaultLanguage : session.Language);
    }

    public string GetPhrase(WorkflowStep state, string key, Language language)
    {
        if (_provider.Dictionary.TryGetValue(language, out var states) &&
            states.TryGetValue(state, out var phrases) &&
            phrases.TryGetValue(key, out var phrase))
        {
            return phrase;
        }

        return $"[{state}:{key}]";
    }
}
