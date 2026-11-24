using System.Text.Json.Serialization;
using Atlas.Application.Contracts;
using Atlas.Domain.Models;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed class UpdateRoomMessage : IRequest<Result<RoomPageItem>>, IItemIdRequest
{
    [JsonIgnore]
    public Guid ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
