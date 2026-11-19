using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using StackExchange.Redis;

namespace Pandora.Infrastructure.Audit;

public sealed class RedisAuditNotifier : IAuditNotifier
{
    public static readonly RedisChannel Channel = RedisChannel.Literal(AuditConstants.AuditChannel);

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisAuditNotifier> _logger;

    public RedisAuditNotifier(IConnectionMultiplexer redis, ILogger<RedisAuditNotifier> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task Notify(AuditEntry entry)
    {
        try
        {
            var payload = JsonSerializer.Serialize(AuditEntryNotification.From(entry), SerializerOptions);
            await _redis.GetSubscriber().PublishAsync(Channel, payload);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось отправить уведомление об аудите {Action}", entry.Action);
        }
    }
}
