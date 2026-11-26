using Atlas.Domain.Entities;
using Atlas.Domain.Models;

namespace Atlas.Domain.Interfaces.Repositories;

public interface IRoomRepository : IRepository<Room>
{
    Task<IReadOnlyList<Room>> FindByNameAsync(
        string nameFragment,
        Func<IQueryable<Room>, IOrderedQueryable<Room>> orderBy,
        CancellationToken cancellationToken = default);

    Task<RoomListPage> GetListPageAsync(RoomPageQuery query, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasDependenciesAsync(Guid roomId, CancellationToken cancellationToken = default);
}
