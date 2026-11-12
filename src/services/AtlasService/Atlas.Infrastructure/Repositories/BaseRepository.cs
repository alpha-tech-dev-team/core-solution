using Atlas.Domain.Interfaces;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using Atlas.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity> : IRepository<TEntity>
    where TEntity : class, IEntity
{
    protected AtlasDbContext Context { get; }

    protected DbSet<TEntity> Entities => Context.Set<TEntity>();

    protected BaseRepository(AtlasDbContext context)
    {
        Context = context;
    }

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Entities
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task<PagedResult<TEntity>> GetPageAsync(
        PageRequest page,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        var query = Entities.AsNoTracking();

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TEntity>(items, page.Page, page.PageSize, total);
    }

    public void Add(TEntity entity)
    {
        Entities.Add(entity);
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        Entities.AddRange(entities);
    }

    public void Update(TEntity entity)
    {
        Entities.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        Entities.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}
