namespace Pandora.Core.Interfaces;

public interface IAccessService
{
    Task<IReadOnlyList<string>> GetServices(long chatId, CancellationToken cancellationToken);
}
