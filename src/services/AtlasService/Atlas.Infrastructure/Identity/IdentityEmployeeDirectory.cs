using System.Net.Http.Json;
using System.Text.Json;
using Atlas.Domain.Interfaces;
using Atlas.Domain.Models;
using CoreSolution.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace Atlas.Infrastructure.Identity;

public sealed class IdentityEmployeeDirectory : IEmployeeDirectory
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityEmployeeDirectory> _logger;

    public IdentityEmployeeDirectory(HttpClient httpClient, ILogger<IdentityEmployeeDirectory> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<EmployeeProfile?> FindByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                ClientApiRoutes.EmployeeByTelegramUser(telegramUserId),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Сотрудник {TelegramUserId} не найден в Identity: {StatusCode}",
                    telegramUserId,
                    response.StatusCode);

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
