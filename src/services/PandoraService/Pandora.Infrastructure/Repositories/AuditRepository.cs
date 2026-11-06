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
}
