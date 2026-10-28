namespace Pandora.Core.Entities;

public sealed class SecretMetadata
{
    public string VaultItemId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}
