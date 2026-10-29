using Atlas.Core.Enums;

namespace Atlas.Core.Interfaces;

public interface ILocalizer
{
    string GetPhrase(WorkflowStep state, string key, Language language);
}
