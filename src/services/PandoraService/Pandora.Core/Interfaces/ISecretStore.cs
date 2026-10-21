using Pandora.Core.Models;

namespace Pandora.Core.Interfaces;

public interface ISecretStore
{
    Task<Secret?> GetSecretAsync(string secretName, CancellationToken cancellationToken);
}
