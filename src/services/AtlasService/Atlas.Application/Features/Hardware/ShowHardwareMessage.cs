using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Hardware;

public record ShowHardwareMessage(long TelegramUserId, int MessageId, int Page) : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest;
