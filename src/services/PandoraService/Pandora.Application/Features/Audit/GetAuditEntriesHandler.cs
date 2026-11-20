using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Audit;

public sealed class GetAuditEntriesHandler : IRequestHandler<GetAuditEntriesMessage, Result<PagedResult<AuditEntry>>>
{
    private const char IdentifierSeparator = ',';

    private readonly IAuditRepository _auditRepository;

    public GetAuditEntriesHandler(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<Result<PagedResult<AuditEntry>>> Handle(GetAuditEntriesMessage request, CancellationToken cancellationToken)
    {
        var filter = new AuditEntryFilter(
            request.Search,
            request.Action,
            request.RequestSource,
            request.SortBy,
            request.SortDirection ?? SortDirection.Descending,
            ParseUserIds(request.UserIds),
            ParseTelegramUserIds(request.TelegramUserIds));

        var page = await _auditRepository.GetPage(
            request.Page ?? PaginationLimits.DefaultPage,
            request.PageSize ?? PaginationLimits.DefaultPageSize,
            filter,
            cancellationToken);

        return Result<PagedResult<AuditEntry>>.Success(page);
    }

    private static Guid[] ParseUserIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(IdentifierSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => Guid.TryParse(item, out var id) ? id : (Guid?)null)
            .OfType<Guid>()
            .ToArray();
    }

    private static long[] ParseTelegramUserIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(IdentifierSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => long.TryParse(item, out var id) ? id : (long?)null)
            .OfType<long>()
            .ToArray();
    }
}
