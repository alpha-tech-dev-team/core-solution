using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.ProblemCategorySelection;

public record ProblemCategorySelectionMessage(long TelegramUserId, int MessageId) : IRequest<Result<WorkflowResponse>>;
