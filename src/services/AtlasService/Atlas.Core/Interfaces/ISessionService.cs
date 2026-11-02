using Atlas.Core.Entities;

namespace Atlas.Core.Interfaces;

public interface ISessionService
{
    Task<UserSession?> GetSession(long telegramUserId, CancellationToken cancellationToken);
    Task SaveSession(UserSession session, CancellationToken cancellationToken);
    Task DeleteSession(long telegramUserId, CancellationToken cancellationToken);
}
