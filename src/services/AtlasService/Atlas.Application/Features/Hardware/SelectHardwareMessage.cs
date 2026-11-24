using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Hardware;

public record SelectHardwareMessage(long TelegramUserId, int MessageId, Guid HardwareId) : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest;
