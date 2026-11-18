using CoreSolution.Shared.Results;

namespace Pandora.Domain.Interfaces;

public interface ISecretVaultReader
{
    Task<Result<IReadOnlyDictionary<string, string>>> GetPasswords(
        IReadOnlyCollection<string> itemIds,
        CancellationToken cancellationToken);
}
