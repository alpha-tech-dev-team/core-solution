using Pandora.Domain.Entities;

namespace Pandora.Domain.Interfaces;

public interface IAuditRepository
{
    Task Add(AuditEntry entry, CancellationToken cancellationToken);
}
