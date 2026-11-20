using Atlas.Domain.Constants;
using Atlas.Domain.Interfaces;
using Atlas.Infrastructure.Database.Context;
using Atlas.Infrastructure.Database.Seeds;
using Atlas.Infrastructure.Identity;
using AppRole = CoreSolution.Shared.Enums.UserRole;
using Atlas.TgBot.Extensions.DependencyInjection;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Infrastructure.Identity;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Identity.Api;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces.Security;
using Identity.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Pandora.Infrastructure.Database;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;
using PandoraApplicationExtensions = Pandora.TgBot.Extension.DependencyInjection.ApplicationExtensions;
using PandoraInfrastructureExtensions = Pandora.TgBot.Extension.DependencyInjection.InfrastructureExtensions;
using PandoraOptionsExtensions = Pandora.TgBot.Extension.DependencyInjection.OptionsExtensions;
using PandoraSecretStoreExtensions = Pandora.TgBot.Extension.DependencyInjection.SecretStoreExtensions;

namespace CoreSolution.Integration.Tests;

public sealed class IntegrationFixture : IAsyncLifetime
{
    private const string AtlasTestToken = "123456:integration-test-token";
    private const string AtlasServiceApiKey = "integration-test-atlas-service-api-key";

    private static long _adminPhoneSequence;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("pgvector/pgvector:pg18").Build();

    private readonly RedisContainer _atlasRedis = new RedisBuilder("redis:7-alpine").Build();

    private readonly RedisContainer _pandoraRedis = new RedisBuilder("redis:7-alpine").Build();

    private WebApplicationFactory<AssemblyMarker> _identity = default!;
    private WebApplicationFactory<AssemblyMarker> _identityRateLimited = default!;
    private bool _pandoraMigrated;
    private bool _atlasMigrated;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _atlasRedis.StartAsync(), _pandoraRedis.StartAsync());

        await using (var connection = new NpgsqlConnection(_postgres.GetConnectionString()))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "CREATE DATABASE pandora_integration";
            await command.ExecuteNonQueryAsync();
        }

        _identity = new WebApplicationFactory<AssemblyMarker>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:ConnectionString"] = _postgres.GetConnectionString(),
                    ["Jwt:Secret"] = "integration-test-secret-key-at-least-32-chars",
                    ["Jwt:Issuer"] = "integration-tests",
                    ["Jwt:Audience"] = "integration-tests",
                    ["ClientApi:AtlasApiKey"] = AtlasServiceApiKey,
                    ["RateLimiting:TokenLimit"] = "1000",
                    ["RateLimiting:TokensPerPeriod"] = "1000",
                    ["RateLimiting:Bot:TokenLimit"] = "1000",
                    ["RateLimiting:Bot:TokensPerPeriod"] = "1000",
                    ["RateLimiting:EmployeeDirectory:TokenLimit"] = "1000",
                    ["RateLimiting:EmployeeDirectory:TokensPerPeriod"] = "1000"
                });
            });
        });

        _ = _identity.Services;

        _identityRateLimited = new WebApplicationFactory<AssemblyMarker>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:ConnectionString"] = _postgres.GetConnectionString(),
                    ["Jwt:Secret"] = "integration-test-secret-key-at-least-32-chars",
                    ["Jwt:Issuer"] = "integration-tests",
                    ["Jwt:Audience"] = "integration-tests",
                    ["ClientApi:AtlasApiKey"] = AtlasServiceApiKey,
                    ["RateLimiting:EmployeeDirectory:TokenLimit"] = "2",
                    ["RateLimiting:EmployeeDirectory:TokensPerPeriod"] = "2"
                });
            });
        });

        _ = _identityRateLimited.Services;
    }

    public async Task DisposeAsync()
    {
        await _identity.DisposeAsync();
        await _identityRateLimited.DisposeAsync();
        await _atlasRedis.DisposeAsync();
        await _pandoraRedis.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public async Task<Guid> SeedAdminAsync(string username, string password, string roleName = nameof(AppRole.PandoraAdmin))
    {
        using var scope = _identity.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var role = await database.Roles.SingleOrDefaultAsync(item => item.Name == roleName)
            ?? database.Roles.Add(new Roles { Id = Guid.NewGuid(), Name = roleName }).Entity;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Surname = username,
            Position = "Admin",
            Phone = $"+79{Interlocked.Increment(ref _adminPhoneSequence):D9}",
            Status = UserStatus.Active,
            AdminUsername = username,
            AdminPasswordHash = hasher.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.UpdatedById = user.Id;

        database.Users.Add(user);
        database.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });

        await database.SaveChangesAsync();

        return user.Id;
    }

    public async Task SeedPendingUserAsync(string phone, string roleName = AtlasConstants.ServiceName)
    {
        using var scope = _identity.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var role = await database.Roles.SingleOrDefaultAsync(item => item.Name == roleName)
            ?? database.Roles.Add(new Roles { Id = Guid.NewGuid(), Name = roleName }).Entity;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Integration",
            Surname = "Test",
            Position = "QA",
            Phone = phone,
            Status = UserStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.UpdatedById = user.Id;

        database.Users.Add(user);
        database.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });

        await database.SaveChangesAsync();
    }

    public async Task SeedPandoraPermissionAsync(long telegramUserId, string itemId)
    {
        await using var connection = new NpgsqlConnection(
            _postgres.GetConnectionString().Replace("Database=postgres", "Database=pandora_integration"));
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            insert into secret_metadata (vault_item_id, resource_name, username)
            values (@itemId, @itemId, 'test-login')
            on conflict (vault_item_id) do nothing;

            insert into permissions (telegram_user_id, vault_item_id, is_active)
            values (@telegramUserId, @itemId, true)
            on conflict (telegram_user_id, vault_item_id) do nothing;
            """;
        command.Parameters.AddWithValue("itemId", itemId);
        command.Parameters.AddWithValue("telegramUserId", telegramUserId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<(long? TelegramUserId, UserStatus Status)> GetUserStateAsync(string phone)
    {
        using var scope = _identity.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await database.Users.SingleAsync(item => item.Phone == phone);

        return (user.TelegramUserId, user.Status);
    }

    public async Task<Guid?> GetUserUpdatedByAsync(string phone)
    {
        using var scope = _identity.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await database.Users.SingleAsync(item => item.Phone == phone);

        return user.UpdatedById;
    }

    public async Task SetUserStatusAsync(string phone, UserStatus status)
    {
        using var scope = _identity.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await database.Users.SingleAsync(item => item.Phone == phone);
        user.Status = status;

        await database.SaveChangesAsync();
    }

    public async Task SetTelegramUserAsync(string phone, long telegramUserId, UserStatus status)
    {
        using var scope = _identity.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await database.Users.SingleAsync(item => item.Phone == phone);
        user.TelegramUserId = telegramUserId;
        user.Status = status;

        await database.SaveChangesAsync();
    }

    public HttpClient CreateIdentityHttpClient()
    {
        return _identity.CreateClient();
    }

    public HttpClient CreateAtlasIdentityHttpClient()
    {
        var client = _identity.CreateClient();

        client.DefaultRequestHeaders.TryAddWithoutValidation(
            ClientApiRoutes.ServiceApiKeyHeader,
            AtlasServiceApiKey);

        return client;
    }

    public HttpClient CreateRateLimitedIdentityHttpClient()
    {
        var client = _identityRateLimited.CreateClient();

        client.DefaultRequestHeaders.TryAddWithoutValidation(
            ClientApiRoutes.ServiceApiKeyHeader,
            AtlasServiceApiKey);

        return client;
    }

    public HttpClient CreateIdentityHttpClientWithCookies()
    {
        return _identity.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });
    }

    public ServiceProvider CreateAtlasServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = _postgres.GetConnectionString(),
                ["Redis:ConnectionString"] = _atlasRedis.GetConnectionString(),
                ["SessionSettings:ExpiryMinutes"] = "30",
                ["Identity:BaseUrl"] = "http://localhost",
                ["Identity:ServiceApiKey"] = AtlasServiceApiKey
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTelegramBot(AtlasTestToken);
        services.AddBotInfrastructure(configuration);
        services.AddApplication(configuration, new TestHostEnvironment(Environments.Development));
        services.AddHttpClient<IIdentityClient, IdentityClient>()
            .ConfigurePrimaryHttpMessageHandler(() => _identity.Server.CreateHandler());
        services.AddHttpClient<IEmployeeDirectory, IdentityEmployeeDirectory>()
            .ConfigurePrimaryHttpMessageHandler(() => _identity.Server.CreateHandler());

        return services.BuildServiceProvider();
    }

    public async Task<ServiceProvider> CreateAtlasServiceProviderAsync()
    {
        var provider = CreateAtlasServiceProvider();

        if (!_atlasMigrated)
        {
            using var scope = provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();

            await dbContext.Database.MigrateAsync();
            await IncidentCategorySeed.SeedAsync(dbContext);
            await EquipmentTypeSeed.SeedAsync(dbContext);
            await RoomSeed.SeedAsync(dbContext);
            await HardwareSeed.SeedAsync(dbContext);
            await SoftwareSeed.SeedAsync(dbContext);

            _atlasMigrated = true;
        }

        return provider;
    }

    public async Task<ServiceProvider> CreatePandoraServiceProviderAsync(Action<IServiceCollection>? configureServices = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = _postgres.GetConnectionString().Replace("Database=postgres", "Database=pandora_integration"),
                ["Database:RedisConnection"] = _pandoraRedis.GetConnectionString(),
                ["SessionSettings:ExpiryMinutes"] = "30",
                ["Identity:BaseUrl"] = "http://localhost",
                ["Identity:ServiceApiKey"] = AtlasServiceApiKey,
                ["Pandora:Secrets:UseMock"] = "true",
                ["Pandora:Secrets:Items:0:Name"] = "TestService",
                ["Pandora:Secrets:Items:0:Login"] = "test-login",
                ["Pandora:Secrets:Items:0:Password"] = "test-password"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        PandoraOptionsExtensions.AddPandoraOptions(services, configuration);
        PandoraInfrastructureExtensions.AddBotInfrastructure(services, configuration);
        PandoraSecretStoreExtensions.AddSecretStore(services, configuration);
        PandoraApplicationExtensions.AddApplication(services);
        services.AddHttpClient<IIdentityClient, IdentityClient>()
            .ConfigurePrimaryHttpMessageHandler(() => _identity.Server.CreateHandler());

        configureServices?.Invoke(services);

        var provider = services.BuildServiceProvider();

        if (!_pandoraMigrated)
        {
            using var scope = provider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<PandoraDbContext>().Database.MigrateAsync();
            _pandoraMigrated = true;
        }

        return provider;
    }
}

[CollectionDefinition(nameof(IntegrationCollection))]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationFixture>;

