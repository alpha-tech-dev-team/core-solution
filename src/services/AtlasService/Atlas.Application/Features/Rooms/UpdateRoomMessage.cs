using System.Text.Json.Serialization;
using Atlas.Domain.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed class UpdateRoomMessage : IRequest<Result<RoomPageItem>>
{
    [JsonIgnore]
    public Guid RoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
