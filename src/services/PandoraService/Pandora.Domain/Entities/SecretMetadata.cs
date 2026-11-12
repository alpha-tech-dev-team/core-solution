using Pandora.Domain.Enums;

namespace Pandora.Domain.Entities;

public sealed class SecretMetadata
{
    public string VaultItemId { get; }
    public string ResourceName { get; private set; }
    public string Username { get; private set; }
    public SecretSyncOperation SyncOperation { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime? NextAttemptAt { get; private set; }
    public string? LastError { get; private set; }

    public SecretMetadata(string vaultItemId, string resourceName, string username)
    {
        VaultItemId = vaultItemId;
        ResourceName = resourceName;
        Username = username;
    }

    public void Rename(string resourceName, string username)
    {
        ResourceName = resourceName;
        Username = username;
    }

    public void MarkPending(SecretSyncOperation operation)
    {
        SyncOperation = operation;
        AttemptCount = 0;
        NextAttemptAt = null;
        LastError = null;
    }
}
