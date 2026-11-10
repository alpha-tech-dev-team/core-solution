namespace Pandora.Domain.Entities;

public sealed class AuditEntry
{
    public Guid Id { get; }
    public DateTime Timestamp { get; }
    public long TelegramUserId { get; }
    public string Action { get; }
    public string? TargetItemId { get; }
    public string RequestSource { get; }

    public AuditEntry(Guid id, DateTime timestamp, long telegramUserId, string action, string? targetItemId, string requestSource)
    {
        Id = id;
        Timestamp = timestamp;
        TelegramUserId = telegramUserId;
        Action = action;
        TargetItemId = targetItemId;
        RequestSource = requestSource;
    }
}
