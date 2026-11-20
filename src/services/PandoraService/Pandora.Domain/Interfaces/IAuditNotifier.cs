using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface IAuditNotifier
{
    Task Notify(AuditEntryNotification notification);
}
