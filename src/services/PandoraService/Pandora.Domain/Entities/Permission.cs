namespace Pandora.Domain.Entities;

public sealed class Permission
{
    public long TelegramUserId { get; set; }
    public string VaultItemId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
