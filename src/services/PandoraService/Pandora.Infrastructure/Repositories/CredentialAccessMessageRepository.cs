using Microsoft.EntityFrameworkCore;
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

    public async Task<IReadOnlyList<CredentialAccessMessage>> GetDue(
        DateTime now,
        int limit,
        Func<IQueryable<CredentialAccessMessage>, IOrderedQueryable<CredentialAccessMessage>> orderBy,
        CancellationToken cancellationToken)
    {
        return await orderBy(_context.CredentialAccessMessage
                .AsNoTracking()
                .Where(message => message.NextAttemptAt <= now
                    && (message.DeletedAt == null || message.NotifiedAt == null)))
            .Take(limit)
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

    public async Task MarkDeleted(
        Guid id,
        DateTime deletedAt,
        DateTime? notifiedAt,
        CancellationToken cancellationToken)
    {
        var message = await FindAsync(id, cancellationToken);

        if (message is null)
        {
            return;
        }

        message.DeletedAt = deletedAt;

        if (notifiedAt.HasValue)
        {
            message.NotifiedAt = notifiedAt;
        }

        message.LastError = null;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkNotified(Guid id, DateTime notifiedAt, CancellationToken cancellationToken)
    {
        var message = await FindAsync(id, cancellationToken);

        if (message is null)
        {
            return;
        }

        message.NotifiedAt = notifiedAt;
        message.LastError = null;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailed(
        Guid id,
        string error,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken)
    {
        var message = await FindAsync(id, cancellationToken);

        if (message is null)
        {
            return;
        }

        message.Attempts += 1;
        message.LastError = TruncateError(error);
        message.NextAttemptAt = nextAttemptAt;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<CredentialAccessMessage?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.CredentialAccessMessage
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    private static string TruncateError(string error)
    {
        return error.Length <= DatabaseConstants.CredentialAccessMessages.LastErrorMaxLength
            ? error
            : error[..DatabaseConstants.CredentialAccessMessages.LastErrorMaxLength];
    }
}
