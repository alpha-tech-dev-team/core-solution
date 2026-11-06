using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pandora.Application.Features.Secrets;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Options;
using Pandora.Infrastructure.Database;
using Pandora.Infrastructure.Repositories;
using Pandora.Infrastructure.Secrets;

namespace Pandora.TgBot.Extension.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddBotInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.ConnectionString);

        var vaultwardenOptions = configuration.GetSection(VaultwardenOptions.SectionName).Get<VaultwardenOptions>() ?? new VaultwardenOptions();

        services.AddIdentityClient(configuration);
        services.AddLocalization(typeof(ConfigSecretStore).Assembly);
        services.AddDbContext<PandoraDbContext>(options => options.UseNpgsql(databaseOptions.ConnectionString));
        services.AddSessionInfrastructure(configuration, databaseOptions.RedisConnection);
        services.AddScoped<ISecretMetadataRepository, SecretMetadataRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<SecretAuditLogger>();
        services.AddHttpClient<VaultwardenSecretWriter>(client =>
        {
            if (!string.IsNullOrWhiteSpace(vaultwardenOptions.BaseUrl))
            {
                client.BaseAddress = new Uri(vaultwardenOptions.BaseUrl);
            }
        });
        services.AddScoped<ISecretWriter>(provider => provider.GetRequiredService<VaultwardenSecretWriter>());

        return services;
    }
}
