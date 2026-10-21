namespace Pandora.Core.Interfaces;

public interface IAccessService
{
    Task<IReadOnlyList<string>?> GetServicesAsync(long chatId, CancellationToken cancellationToken);
}
