using System.Text.Json.Serialization;
using Atlas.Application.Contracts;
using CoreSolution.Shared.Results;
using MediatR;

namespace Atlas.Application.Features.Rooms;

public sealed class DeleteRoomMessage : IRequest<Result>, IItemIdRequest
{
    [JsonIgnore]
    public Guid ItemId { get; set; }

    public DeleteRoomMessage()
    {
    }

    public DeleteRoomMessage(Guid itemId)
    {
        ItemId = itemId;
    }
}
