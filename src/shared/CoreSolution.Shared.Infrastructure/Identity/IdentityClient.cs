using System.Net.Http.Json;
using System.Text.Json;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace CoreSolution.Shared.Infrastructure.Identity;

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

    public async Task<string?> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(ActivateRoute, new { telegramUserId, phone }, SerializerOptions, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(SerializerOptions, cancellationToken);

            return error?.Code ?? ErrorCodes.AccessNotConfirmed;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Запрос активации пользователя {TelegramUserId} в Identity завершился ошибкой", telegramUserId);

            return ErrorCodes.AccessNotConfirmed;
        }
    }

    public async Task<string?> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(CheckRoute, new { telegramUserId, access }, SerializerOptions, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(SerializerOptions, cancellationToken);

            return error?.Code ?? ErrorCodes.AccessNotConfirmed;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Запрос проверки доступа {Service} пользователя {TelegramUserId} в Identity завершился ошибкой", access, telegramUserId);

            return ErrorCodes.AccessNotConfirmed;
        }
    }

    private sealed record ErrorResponse(string Code);
}
