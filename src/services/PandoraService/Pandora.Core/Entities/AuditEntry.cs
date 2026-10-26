namespace Pandora.Core.Entities;

public sealed class AuditEntry
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? TargetItemId { get; set; }
    public string RequestSource { get; set; } = string.Empty;
}
