using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pandora.Core.Interfaces;

namespace Pandora.Infrastructure.Identity;

public sealed class IdentityClient : IIdentityClient
{
    private const string ActivateRoute = "api/client/activate";
    private const string CheckRoute = "api/client/check";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

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
            var response = await _httpClient.PostAsJsonAsync(ActivateRoute, new { telegramUserId, phone }, SerializerOptions, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Запрос активации пользователя в Identity завершился ошибкой");

            return false;
        }
    }

    public async Task<bool> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(CheckRoute, new { telegramUserId, access }, SerializerOptions, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Запрос проверки доступа {Access} в Identity завершился ошибкой", access);

            return false;
        }
    }
}
