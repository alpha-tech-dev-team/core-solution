using Atlas.Domain.Entities;
using Atlas.Domain.Models;

namespace Atlas.Domain.Interfaces.Repositories;

public interface IHardwareRepository : IRepository<Hardware>
{
    Task<PagedResult<Hardware>> GetPageByRoomAsync(Guid roomId, PageRequest page, Func<IQueryable<Hardware>, IOrderedQueryable<Hardware>>? orderBy = null, CancellationToken cancellationToken = default);
    Task<Hardware?> GetByIdInRoomAsync(Guid hardwareId, Guid roomId, CancellationToken cancellationToken = default);
}
