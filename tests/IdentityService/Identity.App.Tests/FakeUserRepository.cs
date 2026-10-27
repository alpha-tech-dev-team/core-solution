using Identity.Core.Entities;
using Identity.Core.Interfaces.Repositories;

namespace Identity.App.Tests;

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly User? _user;

    public FakeUserRepository(User? user)
    {
        _user = user;
    }

    public Task<User?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_user?.Id == id ? _user : null);
    }

    public Task<User?> GetByPhone(string phone, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_user?.Phone == phone ? _user : null);
    }

    public Task<User?> GetByTelegramUserId(long telegramUserId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_user?.TelegramUserId == telegramUserId ? _user : null);
    }

    public Task<User?> GetByAdminUsername(string username, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_user?.AdminUsername == username ? _user : null);
    }

    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }
}
