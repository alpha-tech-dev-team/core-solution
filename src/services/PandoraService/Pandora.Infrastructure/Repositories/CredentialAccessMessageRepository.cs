using Microsoft.EntityFrameworkCore;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Infrastructure.Database;

namespace Pandora.Infrastructure.Repositories;

public sealed class CredentialAccessMessageRepository : ICredentialAccessMessageRepository
{
    private readonly PandoraDbContext _context;

    public CredentialAccessMessageRepository(PandoraDbContext context)
    {
        _context = context;
    }

    public async Task Add(CredentialAccessMessage message, CancellationToken cancellationToken)
    {
        _context.CredentialAccessMessage.Add(message);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CredentialAccessMessage>> ClaimDue(
        DateTime now,
        DateTime claimUntil,
        int limit,
        int maxAttempts,
        CancellationToken cancellationToken)
    {
        var candidates = await _context.CredentialAccessMessage
                .AsNoTracking()
                .Where(message => message.Attempts < maxAttempts
                    && message.NextAttemptAt <= now
                    && (message.ClaimedUntil == null || message.ClaimedUntil <= now)
                    && (message.DeletedAt == null || message.NotifiedAt == null))
            .OrderBy(message => message.NextAttemptAt)
            .ThenBy(message => message.Id)
            .Select(message => message.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var claimedIds = new List<Guid>(candidates.Count);

        foreach (var id in candidates)
        {
            var claimed = await _context.CredentialAccessMessage
                .Where(message => message.Id == id
                    && message.Attempts < maxAttempts
                    && message.NextAttemptAt <= now
                    && (message.ClaimedUntil == null || message.ClaimedUntil <= now)
                    && (message.DeletedAt == null || message.NotifiedAt == null))
                .ExecuteUpdateAsync(update => update
                    .SetProperty(message => message.ClaimedUntil, claimUntil),
                    cancellationToken);

            if (claimed > 0)
            {
                claimedIds.Add(id);
            }
        }

        return await _context.CredentialAccessMessage
            .AsNoTracking()
            .Where(message => claimedIds.Contains(message.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteCompleted(DateTime before, CancellationToken cancellationToken)
    {
        return await _context.CredentialAccessMessage
            .Where(message => message.DeletedAt != null
                && message.NotifiedAt != null
                && message.DeletedAt < before)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<int> DeleteExhausted(int maxAttempts, DateTime before, CancellationToken cancellationToken)
    {
        return await _context.CredentialAccessMessage
            .Where(message => message.Attempts >= maxAttempts
                && message.NextAttemptAt < before
                && (message.ClaimedUntil == null || message.ClaimedUntil < before))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task MarkDeleted(
        Guid id,
        DateTime deletedAt,
        DateTime? notifiedAt,
        CancellationToken cancellationToken)
    {
        if (notifiedAt.HasValue)
        {
            await _context.CredentialAccessMessage
                .Where(message => message.Id == id)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(message => message.DeletedAt, deletedAt)
                    .SetProperty(message => message.NotifiedAt, notifiedAt)
                    .SetProperty(message => message.LastError, (string?)null)
                    .SetProperty(message => message.ClaimedUntil, (DateTime?)null),
                    cancellationToken);

            return;
        }

        await _context.CredentialAccessMessage
            .Where(message => message.Id == id)
            .ExecuteUpdateAsync(update => update
                .SetProperty(message => message.DeletedAt, deletedAt)
                .SetProperty(message => message.LastError, (string?)null),
                cancellationToken);
    }

    public async Task MarkNotified(Guid id, DateTime notifiedAt, CancellationToken cancellationToken)
    {
        await _context.CredentialAccessMessage
            .Where(message => message.Id == id)
            .ExecuteUpdateAsync(update => update
                .SetProperty(message => message.NotifiedAt, notifiedAt)
                .SetProperty(message => message.LastError, (string?)null)
                .SetProperty(message => message.ClaimedUntil, (DateTime?)null),
                cancellationToken);
    }

    public async Task<bool> MarkFailed(
        Guid id,
        int expectedAttempts,
        string error,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken)
    {
        var updated = await _context.CredentialAccessMessage
            .Where(message => message.Id == id && message.Attempts == expectedAttempts)
            .ExecuteUpdateAsync(update => update
                .SetProperty(message => message.Attempts, message => message.Attempts + 1)
                .SetProperty(message => message.LastError, TruncateError(error))
                .SetProperty(message => message.NextAttemptAt, nextAttemptAt)
                .SetProperty(message => message.ClaimedUntil, (DateTime?)null),
                cancellationToken);

        return updated > 0;
    }

    private static string TruncateError(string error)
    {
        return error.Length <= DatabaseConstants.CredentialAccessMessages.LastErrorMaxLength
            ? error
            : error[..DatabaseConstants.CredentialAccessMessages.LastErrorMaxLength];
    }
}
