using CoreSolution.Shared.Results;
using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface ISecretStore : ISecretCatalog
{
    Task<Result<Secret>> GetSecret(string itemId, CancellationToken cancellationToken);
}
