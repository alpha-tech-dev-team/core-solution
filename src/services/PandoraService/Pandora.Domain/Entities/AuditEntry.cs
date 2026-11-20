namespace Pandora.Domain.Entities;

public sealed class AuditEntry
{
    public Guid Id { get; }
    public DateTime Timestamp { get; }
    public Guid? UserId { get; }
    public long? TelegramUserId { get; }
    public string Action { get; }
    public string? TargetItemId { get; }
    public string RequestSource { get; }

    private AuditEntry(Guid id, DateTime timestamp, string action, string? targetItemId, string requestSource)
    {
        Id = id;
        Timestamp = timestamp;
        Action = action;
        TargetItemId = targetItemId;
        RequestSource = requestSource;
    }

    public AuditEntry(Guid id, DateTime timestamp, Guid userId, string action, string? targetItemId, string requestSource)
        : this(id, timestamp, action, targetItemId, requestSource)
    {
        UserId = userId;
    }

    public AuditEntry(Guid id, DateTime timestamp, long telegramUserId, string action, string? targetItemId, string requestSource)
        : this(id, timestamp, action, targetItemId, requestSource)
    {
        TelegramUserId = telegramUserId;
    }
}
