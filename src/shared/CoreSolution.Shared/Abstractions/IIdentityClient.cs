namespace CoreSolution.Shared.Abstractions;

public interface IIdentityClient
{
    Task<string?> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken);
    Task<string?> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken);
}
