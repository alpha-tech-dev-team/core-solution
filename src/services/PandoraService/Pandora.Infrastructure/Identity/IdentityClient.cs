using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.Core.Interfaces;
using StackExchange.Redis;

namespace Pandora.Infrastructure.Identity;

public sealed class IdentityClient : IIdentityClient
{
    private const string AccessCacheKeyPrefix = "identity:access:";
    private const string ActivateRoute = "api/client/activate";
    private const string CheckRoute = "api/client/check";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IDatabase _database;
    private readonly IdentityOptions _options;
    private readonly ILogger<IdentityClient> _logger;

    public IdentityClient(HttpClient httpClient, IConnectionMultiplexer redis, IOptions<IdentityOptions> options, ILogger<IdentityClient> logger)
    {
        _httpClient = httpClient;
        _database = redis.GetDatabase();
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(ActivateRoute, new { telegramUserId, phone }, SerializerOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<bool>(SerializerOptions, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Identity activate request failed");

            return false;
        }
    }

    public async Task<bool> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken)
    {
        var cacheKey = $"{AccessCacheKeyPrefix}{telegramUserId}:{access}";
        var cached = await _database.StringGetAsync(cacheKey);

        if (cached.HasValue)
        {
            return cached == "1";
        }

        var hasAccess = await CheckUserRemote(telegramUserId, access, cancellationToken);

        await _database.StringSetAsync(cacheKey, hasAccess ? "1" : "0", TimeSpan.FromMinutes(_options.CacheExpiryMinutes));

        return hasAccess;
    }

    private async Task<bool> CheckUserRemote(long telegramUserId, string access, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(CheckRoute, new { telegramUserId, access }, SerializerOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<bool>(SerializerOptions, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Identity check failed for access {Access}", access);

            return false;
        }
    }
}
