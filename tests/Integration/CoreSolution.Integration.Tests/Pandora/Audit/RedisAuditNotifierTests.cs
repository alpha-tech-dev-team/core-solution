using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Pandora.Domain.Models;
using Pandora.Infrastructure.Audit;
using StackExchange.Redis;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

public sealed class RedisAuditNotifierTests
{
    [Fact]
    public async Task Notify_WhenRedisFails_PropagatesException()
    {
        var redis = DispatchProxy.Create<IConnectionMultiplexer, FailingRedisProxy>();
        var notifier = new RedisAuditNotifier(redis, NullLogger<RedisAuditNotifier>.Instance);
        var notification = new AuditEntryNotification(
            Guid.NewGuid(), DateTime.UtcNow, null, 42, "action", null, "telegram");

        await Assert.ThrowsAsync<RedisException>(() => notifier.Notify(notification));
    }

    public class FailingRedisProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            throw new RedisException("Redis недоступен");
        }
    }
}
