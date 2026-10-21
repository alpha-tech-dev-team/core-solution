namespace Pandora.Core.Interfaces;

public interface IAccessService
{
    Task<bool> HasAccessAsync(long chatId, CancellationToken cancellationToken);
}
