using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Enums;
using CoreSolution.Shared.Models;

namespace Atlas.Domain.Models;

public sealed class AtlasUserSession : UserSession
{
    public Guid? IncidentCategoryId { get; set; }

    public AtlasUserSession(long tgUserId, string state, Language language, DateTime lastUpdate)
        : base(tgUserId, state, language, lastUpdate)
    {
    }

    public static AtlasUserSession Create(long tgUserId, string initialState)
    {
        return new AtlasUserSession(tgUserId, initialState, LocalizationConstants.DefaultLanguage, DateTime.UtcNow);
    }
}
