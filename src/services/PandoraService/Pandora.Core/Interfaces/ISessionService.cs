using Pandora.Core.Entities;
using Pandora.Core.Enums;

namespace Pandora.Core.Interfaces;

public interface ISessionService
{
    Task<UserSession> UpsertSession(long chatId, WorkflowStep initialState, CancellationToken cancellationToken);

    Task SaveSession(UserSession session, CancellationToken cancellationToken);

    Task DeleteSession(long telegramUserId, CancellationToken cancellationToken);
}
