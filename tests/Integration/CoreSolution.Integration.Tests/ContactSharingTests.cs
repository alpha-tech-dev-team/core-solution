using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.BotHandlers;
using CoreSolution.Shared.TgBot.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace CoreSolution.Integration.Tests;

[Collection(nameof(IntegrationCollection))]
public sealed class ContactSharingTests(IntegrationFixture fixture)
{
    [Fact]
    public void ToTelegramMarkup_RequestContact_ReturnsContactKeyboard()
    {
        var result = Result<WorkflowResponse>.Success(new WorkflowResponse
        {
            Text = "Отправьте номер телефона",
            RequestContact = true
        });

        var markup = result.ToTelegramMarkup("Отправить номер телефона");

        var keyboard = Assert.IsType<ReplyKeyboardMarkup>(markup);
        var button = Assert.Single(keyboard.Keyboard.SelectMany(row => row));
        Assert.Equal("Отправить номер телефона", button.Text);
        Assert.True(button.RequestContact);
    }

    [Fact]
    public async Task ContactBotHandler_CanHandle_AcceptsTypedPhoneAndStartOnly()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();

        var handler = scope.ServiceProvider.GetServices<IBotHandler>()
            .OfType<ContactBotHandler>()
            .Single();

        Assert.True(handler.CanHandle(CreateMessageUpdate("+375333057384")));
        Assert.True(handler.CanHandle(CreateMessageUpdate("/start")));
        Assert.False(handler.CanHandle(CreateMessageUpdate("привет")));
    }

    private static Update CreateMessageUpdate(string text)
    {
        return new Update
        {
            Message = new Message { Text = text, Chat = new Chat { Id = 123 } }
        };
    }
}
