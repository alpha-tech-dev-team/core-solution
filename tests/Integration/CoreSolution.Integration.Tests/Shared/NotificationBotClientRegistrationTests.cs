using Atlas.Domain.Interfaces.Services;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using Xunit;
using AtlasInfrastructureExtensions = Atlas.TgBot.Extensions.DependencyInjection.InfrastructureExtensions;

namespace CoreSolution.Integration.Tests.Shared;

public sealed class NotificationBotClientRegistrationTests
{
    private const long InteractiveBotId = 123456;
    private const long NotificationBotId = 654321;
    private const long IncidentChatId = 8401;

    [Fact]
    public static void AddBotInfrastructure_Atlas_RegistersNotificationServiceWithDedicatedToken()
    {
        var services = CreateServices();

        using var provider = services.BuildServiceProvider();
        var notificationClient = (ITelegramBotClient)provider.GetRequiredKeyedService(
            typeof(ITelegramBotClient),
            AtlasInfrastructureExtensions.NotificationServiceKey)!;
        var interactiveClient = provider.GetRequiredService<ITelegramBotClient>();
        var notificationService = provider.GetRequiredKeyedService<ITelegramNotificationService>(
            AtlasInfrastructureExtensions.NotificationServiceKey);

        Assert.NotNull(notificationService);
        Assert.Equal(NotificationBotId, notificationClient.BotId);
        Assert.Equal(InteractiveBotId, interactiveClient.BotId);
    }

    [Fact]
    public static async Task AddBotInfrastructure_Atlas_IncidentNotifier_UsesNotificationBotClient()
    {
        var services = CreateServices();
        var botClient = new RecordingTelegramBotClient();
        services.AddKeyedSingleton<ITelegramBotClient>(
            AtlasInfrastructureExtensions.NotificationServiceKey,
            botClient);

        using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IIncidentNotifier>().SendAsync(IncidentChatId, "Текст уведомления");

        var request = Assert.Single(botClient.SentMessages);
        Assert.Equal(new ChatId(IncidentChatId), request.ChatId);
    }

    [Fact]
    public static void AddBotInfrastructure_Atlas_WithoutNotificationToken_Throws()
    {
        var services = new ServiceCollection();

        Assert.ThrowsAny<ArgumentException>(() =>
            AtlasInfrastructureExtensions.AddBotInfrastructure(services, CreateConfiguration(null)));
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTelegramBot(IntegrationFixture.AtlasTestToken);

        AtlasInfrastructureExtensions.AddBotInfrastructure(
            services,
            CreateConfiguration(IntegrationFixture.AtlasNotificationTestToken));

        return services;
    }

    private static IConfiguration CreateConfiguration(string? notificationBotToken)
    {
        var values = new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = "Host=localhost;Database=atlas;Username=atlas;Password=atlas",
            ["Redis:ConnectionString"] = "localhost:6379",
            ["SessionSettings:ExpiryMinutes"] = "30",
            ["Identity:BaseUrl"] = "http://localhost",
            ["Identity:ServiceApiKey"] = IntegrationFixture.AtlasServiceApiKey,
            ["Telegram:NotificationBotToken"] = notificationBotToken ?? string.Empty
        };

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private sealed class RecordingTelegramBotClient : ITelegramBotClient
    {
        public List<SendMessageRequest> SentMessages { get; } = [];

        public bool LocalBotServer => false;

        public long BotId => NotificationBotId;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        public IExceptionParser ExceptionsParser { get; set; } = default!;

        public event AsyncEventHandler<ApiRequestEventArgs>? OnMakingApiRequest
        {
            add { }
            remove { }
        }

        public event AsyncEventHandler<ApiResponseEventArgs>? OnApiResponseReceived
        {
            add { }
            remove { }
        }

        public Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is SendMessageRequest message)
            {
                SentMessages.Add(message);
            }

            return Task.FromResult((TResponse)(object)new Message());
        }

        public Task<bool> TestApi(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task DownloadFile(string filePath, Stream destination, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DownloadFile(TGFile file, Stream destination, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
