using System.Net.Http.Json;
using System.Text.Json;
using System.Net;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreSolution.Shared.Infrastructure.Identity;

public sealed class IdentityClient : IIdentityClient
{
    private const string ActivateRoute = "api/client/activate";
    private const string CheckRoute = "api/client/check";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IdentityOptions _identityOptions;
    private readonly ILogger<IdentityClient> _logger;

    public IdentityClient(
        HttpClient httpClient,
        IOptions<IdentityOptions> identityOptions,
        ILogger<IdentityClient> logger)
    {
        _httpClient = httpClient;
        _identityOptions = identityOptions.Value;
        _logger = logger;
    }

    private HttpRequestMessage CreateBotRequest(HttpMethod method, string route)
    {
        var request = new HttpRequestMessage(method, route);

        if (!string.IsNullOrWhiteSpace(_identityOptions.ServiceApiKey))
        {
            request.Headers.TryAddWithoutValidation(ClientApiRoutes.ServiceApiKeyHeader, _identityOptions.ServiceApiKey);
        }

        return request;
    }

    public async Task<string?> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken)
    {
        try
        {
            using var request = CreateBotRequest(HttpMethod.Post, ActivateRoute);
            request.Content = JsonContent.Create(new { telegramUserId, phone }, options: SerializerOptions);
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            var error = await TryReadErrorCodeAsync(response, cancellationToken);

            return error ?? (response.StatusCode == HttpStatusCode.TooManyRequests
                ? ErrorCodes.ServiceTemporarilyUnavailable
                : ErrorCodes.AccessNotConfirmed);
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
            using var request = CreateBotRequest(HttpMethod.Post, CheckRoute);
            request.Content = JsonContent.Create(new { telegramUserId, access }, options: SerializerOptions);
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            var error = await TryReadErrorCodeAsync(response, cancellationToken);

            return error ?? (response.StatusCode == HttpStatusCode.TooManyRequests
                ? ErrorCodes.ServiceTemporarilyUnavailable
                : ErrorCodes.AccessNotConfirmed);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Запрос проверки доступа {Service} пользователя {TelegramUserId} в Identity завершился ошибкой", access, telegramUserId);

            return ErrorCodes.AccessNotConfirmed;
        }
    }

    private async Task<string?> TryReadErrorCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength is 0)
        {
            return null;
        }

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(SerializerOptions, cancellationToken);

            return error?.Code;
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(
                exception,
                "Identity вернул ответ {StatusCode} без корректного JSON",
                (int)response.StatusCode);

            return null;
        }
        catch (NotSupportedException exception)
        {
            _logger.LogWarning(
                exception,
                "Identity вернул ответ {StatusCode} с неподдерживаемым типом содержимого",
                (int)response.StatusCode);

            return null;
        }
    }
}
