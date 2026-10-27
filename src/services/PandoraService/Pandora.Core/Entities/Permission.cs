namespace Pandora.Core.Entities;

public sealed class Permission
{
    private const bool DefaultIsActive = true;

    public Guid UserId { get; set; }
    public string VaultItemId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = DefaultIsActive;
}
