using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Menu;

public record ShowMenuMessage(long TelegramUserId, int MessageId) : IRequest<Result<WorkflowResponse>>;
