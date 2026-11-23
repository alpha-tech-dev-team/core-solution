namespace Pandora.Domain.Entities;

public sealed class AuditEntry
{
    public Guid Id { get; }
    public DateTime Timestamp { get; }
    public Guid UserId { get; }
    public string Action { get; }
    public string? TargetItemId { get; }
    public string RequestSource { get; }
    public long? TargetTelegramUserId { get; }

    public AuditEntry(
        Guid id,
        DateTime timestamp,
        Guid userId,
        string action,
        string? targetItemId,
        string requestSource,
        long? targetTelegramUserId = null)
    {
        Id = id;
        Timestamp = timestamp;
        UserId = userId;
        Action = action;
        TargetItemId = targetItemId;
        RequestSource = requestSource;
        TargetTelegramUserId = targetTelegramUserId;
    }
}
