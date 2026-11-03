using CoreSolution.Shared.Models;

namespace CoreSolution.Shared.Abstractions;

public interface ISessionRepository
{
    Task SaveSession(UserSession session, TimeSpan expiry, CancellationToken cancellationToken);
    Task<UserSession?> GetSession(long telegramUserId, CancellationToken cancellationToken);
    Task DeleteSession(long telegramUserId, CancellationToken cancellationToken);
    Task AddToIndex(long telegramUserId, DateTimeOffset activeAt, CancellationToken cancellationToken);
    Task RemoveFromIndex(long telegramUserId, CancellationToken cancellationToken);
}
