using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Software;

public record SelectSoftwareMessage(long TelegramUserId, int MessageId, Guid SoftwareId) : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest;
