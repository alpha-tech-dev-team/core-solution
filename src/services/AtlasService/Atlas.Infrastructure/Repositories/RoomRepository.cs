using Atlas.Domain.Entities;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using Atlas.Infrastructure.Database.Context;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Repositories;

public sealed class RoomRepository : BaseRepository<Room>, IRoomRepository
{
    public RoomRepository(AtlasDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Room>> FindByNameAsync(
        string nameFragment,
        Func<IQueryable<Room>, IOrderedQueryable<Room>> orderBy,
        CancellationToken cancellationToken = default)
    {
        var normalized = nameFragment.Trim().ToLower();

        var query = Entities
            .AsNoTracking()
            .Where(room => room.Name.ToLower().Contains(normalized));

        return await orderBy(query).ToListAsync(cancellationToken);
    }

    public async Task<RoomListPage> GetListPageAsync(
        RoomPageQuery query,
        CancellationToken cancellationToken = default)
    {
        var source = Entities.AsNoTracking();
        var total = await source.CountAsync(cancellationToken);

        var items = await query.OrderBy(source)
            .Skip(query.Paging.Skip)
            .Take(query.Paging.EffectivePageSize)
            .ProjectToType<RoomPageItem>()
            .ToListAsync(cancellationToken);

        return new RoomListPage(items, query.Paging.EffectivePage, query.Paging.EffectivePageSize, total);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLower();

        return Entities
            .AsNoTracking()
            .AnyAsync(
                room => room.Name.ToLower() == normalized
                    && (excludeId == null || room.Id != excludeId),
                cancellationToken);
    }

    public async Task<bool> HasDependenciesAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        var hasHardware = await Context.Hardware
            .AsNoTracking()
            .AnyAsync(hardware => hardware.RoomId == roomId, cancellationToken);

        if (hasHardware)
        {
            return true;
        }

        return await Context.Incidents
            .AsNoTracking()
            .AnyAsync(incident => incident.RoomId == roomId, cancellationToken);
    }
}
