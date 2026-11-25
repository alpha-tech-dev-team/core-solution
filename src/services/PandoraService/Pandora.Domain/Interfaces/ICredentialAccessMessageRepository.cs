using Pandora.Domain.Entities;
using Pandora.Domain.Models;

namespace Pandora.Domain.Interfaces;

public interface ICredentialAccessMessageRepository
{
    Task Add(CredentialAccessMessage message, CancellationToken cancellationToken);
    Task<IReadOnlyList<CredentialAccessMessage>> ClaimDue(CredentialMessageClaim claim, CancellationToken ct);
    Task<int> DeleteCompleted(DateTime before, CancellationToken cancellationToken);
    Task<int> DeleteExhausted(int maxAttempts, DateTime before, CancellationToken cancellationToken);
    Task MarkDeleted(Guid id, DateTime deletedAt, DateTime? notifiedAt, CancellationToken cancellationToken);
    Task MarkNotified(Guid id, DateTime notifiedAt, CancellationToken cancellationToken);
    Task<bool> MarkFailed(Guid id, CredentialMessageFailure failure, CancellationToken ct);
}
