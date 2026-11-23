namespace Pandora.Domain.Interfaces;

public interface IAuditSaver
{
    Task SaveChanges(CancellationToken cancellationToken);
}
