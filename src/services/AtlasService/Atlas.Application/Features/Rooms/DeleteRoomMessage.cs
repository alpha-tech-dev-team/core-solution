using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed record DeleteRoomMessage(Guid RoomId) : IRequest<Result>;
