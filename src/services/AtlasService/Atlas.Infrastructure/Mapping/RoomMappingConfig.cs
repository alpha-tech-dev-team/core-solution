using Atlas.Domain.Entities;
using Atlas.Domain.Models;
using Mapster;

namespace Atlas.Infrastructure.Mapping;

public sealed class RoomMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Room, RoomPageItem>();
    }
}
