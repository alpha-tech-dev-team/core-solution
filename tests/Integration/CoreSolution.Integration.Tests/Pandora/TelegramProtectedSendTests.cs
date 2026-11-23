using System.Net;
using System.Text;
using System.Text.Json;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

public sealed class TelegramProtectedSendTests
{
    private const long ChatId = 9301;
    private const string BotToken = "123456:protected-send-test-token";

    [Fact]
    public async Task SendMessageAsync_WithProtectionAndSpoiler_SendsProtectContentAndEntities()
    {
        var handler = new RecordingTelegramHandler();
        var service = CreateService(handler);
        var password = "s3cret-pass";
        var text = $"Пароль: {password}";
        var options = new TelegramSendOptions(
            ProtectContent: true,
            Entities: new[]
            {
                new MessageEntity
                {
                    Type = MessageEntityType.Spoiler,
                    Offset = text.Length - password.Length,
                    Length = password.Length
                }
            });

        await service.SendMessageAsync(ChatId, text, null, CancellationToken.None, options);

        var request = Assert.Single(handler.Requests);

        Assert.True(request.GetProperty("protect_content").GetBoolean());

        var entity = Assert.Single(request.GetProperty("entities").EnumerateArray());

        Assert.Equal("spoiler", entity.GetProperty("type").GetString());
        Assert.Equal(text.Length - password.Length, entity.GetProperty("offset").GetInt32());
        Assert.Equal(password.Length, entity.GetProperty("length").GetInt32());
        Assert.Equal(text, request.GetProperty("text").GetString());
    }

    [Fact]
    public async Task SendMessageAsync_WithoutOptions_SendsNeitherProtectionNorEntities()
    {
        var handler = new RecordingTelegramHandler();
        var service = CreateService(handler);

        await service.SendMessageAsync(ChatId, "Обычное сообщение", cancellationToken: CancellationToken.None);

        var request = Assert.Single(handler.Requests);

        Assert.False(request.TryGetProperty("protect_content", out var protect) && protect.GetBoolean());
        Assert.False(request.TryGetProperty("entities", out _));
    }

    [Fact]
    public async Task SendMessageAsync_WithLongMaskedText_SplitsWithoutCuttingSpoiler()
    {
        var handler = new RecordingTelegramHandler();
        var service = CreateService(handler);
        var filler = new string('a', 4090);
        var password = new string('p', 40);
        var text = $"{filler}\n{password}";
        var options = new TelegramSendOptions(
            ProtectContent: true,
            Entities: new[]
            {
                new MessageEntity
                {
                    Type = MessageEntityType.Spoiler,
                    Offset = filler.Length + 1,
                    Length = password.Length
                }
            });

        await service.SendMessageAsync(ChatId, text, null, CancellationToken.None, options);

        Assert.Equal(2, handler.Requests.Count);

        var first = handler.Requests[0];
        var second = handler.Requests[1];

        Assert.Equal($"{filler}\n", first.GetProperty("text").GetString());
        Assert.False(first.TryGetProperty("entities", out var firstEntities) && firstEntities.GetArrayLength() > 0);

        var entity = Assert.Single(second.GetProperty("entities").EnumerateArray());

        Assert.Equal(password, second.GetProperty("text").GetString());
        Assert.Equal("spoiler", entity.GetProperty("type").GetString());
        Assert.Equal(0, entity.GetProperty("offset").GetInt32());
        Assert.Equal(password.Length, entity.GetProperty("length").GetInt32());
        Assert.True(first.GetProperty("protect_content").GetBoolean());
        Assert.True(second.GetProperty("protect_content").GetBoolean());
    }

    private static ITelegramNotificationService CreateService(RecordingTelegramHandler handler)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddTelegramBot(BotToken);
        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(BotToken, new HttpClient(handler)));

        return services.BuildServiceProvider().GetRequiredService<ITelegramNotificationService>();
    }

    private sealed class RecordingTelegramHandler : HttpMessageHandler
    {
        public List<JsonElement> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);

            Requests.Add(JsonDocument.Parse(body).RootElement.Clone());

            var response = JsonSerializer.Serialize(new
            {
                ok = true,
                result = new
                {
                    message_id = 1,
                    date = 0,
                    chat = new { id = ChatId, type = "private" }
                }
            });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json")
            };
        }
    }
}
