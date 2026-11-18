using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public record SelectRoomMessage(long TelegramUserId, int MessageId, Guid RoomId)
    : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest;
