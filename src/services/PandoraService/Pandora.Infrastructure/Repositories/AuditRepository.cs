using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Database;

namespace Pandora.Infrastructure.Repositories;

public sealed class AuditRepository : IAuditRepository
{
    private readonly PandoraDbContext _context;

    public AuditRepository(PandoraDbContext context)
    {
        _context = context;
    }

    public async Task Add(AuditEntry entry, CancellationToken cancellationToken)
    {
        _context.AuditEntry.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditEntry>> GetPage(int page, int pageSize, AuditEntryFilter filter, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(page, PaginationLimits.FirstPage);
        var size = Math.Clamp(pageSize, PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize);

        var query = ApplyFilter(_context.AuditEntry.AsNoTracking(), filter);

        var total = await query.CountAsync(cancellationToken);
        var offset = (long)(pageNumber - PaginationLimits.FirstPage) * size;

        if (offset >= total)
        {
            return new PagedResult<AuditEntry>([], pageNumber, size, total);
        }

        var items = await ApplySorting(query, filter)
            .Skip((int)offset)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditEntry>(items, pageNumber, size, total);
    }

    private static IQueryable<AuditEntry> ApplyFilter(IQueryable<AuditEntry> query, AuditEntryFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = ApplySearch(query, filter);
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(entry => entry.Action == filter.Action);
        }

        if (!string.IsNullOrWhiteSpace(filter.RequestSource))
        {
            query = query.Where(entry => entry.RequestSource == filter.RequestSource);
        }

        return query;
    }

    private static IQueryable<AuditEntry> ApplySearch(IQueryable<AuditEntry> query, AuditEntryFilter filter)
    {
        var search = PostgresLikePattern.Contains(filter.Search!.Trim());

        var userIds = filter.UserIds?.ToArray() ?? [];
        var telegramUserIds = filter.TelegramUserIds?.ToArray() ?? [];

        return query.Where(entry =>
            EF.Functions.ILike(entry.Action, search, PostgresLikePattern.EscapeCharacter) ||
            (entry.TargetItemId != null && EF.Functions.ILike(entry.TargetItemId, search, PostgresLikePattern.EscapeCharacter)) ||
            EF.Functions.ILike(entry.RequestSource, search, PostgresLikePattern.EscapeCharacter) ||
            (entry.UserId != null && EF.Functions.ILike(entry.UserId.Value.ToString(), search, PostgresLikePattern.EscapeCharacter)) ||
            (entry.TelegramUserId != null && EF.Functions.ILike(entry.TelegramUserId.Value.ToString(), search, PostgresLikePattern.EscapeCharacter)) ||
            (userIds.Length > 0 && entry.UserId != null && userIds.Contains(entry.UserId.Value)) ||
            (telegramUserIds.Length > 0 && entry.TelegramUserId != null && telegramUserIds.Contains(entry.TelegramUserId.Value)));
    }

    private static IQueryable<AuditEntry> ApplySorting(IQueryable<AuditEntry> query, AuditEntryFilter filter)
    {
        var sortField = AuditEntrySortFields.Resolve(filter.SortBy);
        var selector = AuditEntrySortFields.Fields[sortField];
        var ordered = filter.SortDirection == SortDirection.Ascending
            ? query.OrderBy(selector)
            : query.OrderByDescending(selector);

        return sortField == AuditEntrySortFields.Timestamp
            ? ordered.ThenBy(entry => entry.Id)
            : ordered.ThenByDescending(entry => entry.Timestamp).ThenBy(entry => entry.Id);
    }
}
