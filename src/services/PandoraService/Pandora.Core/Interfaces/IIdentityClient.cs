namespace Pandora.Core.Interfaces;

public interface IIdentityClient
{
    Task<bool> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken);
}
