namespace Pandora.Domain.Entities;

public sealed class SecretMetadata
{
    public SecretMetadata(string vaultItemId, string resourceName, string username)
    {
        VaultItemId = vaultItemId;
        ResourceName = resourceName;
        Username = username;
    }

    public string VaultItemId { get; }
    public string ResourceName { get; private set; }
    public string Username { get; private set; }

    public void Rename(string resourceName, string username)
    {
        ResourceName = resourceName;
        Username = username;
    }
}
