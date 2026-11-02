using System.Text.Json;
using Atlas.Core.Entities;
using Atlas.Core.Interfaces;
using StackExchange.Redis;

namespace Atlas.Infrastructure.Repositories;

public sealed class RedisSessionRepository : ISessionRepository
{
    private const string SessionKeyPrefix = "session:";
    private const string AllSessionsIndexKey = "sessions:index";

    private readonly IDatabase _database;

    public RedisSessionRepository(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task SaveSession(UserSession session, TimeSpan expiry, CancellationToken cancellationToken)
    {
        var key = GetKey(session.TgUserId);
        var data = JsonSerializer.Serialize(session);

        await _database.StringSetAsync(key, data, expiry);
    }

    public async Task<UserSession?> GetSession(long telegramUserId, CancellationToken cancellationToken)
    {
        var key = GetKey(telegramUserId);
        var data = await _database.StringGetAsync(key);

        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<UserSession>(data.ToString());
    }

    public async Task DeleteSession(long telegramUserId, CancellationToken cancellationToken)
    {
        var key = GetKey(telegramUserId);

        await _database.KeyDeleteAsync(key);
    }

    public async Task AddToIndex(long telegramUserId, DateTimeOffset activeAt, CancellationToken cancellationToken)
    {
        await _database.SortedSetAddAsync(AllSessionsIndexKey, telegramUserId, activeAt.ToUnixTimeSeconds());
    }

    public async Task RemoveFromIndex(long telegramUserId, CancellationToken cancellationToken)
    {
        await _database.SortedSetRemoveAsync(AllSessionsIndexKey, telegramUserId);
    }

    private string GetKey(long telegramUserId)
    {
        return $"{SessionKeyPrefix}{telegramUserId}";
    }
}
