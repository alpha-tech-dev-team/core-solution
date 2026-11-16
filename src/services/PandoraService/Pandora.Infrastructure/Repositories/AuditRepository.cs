using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Infrastructure.Database;

namespace Pandora.Infrastructure.Repositories;

public sealed class AuditRepository : IAuditRepository
{
    private readonly PandoraDbContext _context;

    public AuditRepository(PandoraDbContext context)
    {
        _context = context;
    }

    public async Task Add(AuditEntry entry, CancellationToken cancellationToken)
    {
        _context.AuditEntry.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditEntry>> GetPage(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.AuditEntry.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var offset = (long)(page - PaginationLimits.FirstPage) * pageSize;

        if (offset >= total)
        {
            return new PagedResult<AuditEntry>([], page, pageSize, total);
        }

        var items = await query
            .OrderByDescending(entry => entry.Timestamp)
            .ThenBy(entry => entry.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditEntry>(items, page, pageSize, total);
    }
}
