using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;

namespace Atlas.Infrastructure.Sessions;

public sealed class AtlasSessionFactory : ISessionFactory
{
    public Type SessionType => typeof(AtlasUserSession);

    public UserSession Create(long telegramUserId, string state)
    {
        return AtlasUserSession.Create(telegramUserId, state);
    }
}
