namespace Pandora.Domain.Interfaces;

public interface IAuditSaver
{
    Task SaveAsync(CancellationToken cancellationToken);
}
