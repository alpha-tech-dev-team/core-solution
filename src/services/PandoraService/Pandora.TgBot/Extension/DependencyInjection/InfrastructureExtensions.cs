using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Database;
using Pandora.Infrastructure.Secrets;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddBotInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.ConnectionString);

        services.AddIdentityClient(configuration);
        services.AddScoped<ISecretStore, ConfigSecretStore>();
        services.AddLocalization(typeof(ConfigSecretStore).Assembly);
        services.AddDbContext<PandoraDbContext>(options => options.UseNpgsql(databaseOptions.ConnectionString));
        services.AddSessionInfrastructure(configuration, databaseOptions.RedisConnection);

        return services;
    }
}
