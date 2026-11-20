using Pandora.Domain.Interfaces;

namespace Pandora.Infrastructure.Database;

public sealed class AuditSaver : IAuditSaver
{
    private readonly PandoraDbContext _context;

    public AuditSaver(PandoraDbContext context)
    {
        _context = context;
    }

    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
