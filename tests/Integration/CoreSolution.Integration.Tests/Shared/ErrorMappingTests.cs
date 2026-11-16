using CoreSolution.Shared.Constants;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using CoreSolution.Shared.TgBot.Abstractions;
using CoreSolution.Shared.TgBot.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreSolution.Integration.Tests.Shared;

[Collection(nameof(IntegrationCollection))]
public sealed class ErrorMappingTests(IntegrationFixture fixture)
{
    private const string AccessNotConfirmedText = "Доступ не подтверждён. Обратитесь к администратору.";
    private const string UserBlockedText = "Ваш аккаунт заблокирован. Обратитесь к администратору.";
    private const string UnexpectedErrorText = "Произошла ошибка. Попробуйте позже.";
    private const string SecretNotFoundText = "Учётные данные для этого сервиса не найдены. Обратитесь к администратору.";

    [Fact]
    public async Task GetUserText_AtlasKnownCode_ReturnsLocalizedText()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IUserErrorMapper>();

        var text = Result<WorkflowResponse>.Failure(ErrorCodes.AccessNotConfirmed).GetUserText(mapper);

        Assert.Equal(AccessNotConfirmedText, text);
    }

    [Fact]
    public async Task GetUserText_AtlasUnknownCode_ReturnsUnexpectedErrorText()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IUserErrorMapper>();

        var text = Result<WorkflowResponse>.Failure("SomethingStrange").GetUserText(mapper);

        Assert.Equal(UnexpectedErrorText, text);
    }

    [Fact]
    public async Task GetUserText_PandoraSecretNotFound_ReturnsLocalizedText()
    {
        await using var pandora = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = pandora.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IUserErrorMapper>();

        var text = Result<WorkflowResponse>.Failure("SecretNotFound").GetUserText(mapper);

        Assert.Equal(SecretNotFoundText, text);
    }

    [Fact]
    public async Task GetUserText_AtlasBlockedUser_ReturnsLocalizedText()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IUserErrorMapper>();

        var text = Result<WorkflowResponse>.Failure(ErrorCodes.UserBlocked).GetUserText(mapper);

        Assert.Equal(UserBlockedText, text);
    }

    [Fact]
    public async Task GetUserText_PandoraBlockedUser_ReturnsLocalizedText()
    {
        await using var pandora = await fixture.CreatePandoraServiceProviderAsync();
        using var scope = pandora.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IUserErrorMapper>();

        var text = Result<WorkflowResponse>.Failure(ErrorCodes.UserBlocked).GetUserText(mapper);

        Assert.Equal(UserBlockedText, text);
    }

    [Fact]
    public async Task GetUserText_SuccessResult_ReturnsResponseText()
    {
        await using var atlas = fixture.CreateAtlasServiceProvider();
        using var scope = atlas.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IUserErrorMapper>();
        var result = Result<WorkflowResponse>.Success(new WorkflowResponse { Text = "Меню:" });

        Assert.Equal("Меню:", result.GetUserText(mapper));
    }
}
