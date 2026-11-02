using CoreSolution.Shared.Models;

namespace CoreSolution.Shared.Abstractions;

public interface ISessionService
{
    Task<UserSession?> GetSession(long telegramUserId, CancellationToken cancellationToken);
    Task SaveSession(UserSession session, CancellationToken cancellationToken);
    Task DeleteSession(long telegramUserId, CancellationToken cancellationToken);
}
