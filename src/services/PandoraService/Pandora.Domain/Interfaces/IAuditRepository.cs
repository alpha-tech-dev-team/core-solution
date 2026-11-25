using CoreSolution.Shared.Models;
using Pandora.Domain.Entities;
using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface IAuditRepository
{
    Task Add(AuditEntry entry, CancellationToken cancellationToken);
    Task<PagedResult<AuditEntry>> GetPage(int page, int pageSize, AuditEntryFilter filter, CancellationToken cancellationToken);
}
