using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces.Repositories;
using Identity.Domain.Models;
using Identity.Infrastructure.Database;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public Task<User?> GetByPhone(string phone, bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        var query = Context.Users.AsQueryable();

        if (!includeArchived)
        {
            query = query.Where(x => x.Status != UserStatus.Archived);
        }

        var normalized = PhoneNumbers.Normalize(phone);

        return query.FirstOrDefaultAsync(x => x.Phone == normalized, cancellationToken);
    }

    public Task<User?> GetById(Guid id, bool includeRoles = false, CancellationToken cancellationToken = default)
    {
        var query = Context.Users.AsQueryable();

        if (includeRoles)
        {
            query = query.Include(x => x.UserRoles).ThenInclude(x => x.Role);
        }

        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<User?> GetByAdminUsername(string username, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.AdminUsername == username, cancellationToken);
    }

    public Task<User?> GetByTelegramUserId(long telegramUserId, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId, cancellationToken);
    }

    public async Task<EmployeesPage> GetEmployees(
        PagedMessage paging,
        Func<IQueryable<User>, IQueryable<User>> query,
        CancellationToken cancellationToken = default)
    {
        var filtered = query(Context.Users.AsNoTracking());

        var items = await filtered
            .Skip(paging.Skip)
            .Take(paging.EffectivePageSize)
            .ProjectToType<EmployeeInfo>()
            .ToListAsync(cancellationToken);

        var total = await filtered.CountAsync(cancellationToken);

        return new EmployeesPage(items, total, paging.EffectivePage, paging.EffectivePageSize);
    }

    public Task<List<Roles>> GetAllRoles(CancellationToken cancellationToken = default)
    {
        return Context.Roles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    protected override void ApplyTimestamps(DateTime now)
    {
        base.ApplyTimestamps(now);

        var usersWithChangedRoles = Context.ChangeTracker
            .Entries<UserRole>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Deleted)
            .Select(entry => entry.Entity.UserId)
            .ToHashSet();

        foreach (var entry in Context.ChangeTracker.Entries<User>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified)
                && usersWithChangedRoles.Contains(entry.Entity.Id))
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
