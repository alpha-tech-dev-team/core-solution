using System.Text.Json;
using Pandora.Core.Entities;
using Pandora.Core.Interfaces;
using StackExchange.Redis;

namespace Pandora.Infrastructure.Repositories;

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
        var key = GetKey(session.Id);
        var data = JsonSerializer.Serialize(session);

        await _database.StringSetAsync(key, data, expiry);
    }

    public async Task<UserSession?> GetSession(long chatId, CancellationToken cancellationToken)
    {
        var key = GetKey(chatId);
        var data = await _database.StringGetAsync(key);

        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<UserSession>(data.ToString());
    }

    public async Task DeleteSession(long chatId, CancellationToken cancellationToken)
    {
        var key = GetKey(chatId);

        await _database.KeyDeleteAsync(key);
    }

    public async Task AddToIndex(long chatId, DateTimeOffset activeAt, CancellationToken cancellationToken)
    {
        await _database.SortedSetAddAsync(AllSessionsIndexKey, chatId, activeAt.ToUnixTimeSeconds());
    }

    public async Task RemoveFromIndex(long chatId, CancellationToken cancellationToken)
    {
        await _database.SortedSetRemoveAsync(AllSessionsIndexKey, chatId);
    }

    private string GetKey(long chatId)
    {
        return $"{SessionKeyPrefix}{chatId}";
    }
}
