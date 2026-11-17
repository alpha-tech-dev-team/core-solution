using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Entities;

namespace Pandora.Application.Features.Audit;

public sealed record GetAuditEntriesMessage : PagedQuery, IRequest<Result<PagedResult<AuditEntry>>>
{
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? Action { get; set; }
    public string? RequestSource { get; set; }
}
