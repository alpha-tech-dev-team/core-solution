using CoreSolution.Shared.Models;
using Pandora.Domain.Entities;

namespace Pandora.Domain.Interfaces;

public interface IAuditRepository
{
    Task Add(AuditEntry entry, CancellationToken cancellationToken);
    Task<PagedResult<AuditEntry>> GetPage(int page, int pageSize, CancellationToken cancellationToken);
}
