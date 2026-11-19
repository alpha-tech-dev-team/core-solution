using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Audit;
using StackExchange.Redis;

namespace Pandora.Api.Hubs;

public sealed class AuditNotificationSubscriber : BackgroundService
{
    private const int SubscriptionRetryDelaySeconds = 5;
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
        while (!stoppingToken.IsCancellationRequested)
        {
            ISubscriber? subscriber = null;
            var subscribed = false;

            try
            {
                subscriber = _redis.GetSubscriber();
                var queue = await subscriber.SubscribeAsync(RedisAuditNotifier.Channel);
                subscribed = true;
                queue.OnMessage(BroadcastAsync);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось подписаться на уведомления аудита через Redis");
            }
            finally
            {
                if (subscribed && subscriber is not null)
                {
                    await Unsubscribe(subscriber);
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(SubscriptionRetryDelaySeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task Unsubscribe(ISubscriber subscriber)
    {
        try
        {
            await subscriber.UnsubscribeAsync(RedisAuditNotifier.Channel);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось отписаться от уведомлений аудита через Redis");
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
