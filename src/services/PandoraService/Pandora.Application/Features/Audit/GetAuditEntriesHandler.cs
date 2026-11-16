using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;

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
        var page = await _auditRepository.GetPage(
            request.Page ?? PaginationLimits.DefaultPage,
            request.PageSize ?? PaginationLimits.DefaultPageSize,
            cancellationToken);

        return Result<PagedResult<AuditEntry>>.Success(page);
    }
}
