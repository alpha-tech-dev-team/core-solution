using Pandora.Core.Entities;

namespace Pandora.Core.Interfaces;

public interface ISessionRepository
{
    Task SaveSession(UserSession session, TimeSpan expiry, CancellationToken cancellationToken);

    Task<UserSession?> GetSession(long chatId, CancellationToken cancellationToken);

    Task DeleteSession(long chatId, CancellationToken cancellationToken);

    Task AddToIndex(long chatId, DateTimeOffset activeAt, CancellationToken cancellationToken);

    Task RemoveFromIndex(long chatId, CancellationToken cancellationToken);
}
