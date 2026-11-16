using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Entities;

namespace Pandora.Application.Features.Audit;

public sealed class GetAuditEntriesMessage : PagedQuery, IRequest<Result<PagedResult<AuditEntry>>>
{
}
