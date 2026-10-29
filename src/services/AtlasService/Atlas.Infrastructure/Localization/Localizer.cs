using Atlas.Core.Enums;
using Atlas.Core.Interfaces;

namespace Atlas.Infrastructure.Localization;

public class Localizer : ILocalizer
{
    private readonly LocalizationProvider _provider;

    public Localizer(LocalizationProvider provider)
    {
        _provider = provider;
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
