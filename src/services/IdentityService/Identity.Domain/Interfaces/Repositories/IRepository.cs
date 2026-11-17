namespace Identity.Domain.Interfaces.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    void Add(TEntity entity);
    Task<int> SaveChanges(CancellationToken cancellationToken = default);
}
