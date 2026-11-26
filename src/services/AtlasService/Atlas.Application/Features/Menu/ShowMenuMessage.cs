using Atlas.Domain.Constants;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Menu;

public record ShowMenuMessage(
    long TelegramUserId,
    int MessageId,
    int? Page = PaginationLimits.DefaultPage,
    int? PageSize = PaginationLimits.DefaultPageSize)
    : PagedMessage(Page, PageSize), IRequest<Result<WorkflowResponse>>;
