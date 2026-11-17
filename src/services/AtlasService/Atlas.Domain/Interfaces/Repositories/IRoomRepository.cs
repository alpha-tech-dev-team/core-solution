using Atlas.Domain.Entities;

namespace Atlas.Domain.Interfaces.Repositories;

public interface IRoomRepository : IRepository<Room>
{
    Task<IReadOnlyList<Room>> FindByNameAsync(
        string nameFragment,
        Func<IQueryable<Room>, IOrderedQueryable<Room>> orderBy,
        CancellationToken cancellationToken = default);
}
