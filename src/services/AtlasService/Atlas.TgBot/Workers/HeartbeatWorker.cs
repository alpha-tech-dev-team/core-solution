using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Atlas.TgBot.Workers;

public sealed class HeartbeatWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    private readonly ILogger<HeartbeatWorker> _logger;

    public HeartbeatWorker(ILogger<HeartbeatWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Heartbeat worker запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            _logger.LogDebug("Heartbeat: бот жив, {UtcNow}", DateTimeOffset.UtcNow);
        }

        _logger.LogInformation("Heartbeat worker остановлен");
    }
}
