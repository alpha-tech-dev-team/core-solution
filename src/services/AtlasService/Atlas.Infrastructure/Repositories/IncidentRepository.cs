using Atlas.Domain.Entities;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Infrastructure.Database.Context;

namespace Atlas.Infrastructure.Repositories;

public sealed class IncidentRepository : BaseRepository<Incident>, IIncidentRepository
{
    public IncidentRepository(AtlasDbContext context)
        : base(context)
    {
    }

    public async Task AddWithStatusHistoryAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        Entities.Add(incident);

        Context.IncidentStatusHistory.Add(new IncidentStatusHistory
        {
            Incident = incident,
            Status = incident.Status,
            CreatedAt = incident.CreatedAt
        });

        await Context.SaveChangesAsync(cancellationToken);
    }
}
