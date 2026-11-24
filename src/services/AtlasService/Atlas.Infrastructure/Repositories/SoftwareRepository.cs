using Atlas.Domain.Entities;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Infrastructure.Database.Context;

namespace Atlas.Infrastructure.Repositories;

public sealed class SoftwareRepository : BaseRepository<Software>, ISoftwareRepository
{
    public SoftwareRepository(AtlasDbContext context)
        : base(context)
    {
    }
}
