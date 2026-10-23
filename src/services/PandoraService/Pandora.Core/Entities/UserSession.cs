using Pandora.Core.Enums;

namespace Pandora.Core.Entities;

public class UserSession
{
    public UserSession(long tgUserId, WorkflowStep state, Language language, List<string> roles, DateTime lastUpdate)
    {
        TgUserId = tgUserId;
        State = state;
        Language = language;
        Roles = roles;
        LastUpdate = lastUpdate;
    }

    public long TgUserId { get; set; }

    public WorkflowStep State { get; set; }

    public Language Language { get; set; }

    public List<string> Roles { get; set; }

    public DateTime LastUpdate { get; set; }
}
