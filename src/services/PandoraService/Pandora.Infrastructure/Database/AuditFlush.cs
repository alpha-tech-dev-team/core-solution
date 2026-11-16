using Pandora.Domain.Interfaces;

namespace Pandora.Infrastructure.Database;

public sealed class AuditFlush : IAuditFlush
{
    private readonly PandoraDbContext _context;

    public AuditFlush(PandoraDbContext context)
    {
        _context = context;
    }

    public Task Flush(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
