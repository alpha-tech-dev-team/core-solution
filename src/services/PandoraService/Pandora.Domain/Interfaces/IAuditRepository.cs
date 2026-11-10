using Pandora.Domain.Entities;
using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface IAuditRepository
{
    Task Add(AuditEntry entry, CancellationToken cancellationToken);
    Task<AuditEntryPage> GetPage(int page, int pageSize, CancellationToken cancellationToken);
}
