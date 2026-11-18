using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public record SelectRoomByTextMessage(long TelegramUserId, int MessageId, string Text, int? RoomMenuMessageId)
    : IRequest<Result<WorkflowResponse>>, IAuthorizedBotRequest;
