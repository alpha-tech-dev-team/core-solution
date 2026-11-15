using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Constants;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Audit;

public sealed class GetAuditEntriesMessageHandler : IRequestHandler<GetAuditEntriesMessage, Result<AuditEntryPage>>
{
    private readonly IAuditRepository _auditRepository;

    public GetAuditEntriesMessageHandler(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<Result<AuditEntryPage>> Handle(GetAuditEntriesMessage request, CancellationToken cancellationToken)
    {
        var page = await _auditRepository.GetPage(
            request.Page ?? PaginationLimits.DefaultPage,
            request.PageSize ?? PaginationLimits.DefaultPageSize,
            cancellationToken);

        return Result<AuditEntryPage>.Success(page);
    }
}
