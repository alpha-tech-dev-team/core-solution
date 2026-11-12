using System.Text.Json;
using Atlas.Domain.Interfaces;
using Atlas.Domain.Models;
using StackExchange.Redis;

namespace Atlas.Infrastructure.Sessions;

public sealed class RedisDraftStore : IDraftStore
{
    private const string KeyPrefix = "atlas:draft:";
    private const int ExpiryMinutes = 60;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IDatabase _database;

    public RedisDraftStore(IConnectionMultiplexer multiplexer)
    {
        _database = multiplexer.GetDatabase();
    }

    public async Task<IncidentDraft?> GetAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(GetKey(telegramUserId));

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<IncidentDraft>(value.ToString(), SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task SaveAsync(long telegramUserId, IncidentDraft draft, CancellationToken cancellationToken = default)
    {
        var value = JsonSerializer.Serialize(draft, SerializerOptions);

        await _database.StringSetAsync(GetKey(telegramUserId), value, TimeSpan.FromMinutes(ExpiryMinutes));
    }

    public async Task ClearAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(GetKey(telegramUserId));
    }

    private static string GetKey(long telegramUserId)
    {
        return $"{KeyPrefix}{telegramUserId}";
    }
}
