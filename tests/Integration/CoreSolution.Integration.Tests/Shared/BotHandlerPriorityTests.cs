using Atlas.Domain.Enums;
using Atlas.Domain.Models;
using Atlas.TgBot.BotHandlers;
using Atlas.TgBot.Extensions.DependencyInjection;
using CoreSolution.Shared.Infrastructure.Sessions;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.BotHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Xunit;

namespace CoreSolution.Integration.Tests.Shared;

public sealed class BotHandlerPriorityTests
{
    [Fact]
    public void BotHandlers_AreRegisteredWithContactHandlerFirst()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Telegram:BotToken"] = "123456:handler-priority-test-token"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddAtlasBot(configuration);

        var handlers = services
            .Where(descriptor => descriptor.ServiceType == typeof(IBotHandler))
            .Select(descriptor => descriptor.ImplementationType)
            .ToList();

        Assert.NotEmpty(handlers);
        Assert.Equal(typeof(ContactBotHandler), handlers[0]);
        Assert.Contains(typeof(SelectRoomByTextBotHandler), handlers);
        Assert.True(handlers.IndexOf(typeof(ContactBotHandler)) < handlers.IndexOf(typeof(SelectRoomByTextBotHandler)));
    }

    [Fact]
    public void StartCommand_IsNotTakenByRoomSelectionTextHandler()
    {
        var requestContext = new RequestContext
        {
            Session = AtlasUserSession.Create(1001, nameof(WorkflowStep.RoomSelection))
        };

        var handler = new SelectRoomByTextBotHandler(null!, null!, null!, requestContext, null!);

        Assert.False(handler.CanHandle(CreateTextUpdate("/start")));
    }

    [Fact]
    public void StartCommand_IsTakenByContactHandler()
    {
        var handler = new ContactBotHandler(null!, null!, null!);

        Assert.True(handler.CanHandle(CreateTextUpdate("/start")));
    }

    [Fact]
    public void PlainText_IsTakenByRoomSelectionTextHandler()
    {
        var requestContext = new RequestContext
        {
            Session = AtlasUserSession.Create(1001, nameof(WorkflowStep.RoomSelection))
        };

        var handler = new SelectRoomByTextBotHandler(null!, null!, null!, requestContext, null!);

        Assert.True(handler.CanHandle(CreateTextUpdate("101")));
    }

    private static Update CreateTextUpdate(string text)
    {
        return new Update
        {
            Id = 1,
            Message = new Message
            {
                Text = text,
                Chat = new Chat { Id = 1001 },
                From = new User { Id = 1001 }
            }
        };
    }
}
