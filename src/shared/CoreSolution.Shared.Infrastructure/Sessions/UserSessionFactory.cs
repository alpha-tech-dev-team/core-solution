using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;

namespace CoreSolution.Shared.Infrastructure.Sessions;

public sealed class UserSessionFactory : ISessionFactory
{
    public Type SessionType => typeof(UserSession);

    public UserSession Create(long telegramUserId, string state)
    {
        return UserSession.Create(telegramUserId, state);
    }
}
