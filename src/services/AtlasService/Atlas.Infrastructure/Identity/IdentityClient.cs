using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Atlas.Core.Interfaces;
using Atlas.Infrastructure.Options;
using System.Net;

namespace Atlas.Infrastructure.Identity;

public sealed class IdentityClient : IIdentityClient
{
    private const string ActivateRoute = "api/client/activate";
    private const string Checkroute = "api/client/check";
    
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults .Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityClient> _logger;

    public IdentityClient(HttpClient httpClient, ILogger<IdentityClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                ActivateRoute, new{telegramUserId, phone}, JsonOptions, cancellationToken
            );
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "identity ActivateUser request failed", telegramUserId);
            return false;
        }
    }

    public async Task<bool> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken)
    {
        try
        {
            var response = await  _httpClient.PostAsJsonAsync(
                Checkroute, new {telegramUserId, access}, JsonOptions, cancellationToken
            );
            return response.IsSuccessStatusCode;
        }
        catch(HttpRequestException ex)
        {
            _logger.LogWarning(ex, "identity CheckUser request failed", telegramUserId);
            return false;
        }
    }
}