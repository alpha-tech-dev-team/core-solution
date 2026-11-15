using System.Text.Json;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CoreSolution.Shared.Infrastructure.Sessions;

public sealed class RedisSessionRepository : ISessionRepository
{
    private const string SessionKeyPrefix = "session:";
    private const string AllSessionsIndexKey = "sessions:index";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IDatabase _database;
    private readonly ISessionFactory _sessionFactory;
    private readonly ILogger<RedisSessionRepository> _logger;

    public RedisSessionRepository(
        IConnectionMultiplexer redis,
        ISessionFactory sessionFactory,
        ILogger<RedisSessionRepository> logger)
    {
        _database = redis.GetDatabase();
        _sessionFactory = sessionFactory;
        _logger = logger;
    }

    public async Task SaveSession(UserSession session, TimeSpan expiry, CancellationToken cancellationToken)
    {
        var key = GetKey(session.TgUserId);
        var data = JsonSerializer.Serialize(session, session.GetType(), SerializerOptions);

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

        try
        {
            return (UserSession?)JsonSerializer.Deserialize(data.ToString(), _sessionFactory.SessionType, SerializerOptions);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Сессия пользователя {TgUserId} повреждена и будет пересоздана", telegramUserId);

            return null;
        }
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

    private static string GetKey(long telegramUserId)
    {
        return $"{SessionKeyPrefix}{telegramUserId}";
    }
}
