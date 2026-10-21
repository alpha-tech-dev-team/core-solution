namespace Identity.Core.Interfaces.Services;

public interface IIdentityService
{
    Task<bool> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken = default);
    Task<bool> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken = default);
}
