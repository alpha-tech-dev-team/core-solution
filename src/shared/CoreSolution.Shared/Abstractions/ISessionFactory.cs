using CoreSolution.Shared.Models;

namespace CoreSolution.Shared.Abstractions;

public interface ISessionFactory
{
    Type SessionType { get; }

    UserSession Create(long telegramUserId, string state);
}
