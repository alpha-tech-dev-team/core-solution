using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.IncidentCategories;

public record SelectProblemCategoryMessage(long TelegramUserId, int MessageId, Guid CategoryId)
    : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest;
