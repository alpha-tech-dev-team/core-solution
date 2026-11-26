using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Pandora.Domain.Enums;
using Pandora.TgBot.Credentials;
using Telegram.Bot;
using Telegram.Bot.Types;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

public sealed class TelegramCredentialMessageGatewayTests
{
    private const long ChatId = 9401;
    private const int MessageId = 1234;
    private const string BotToken = "123456:gateway-test-token";

    [Theory]
    [InlineData("Bad Request: message to delete not found", 400, CredentialMessageOutcome.NotFound)]
    [InlineData("Bad Request: message not found in unrelated context", 400, CredentialMessageOutcome.Failed)]
    [InlineData("Bad Request: message to delete not found", 500, CredentialMessageOutcome.Failed)]
    [InlineData("Bad Request: message to edit not found", 400, CredentialMessageOutcome.Failed)]
    public async Task Delete_ClassifiesOnlyExactBadRequestDescription(
        string description,
        int errorCode,
        CredentialMessageOutcome expected)
    {
        var gateway = CreateGateway(new ErrorResponseHandler(description, errorCode));

        var outcome = await gateway.Delete(ChatId, MessageId, CancellationToken.None);

        Assert.Equal(expected, outcome);
    }

    [Theory]
    [InlineData("Bad Request: message to edit not found", 400, CredentialMessageOutcome.NotFound)]
    [InlineData("Bad Request: message not found in unrelated context", 400, CredentialMessageOutcome.Failed)]
    [InlineData("Bad Request: message to edit not found", 500, CredentialMessageOutcome.Failed)]
    [InlineData("Bad Request: message to delete not found", 400, CredentialMessageOutcome.Failed)]
    public async Task ReplaceText_ClassifiesOnlyExactBadRequestDescription(
        string description,
        int errorCode,
        CredentialMessageOutcome expected)
    {
        var gateway = CreateGateway(new ErrorResponseHandler(description, errorCode));

        var outcome = await gateway.ReplaceText(ChatId, MessageId, "expiry notice", CancellationToken.None);

        Assert.Equal(expected, outcome);
    }

    [Fact]
    public async Task ReplaceText_WhenNotModified_ReturnsSucceeded()
    {
        var gateway = CreateGateway(new ErrorResponseHandler("Bad Request: message is not modified", 400));

        var outcome = await gateway.ReplaceText(ChatId, MessageId, "expiry notice", CancellationToken.None);

        Assert.Equal(CredentialMessageOutcome.Succeeded, outcome);
    }

    private static TelegramCredentialMessageGateway CreateGateway(HttpMessageHandler handler)
    {
        var client = new TelegramBotClient(BotToken, new HttpClient(handler));

        return new TelegramCredentialMessageGateway(
            client,
            NullLogger<TelegramCredentialMessageGateway>.Instance);
    }

    private sealed class ErrorResponseHandler(string description, int errorCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = JsonSerializer.Serialize(new
            {
                ok = false,
                error_code = errorCode,
                description
            });

            return Task.FromResult(new HttpResponseMessage((HttpStatusCode)errorCode)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json")
            });
        }
    }
}
