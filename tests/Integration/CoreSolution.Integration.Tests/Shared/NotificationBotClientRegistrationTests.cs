using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Xunit;
using AtlasInfrastructureExtensions = Atlas.TgBot.Extensions.DependencyInjection.InfrastructureExtensions;

namespace CoreSolution.Integration.Tests.Shared;

public sealed class NotificationBotClientRegistrationTests
{
    private const long InteractiveBotId = 123456;
    private const long NotificationBotId = 654321;

    [Fact]
    public static void AddBotInfrastructure_Atlas_RegistersNotificationServiceWithDedicatedToken()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTelegramBot(IntegrationFixture.AtlasTestToken);

        AtlasInfrastructureExtensions.AddBotInfrastructure(
            services,
            CreateConfiguration(IntegrationFixture.AtlasNotificationTestToken));

        using var provider = services.BuildServiceProvider();
        var notificationClient = (ITelegramBotClient)provider.GetRequiredKeyedService(
            typeof(ITelegramBotClient),
            TelegramServiceCollectionExtensions.NotificationServiceKey)!;
        var interactiveClient = provider.GetRequiredService<ITelegramBotClient>();
        var notificationService = provider.GetRequiredKeyedService<ITelegramNotificationService>(
            TelegramServiceCollectionExtensions.NotificationServiceKey);

        Assert.NotNull(notificationService);
        Assert.Equal(NotificationBotId, notificationClient.BotId);
        Assert.Equal(InteractiveBotId, interactiveClient.BotId);
    }

    [Fact]
    public static void AddBotInfrastructure_Atlas_WithoutNotificationToken_Throws()
    {
        var services = new ServiceCollection();

        Assert.ThrowsAny<ArgumentException>(() =>
            AtlasInfrastructureExtensions.AddBotInfrastructure(services, CreateConfiguration(null)));
    }

    private static IConfiguration CreateConfiguration(string? notificationBotToken)
    {
        var values = new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = "Host=localhost;Database=atlas;Username=atlas;Password=atlas",
            ["Redis:ConnectionString"] = "localhost:6379",
            ["SessionSettings:ExpiryMinutes"] = "30",
            ["Identity:BaseUrl"] = "http://localhost"
        };

        if (notificationBotToken is not null)
        {
            values["Telegram:NotificationBotToken"] = notificationBotToken;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
