using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Domain.Entities;

namespace Pandora.Application.Features.Audit;

public sealed record GetAuditEntriesMessage(
    int? Page = null,
    int? PageSize = null,
    string? Search = null,
    string? SortBy = null,
    SortDirection? SortDirection = null,
    string? Action = null,
    string? RequestSource = null,
    string? UserIds = null,
    string? TelegramUserIds = null)
    : PagedSearchSortMessage(Page, PageSize, Search, SortBy, SortDirection),
        IRequest<Result<PagedResult<AuditEntry>>>;
