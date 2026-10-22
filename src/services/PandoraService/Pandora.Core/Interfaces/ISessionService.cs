using Pandora.Core.Entities;

namespace Pandora.Core.Interfaces;

public interface ISessionService
{
    Task SaveSession(UserSession session, CancellationToken cancellationToken);

    Task DeleteSession(long telegramUserId, CancellationToken cancellationToken);
}
