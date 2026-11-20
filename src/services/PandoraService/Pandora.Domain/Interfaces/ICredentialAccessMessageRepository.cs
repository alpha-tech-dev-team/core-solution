using Pandora.Domain.Entities;

namespace Pandora.Domain.Interfaces;

public interface ICredentialAccessMessageRepository
{
    Task Add(CredentialAccessMessage message, CancellationToken cancellationToken);
    Task<IReadOnlyList<CredentialAccessMessage>> GetDue(
        DateTime now,
        int limit,
        CancellationToken cancellationToken);
    Task<int> DeleteCompleted(DateTime before, CancellationToken cancellationToken);
    Task MarkDeleted(Guid id, DateTime deletedAt, DateTime? notifiedAt, CancellationToken cancellationToken);
    Task MarkNotified(Guid id, DateTime notifiedAt, CancellationToken cancellationToken);
    Task MarkFailed(Guid id, string error, DateTime nextAttemptAt, CancellationToken cancellationToken);
}
