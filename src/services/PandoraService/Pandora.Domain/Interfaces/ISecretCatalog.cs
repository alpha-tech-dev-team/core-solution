using CoreSolution.Shared.Results;
using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface ISecretCatalog
{
    Task<Result<IReadOnlyList<SecretItem>>> GetItems(CancellationToken cancellationToken);
}
