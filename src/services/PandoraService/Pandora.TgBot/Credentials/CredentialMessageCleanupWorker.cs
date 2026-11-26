using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pandora.Domain.Options;
using Pandora.TgBot.Application.Features.Credentials;

namespace Pandora.TgBot.Credentials;

public sealed class CredentialMessageCleanupWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<CredentialCleanupOptions> _options;
    private readonly ILogger<CredentialMessageCleanupWorker> _logger;

    public CredentialMessageCleanupWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<CredentialCleanupOptions> options,
        ILogger<CredentialMessageCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _options.Value;

        if (!options.Enabled)
        {
            _logger.LogInformation("Автоудаление сообщений с учётными данными отключено в конфигурации");

            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var cleanupService = scope.ServiceProvider.GetRequiredService<CredentialMessageCleanupService>();

                await cleanupService.ProcessDueAsync(DateTime.UtcNow, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Сбой цикла автоудаления сообщений с учётными данными");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(options.PollIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
