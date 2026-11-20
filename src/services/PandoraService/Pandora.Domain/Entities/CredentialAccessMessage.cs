namespace Pandora.Domain.Entities;

public sealed class CredentialAccessMessage
{
    public Guid Id { get; set; }
    public long ChatId { get; set; }
    public int MessageId { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime NextAttemptAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? NotifiedAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
