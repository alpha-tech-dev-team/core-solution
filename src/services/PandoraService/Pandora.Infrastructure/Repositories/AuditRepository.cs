using Microsoft.EntityFrameworkCore;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
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

    public async Task<AuditEntryPage> GetPage(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.AuditEntry.AsNoTracking();

        var total = await query.CountAsync(cancellationToken);
        var offset = (long)(page - 1) * pageSize;

        if (offset >= total)
        {
            return new AuditEntryPage([], page, pageSize, total);
        }

        var items = await query
            .OrderByDescending(entry => entry.Timestamp)
            .ThenBy(entry => entry.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new AuditEntryPage(items, page, pageSize, total);
    }
}
