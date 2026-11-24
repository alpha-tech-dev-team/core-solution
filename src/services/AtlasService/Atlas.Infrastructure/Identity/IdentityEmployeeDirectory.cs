using System.Net.Http.Json;
using System.Text.Json;
using Atlas.Domain.Interfaces;
using Atlas.Domain.Models;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Identity;

public sealed class IdentityEmployeeDirectory : IEmployeeDirectory
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IdentityOptions _identityOptions;
    private readonly ILogger<IdentityEmployeeDirectory> _logger;

    public IdentityEmployeeDirectory(
        HttpClient httpClient,
        IOptions<IdentityOptions> identityOptions,
        ILogger<IdentityEmployeeDirectory> logger)
    {
        _httpClient = httpClient;
        _identityOptions = identityOptions.Value;
        _logger = logger;
    }

    public async Task<EmployeeProfile?> FindByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                ClientApiRoutes.EmployeeByTelegramUser(telegramUserId));

            if (!string.IsNullOrWhiteSpace(_identityOptions.ServiceApiKey))
            {
                request.Headers.TryAddWithoutValidation(
                    ClientApiRoutes.ServiceApiKeyHeader,
                    _identityOptions.ServiceApiKey);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Сотрудник {TelegramUserId} не найден в Identity: {StatusCode}", telegramUserId, response.StatusCode);

                return null;
            }

            return await response.Content.ReadFromJsonAsync<EmployeeProfile>(SerializerOptions, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Не удалось получить сотрудника {TelegramUserId} из Identity", telegramUserId);

            return null;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(exception, "Превышено время ожидания ответа Identity для сотрудника {TelegramUserId}", telegramUserId);

            return null;
        }
    }
}
