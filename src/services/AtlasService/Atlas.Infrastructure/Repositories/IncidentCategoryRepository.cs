using Atlas.Domain.Entities;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Infrastructure.Database.Context;

namespace Atlas.Infrastructure.Repositories;

public sealed class IncidentCategoryRepository : BaseRepository<IncidentCategory>, IIncidentCategoryRepository
{
    public IncidentCategoryRepository(AtlasDbContext context)
        : base(context)
    {
    }
}
