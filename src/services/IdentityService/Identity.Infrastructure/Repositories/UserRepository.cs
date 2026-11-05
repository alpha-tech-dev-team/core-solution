using Identity.Domain.Entities;
using Identity.Domain.Interfaces.Repositories;
using Identity.Domain.Models;
using Identity.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByPhone(string phone, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .FirstOrDefaultAsync(x => x.Phone == phone, cancellationToken);
    }

    public Task<User?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<User?> GetByAdminUsername(string username, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.AdminUsername == username, cancellationToken);
    }

    public Task<User?> GetByTelegramUserId(long telegramUserId, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId, cancellationToken);
    }

    public Task<List<EmployeeInfo>> GetEmployees(CancellationToken cancellationToken = default)
    {
        return _context.Users
            .AsNoTracking()
            .OrderBy(x => x.Surname)
            .ThenBy(x => x.Name)
            .Select(x => new EmployeeInfo(
                x.Id,
                x.Name,
                x.Surname,
                x.Patronymic,
                x.Position,
                x.Phone,
                x.Status.ToString(),
                x.UserRoles.Select(role => role.Role.Name).ToArray()))
            .ToListAsync(cancellationToken);
    }

    public Task<List<Roles>> GetAllRoles(CancellationToken cancellationToken = default)
    {
        return _context.Roles
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
