using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Audit;
using StackExchange.Redis;

namespace Pandora.Api.Hubs;

public sealed class AuditNotificationSubscriber : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _redis;
    private readonly IHubContext<AuditHub> _hubContext;
    private readonly ILogger<AuditNotificationSubscriber> _logger;

    public AuditNotificationSubscriber(
        IConnectionMultiplexer redis,
        IHubContext<AuditHub> hubContext,
        ILogger<AuditNotificationSubscriber> logger)
    {
        _redis = redis;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queue = await _redis.GetSubscriber().SubscribeAsync(RedisAuditNotifier.Channel);
        queue.OnMessage(BroadcastAsync);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            await queue.UnsubscribeAsync();
        }
    }

    private async Task BroadcastAsync(ChannelMessage message)
    {
        try
        {
            var notification = JsonSerializer.Deserialize<AuditEntryNotification>(
                message.Message.ToString(),
                SerializerOptions);

            if (notification is not null)
            {
                await _hubContext.Clients.All.SendAsync(AuditHub.ReceiveAuditEntryMethod, notification);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось разослать уведомление об аудите подписчикам");
        }
    }
}
