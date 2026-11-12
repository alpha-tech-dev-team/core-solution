using Identity.Domain.Interfaces.Entities;
using Identity.Domain.Interfaces.Repositories;
using Identity.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity>(AppDbContext context) : IRepository<TEntity> where TEntity : class
{
    protected AppDbContext Context { get; } = context;

    public void Add(TEntity entity)
    {
        Context.Set<TEntity>().Add(entity);
    }

    public void Remove(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    public virtual Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps(DateTime.UtcNow);

        return Context.SaveChangesAsync(cancellationToken);
    }

    protected virtual void ApplyTimestamps(DateTime now)
    {
        foreach (var entry in Context.ChangeTracker.Entries())
        {
            if ((entry.State is EntityState.Added or EntityState.Modified) && entry.Entity is IAuditableEntity auditable)
            {
                auditable.UpdatedAt = now;
            }
        }
    }
}
