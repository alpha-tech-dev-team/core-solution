using Atlas.Domain.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed class CreateRoomMessage : IRequest<Result<RoomPageItem>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
