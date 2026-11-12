using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface ISecretWriter
{
    Task<SecretItem?> Create(string name, string username, string password, CancellationToken cancellationToken);
    Task<SecretUpdateResult> Update(string itemId, SecretUpdateData data, CancellationToken cancellationToken);
    Task<SecretDeleteResult> Delete(string itemId, CancellationToken cancellationToken);
}
