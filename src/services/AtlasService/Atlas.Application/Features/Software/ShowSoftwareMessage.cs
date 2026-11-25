using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Software;

public record ShowSoftwareMessage(long TelegramUserId, int MessageId, int Page) : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest;
