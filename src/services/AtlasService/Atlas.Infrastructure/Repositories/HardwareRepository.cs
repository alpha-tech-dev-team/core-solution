using Atlas.Domain.Entities;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using Atlas.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Repositories;

public sealed class HardwareRepository : BaseRepository<Hardware>, IHardwareRepository
{
    public HardwareRepository(AtlasDbContext context)
        : base(context)
    {
    }

    public async Task<PagedResult<Hardware>> GetPageByRoomAsync(
        Guid roomId,
        PageRequest page,
        Func<IQueryable<Hardware>, IOrderedQueryable<Hardware>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        var query = Entities.AsNoTracking().Where(item => item.RoomId == roomId);
        var ordered = orderBy is null ? query.OrderBy(item => item.Name).ThenBy(item => item.Id) : orderBy(query);
        var total = await ordered.CountAsync(cancellationToken);
        var items = await ordered.Skip(page.Skip).Take(page.PageSize).ToListAsync(cancellationToken);

        return new PagedResult<Hardware>(items, page.Page, page.PageSize, total);
    }

    public Task<Hardware?> GetByIdInRoomAsync(Guid hardwareId, Guid roomId, CancellationToken cancellationToken = default)
    {
        return Entities.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == hardwareId && item.RoomId == roomId, cancellationToken);
    }
}
