using Pandora.Domain.Entities;

namespace Pandora.Domain.Interfaces;

public interface ICredentialAccessMessageRepository
{
    Task Add(CredentialAccessMessage message, CancellationToken cancellationToken);
    Task<IReadOnlyList<CredentialAccessMessage>> ClaimDue(
        DateTime now,
        DateTime claimUntil,
        int limit,
        int maxAttempts,
        CancellationToken cancellationToken);
    Task<int> DeleteCompleted(DateTime before, CancellationToken cancellationToken);
    Task<int> DeleteExhausted(int maxAttempts, DateTime before, CancellationToken cancellationToken);
    Task MarkDeleted(Guid id, DateTime deletedAt, DateTime? notifiedAt, CancellationToken cancellationToken);
    Task MarkNotified(Guid id, DateTime notifiedAt, CancellationToken cancellationToken);
    Task<bool> MarkFailed(
        Guid id,
        int expectedAttempts,
        string error,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken);
}
