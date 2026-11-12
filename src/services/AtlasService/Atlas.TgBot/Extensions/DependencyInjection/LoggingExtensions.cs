using Serilog;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class LoggingExtensions
{
    public static IServiceCollection AddAtlasLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(serviceProvider));

        return services;
    }
}
