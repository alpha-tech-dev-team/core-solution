using Atlas.TgBot.Notifications;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Xunit;
using AtlasApplicationExtensions = Atlas.TgBot.Extensions.DependencyInjection.ApplicationExtensions;

namespace CoreSolution.Integration.Tests.Shared;

public sealed class NotificationBotClientRegistrationTests
{
    private const long InteractiveBotId = 123456;
    private const long NotificationBotId = 654321;

    [Fact]
    public static void AddApplication_Atlas_RegistersNotificationClientWithDedicatedToken()
    {
        var services = new ServiceCollection();
        services.AddTelegramBot(IntegrationFixture.AtlasTestToken);

        AtlasApplicationExtensions.AddApplication(
            services,
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Telegram:NotificationBotToken"] = IntegrationFixture.AtlasNotificationTestToken
                })
                .Build(),
            new TestHostEnvironment(Environments.Production));

        using var provider = services.BuildServiceProvider();
        var notificationClientKey = services
            .Single(descriptor => descriptor.ServiceType == typeof(ITelegramBotClient) && descriptor.IsKeyedService)
            .ServiceKey!;

        var notificationClient = (ITelegramBotClient)provider.GetRequiredKeyedService(
            typeof(ITelegramBotClient),
            notificationClientKey)!;
        var interactiveClient = provider.GetRequiredService<ITelegramBotClient>();
        var alertSender = provider.GetRequiredService<IIncidentAlertSender>();

        Assert.IsType<TelegramIncidentAlertSender>(alertSender);
        Assert.Equal(NotificationBotId, notificationClient.BotId);
        Assert.Equal(InteractiveBotId, interactiveClient.BotId);
    }

    [Fact]
    public static void AddApplication_Atlas_WithoutNotificationToken_Throws()
    {
        var services = new ServiceCollection();

        Assert.ThrowsAny<ArgumentException>(() =>
            AtlasApplicationExtensions.AddApplication(
                services,
                new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Telegram:BotToken"] = IntegrationFixture.AtlasTestToken
                    })
                    .Build(),
                new TestHostEnvironment(Environments.Production)));
    }
}
