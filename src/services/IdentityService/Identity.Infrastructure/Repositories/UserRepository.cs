using Identity.Core.Entities;
using Identity.Core.Interfaces.Repositories;
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

    public Task SaveChanges(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
