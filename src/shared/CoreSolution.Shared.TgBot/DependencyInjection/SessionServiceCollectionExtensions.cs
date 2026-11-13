using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Infrastructure.Sessions;
using CoreSolution.Shared.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CoreSolution.Shared.TgBot.DependencyInjection;

public static class SessionServiceCollectionExtensions
{
    public static IServiceCollection AddSessionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string redisConnection,
        bool requiresPhoneConfirmation = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(redisConnection, nameof(redisConnection));

        var section = configuration.GetSection(SessionOptions.SectionName);
        var sessionOptions = section.Get<SessionOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {SessionOptions.SectionName}.");

        if (sessionOptions.ExpiryMinutes <= 0)
        {
            throw new InvalidOperationException($"{SessionOptions.SectionName}:{nameof(SessionOptions.ExpiryMinutes)} must be greater than zero.");
        }

        services.Configure<SessionOptions>(section);
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddScoped<ISessionRepository, RedisSessionRepository>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IPhoneConfirmationService>(provider => new PhoneConfirmationService(
            provider.GetRequiredService<IIdentityClient>(),
            provider.GetRequiredService<ISessionService>(),
            requiresPhoneConfirmation));
        services.AddScoped<IRequestContext, RequestContext>();

        return services;
    }
}
