using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;

namespace Pandora.Infrastructure.Database;

public sealed class AuditContext : IAuditContext
{
    private readonly List<AuditEntry> _entries = new();

    public void Enqueue(AuditEntry entry)
    {
        _entries.Add(entry);
    }

    public IReadOnlyList<AuditEntry> TakeAll()
    {
        var entries = _entries.ToArray();
        _entries.Clear();

        return entries;
    }
}
