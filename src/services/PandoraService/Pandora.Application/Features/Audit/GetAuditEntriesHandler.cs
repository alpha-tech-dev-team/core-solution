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
            ParseSortDirection(request.SortDirection));

        var page = await _auditRepository.GetPage(
            request.Page ?? PaginationLimits.DefaultPage,
            request.PageSize ?? PaginationLimits.DefaultPageSize,
            filter,
            cancellationToken);

        return Result<PagedResult<AuditEntry>>.Success(page);
    }

    private static AuditSortDirection ParseSortDirection(string? value)
    {
        return string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase)
            ? AuditSortDirection.Ascending
            : AuditSortDirection.Descending;
    }
}
