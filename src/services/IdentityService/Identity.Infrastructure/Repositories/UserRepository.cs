using Identity.Domain.Entities;
using Identity.Domain.Interfaces.Repositories;
using Identity.Domain.Models;
using Mapster;
using Identity.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public Task<User?> GetByPhone(string phone, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .FirstOrDefaultAsync(x => x.Phone == phone, cancellationToken);
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

    public async Task<EmployeesPage> GetEmployees(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = await Context.Users
            .OrderBy(x => x.Surname)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<EmployeeInfo>()
            .ToListAsync(cancellationToken);

        var total = await Context.Users.CountAsync(cancellationToken);

        return new EmployeesPage(items, total, page, pageSize);
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
