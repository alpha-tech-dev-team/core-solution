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

    public async Task<IReadOnlyList<Room>> FindByNameAsync(string nameFragment, CancellationToken cancellationToken = default)
    {
        var normalized = nameFragment.Trim().ToLower();

        return await Entities
            .AsNoTracking()
            .Where(room => room.Name.ToLower().Contains(normalized))
            .OrderBy(room => room.Name)
            .ThenBy(room => room.Id)
            .ToListAsync(cancellationToken);
    }
}
