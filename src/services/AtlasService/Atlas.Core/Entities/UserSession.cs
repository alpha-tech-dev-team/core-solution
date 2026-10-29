using Atlas.Core.Enums;

namespace Atlas.Core.Entities;

public class UserSession
{
    public long TgUserId { get; set; }
    public WorkflowStep State { get; set; }
    public Language Language { get; set; }
    public DateTime LastUpdate { get; set; }

    public UserSession(long tgUserId, WorkflowStep state, Language language, DateTime lastUpdate)
    {
        TgUserId = tgUserId;
        State = state;
        Language = language;
        LastUpdate = lastUpdate;
    }
}
