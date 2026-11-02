using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.ProblemCategory;

public record ProblemCategoryMessage(long TelegramUserId, int MessageId) : IRequest<Result<WorkflowResponse>>;