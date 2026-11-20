using Atlas.Domain.Interfaces;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Options;
using Atlas.Infrastructure.Database.Context;
using Atlas.Infrastructure.Database.Extensions;
using Atlas.Infrastructure.Identity;
using Atlas.Infrastructure.Repositories;
using Atlas.Infrastructure.Sessions;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Options;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pgvector.EntityFrameworkCore;

namespace Atlas.TgBot.Extensions.DependencyInjection;

public static class InfrastructureExtensions
{
    private static readonly TimeSpan IdentityRequestTimeout = TimeSpan.FromSeconds(10);

    public static IServiceCollection AddBotInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {DatabaseOptions.SectionName}.");

        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {RedisOptions.SectionName}.");

        var identityOptions = configuration.GetSection(IdentityOptions.SectionName).Get<IdentityOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section {IdentityOptions.SectionName}.");

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.ConnectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(redisOptions.ConnectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(identityOptions.BaseUrl);

        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        services.AddDbContext<AtlasDbContext>(options =>
            options.UseNpgsql(databaseOptions.ConnectionString, npgsql =>
            {
                npgsql.UseVector();
                npgsql.MapAtlasEnums();
            }));

        services.AddScoped<IIncidentCategoryRepository, IncidentCategoryRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IHardwareRepository, HardwareRepository>();
        services.AddScoped<ISoftwareRepository, SoftwareRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        services.AddIdentityClient(configuration);
        services.AddHttpClient<IEmployeeDirectory, IdentityEmployeeDirectory>(client =>
        {
            client.BaseAddress = new Uri(identityOptions.BaseUrl);
            client.Timeout = IdentityRequestTimeout;
        });
        services.Configure<IdentityOptions>(configuration.GetSection(IdentityOptions.SectionName));
        services.Replace(ServiceDescriptor.Scoped<ISessionFactory, AtlasSessionFactory>());
        services.AddSessionInfrastructure(configuration, redisOptions.ConnectionString);
        services.AddLocalization(typeof(Atlas.Infrastructure.AssemblyMarker).Assembly);

        return services;
    }
}
