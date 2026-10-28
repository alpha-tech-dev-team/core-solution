namespace Atlas.Core.Interfaces;

public interface IIdentityClient
{
    Task<bool> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken);
    Task<bool> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken);
}