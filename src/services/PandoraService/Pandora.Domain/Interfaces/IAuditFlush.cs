namespace Pandora.Domain.Interfaces;

public interface IAuditFlush
{
    Task Flush(CancellationToken cancellationToken);
}
