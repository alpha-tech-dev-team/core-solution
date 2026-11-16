using CoreSolution.Shared.Results;
using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface ISecretVaultReader
{
    Task<Result<IReadOnlyList<SecretPageItem>>> GetListItems(CancellationToken cancellationToken);
}
