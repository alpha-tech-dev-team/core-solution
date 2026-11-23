using Atlas.Domain.Entities;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using Atlas.Infrastructure.Database.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IncidentListPage> GetListPageAsync(
        IncidentPageQuery query,
        CancellationToken cancellationToken = default)
    {
        var source = Entities.AsNoTracking();
        var total = await source.CountAsync(cancellationToken);

        var items = await query.OrderBy(source)
            .Skip(query.Paging.Skip)
            .Take(query.Paging.EffectivePageSize)
            .ProjectToType<IncidentPageItem>()
            .ToListAsync(cancellationToken);

        return new IncidentListPage(items, query.Paging.EffectivePage, query.Paging.EffectivePageSize, total);
    }
}
