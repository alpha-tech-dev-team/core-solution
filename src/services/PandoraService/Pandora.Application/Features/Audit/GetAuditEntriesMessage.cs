using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Audit;

public sealed class GetAuditEntriesMessage : IRequest<Result<AuditEntryPage>>
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
