using CoreSolution.Shared.Enums;

namespace CoreSolution.Shared.Abstractions;

public interface ILocalizer
{
    string GetPhrase(string state, string key);
    string GetPhrase(string state, string key, Language language);
}
