using Pandora.Domain.Entities;

namespace Pandora.Domain.Interfaces;

public interface IAuditContext
{
    void Enqueue(AuditEntry entry);
    IReadOnlyList<AuditEntry> TakeAll();
}
