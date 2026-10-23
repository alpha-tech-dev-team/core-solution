using Pandora.Core.Enums;

namespace Pandora.Core.Interfaces;

public interface ILocalizer
{
    string GetPhrase(WorkflowStep state, string key);

    string GetPhrase(WorkflowStep state, string key, Language language);
}
