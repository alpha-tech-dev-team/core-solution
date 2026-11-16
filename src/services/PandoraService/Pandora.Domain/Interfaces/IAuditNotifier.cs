using Pandora.Domain.Entities;

namespace Pandora.Domain.Interfaces;

public interface IAuditNotifier
{
    Task NotifyAsync(AuditEntry entry);
}
