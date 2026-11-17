using Atlas.Domain.Entities;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Infrastructure.Database.Context;
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
}
