using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Enums;

namespace CoreSolution.Shared.Models;

public class UserSession
{
    public long TgUserId { get; set; }
    public string State { get; set; }
    public Language Language { get; set; }
    public DateTime LastUpdate { get; set; }

    public UserSession(long tgUserId, string state, Language language, DateTime lastUpdate)
    {
        TgUserId = tgUserId;
        State = state;
        Language = language;
        LastUpdate = lastUpdate;
    }

    public static UserSession Create(long tgUserId, string initialStep)
    {
        return new UserSession(tgUserId, initialStep, LocalizationConstants.DefaultLanguage, DateTime.UtcNow);
    }
}
