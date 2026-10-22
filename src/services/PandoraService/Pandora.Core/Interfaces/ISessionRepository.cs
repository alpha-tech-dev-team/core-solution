using Pandora.Core.Entities;

namespace Pandora.Core.Interfaces;

public interface ISessionRepository
{
    Task SaveSession(UserSession session, TimeSpan expiry, CancellationToken cancellationToken);

    Task<UserSession?> GetSession(long telegramUserId, CancellationToken cancellationToken);

    Task DeleteSession(long telegramUserId, CancellationToken cancellationToken);

    Task AddToIndex(long telegramUserId, DateTimeOffset activeAt, CancellationToken cancellationToken);
    Task RemoveFromIndex(long telegramUserId, CancellationToken cancellationToken);
}
