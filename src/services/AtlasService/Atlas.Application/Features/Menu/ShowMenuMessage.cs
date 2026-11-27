using Atlas.Domain.Constants;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Menu;

public record ShowMenuMessage(
    long TelegramUserId,
    int MessageId,
    int? Page = PaginationLimits.DefaultPage)
    : PagedMessage(Page, PaginationLimits.DefaultPageSize), IRequest<Result<WorkflowResponse>>;
