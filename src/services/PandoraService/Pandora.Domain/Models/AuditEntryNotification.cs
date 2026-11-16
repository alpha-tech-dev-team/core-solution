using Pandora.Domain.Entities;

namespace Pandora.Domain.Models;

public sealed record AuditEntryNotification(
    Guid Id,
    DateTime Timestamp,
    Guid? UserId,
    long? TelegramUserId,
    string Action,
    string? TargetItemId,
    string RequestSource)
{
    public static AuditEntryNotification From(AuditEntry entry)
    {
        return new AuditEntryNotification(
            entry.Id,
            entry.Timestamp,
            entry.UserId,
            entry.TelegramUserId,
            entry.Action,
            entry.TargetItemId,
            entry.RequestSource);
    }
}
