using Atlas.Application.Common;
using Atlas.Application.Features.IncidentCategories;
using Atlas.Application.Features.Menu;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreSolution.Integration.Tests.AtlasTests;

[Collection(nameof(IntegrationCollection))]
public sealed class ProblemCategoryTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task ShowMenu_ReturnsCategoriesFromDatabase()
    {
        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new ShowMenuMessage(8301, 1));

        Assert.True(result.IsSuccess);

        var buttons = result.Value!.ButtonRows!
            .SelectMany(row => row.Buttons)
            .ToList();

        Assert.Equal(3, buttons.Count);
        Assert.Contains(buttons, button => button.Text == IncidentCategoryConstants.Hardware.Name);
        Assert.Contains(buttons, button => button.Text == IncidentCategoryConstants.Software.Name);
        Assert.Contains(buttons, button => button.Text == IncidentCategoryConstants.Other.Name);
        Assert.All(buttons, button => Assert.StartsWith(CallbackConstants.CategoryPrefix, button.CallbackData));
    }

    [Fact]
    public async Task SelectProblemCategory_StoresChoiceAndReportsSelectedCategory()
    {
        const long telegramUserId = 8302;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .SaveSession(UserSession.Create(telegramUserId, nameof(WorkflowStep.ProblemCategorySelection)), CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var draftStore = scope.ServiceProvider.GetRequiredService<Atlas.Domain.Interfaces.IDraftStore>();

        var categoryId = IncidentCategoryConstants.Hardware.Id;

        var result = await mediator.Send(new SelectProblemCategoryMessage(telegramUserId, 2, categoryId));

        Assert.True(result.IsSuccess);
        Assert.Contains(IncidentCategoryConstants.Hardware.Name, result.Value!.Text);

        var draft = await draftStore.GetAsync(telegramUserId);

        Assert.NotNull(draft);
        Assert.Equal(categoryId, draft!.CategoryId);
        Assert.Equal(WorkflowStep.CategorySelected, draft.Step);
    }

    [Fact]
    public async Task SelectProblemCategory_UnknownCategory_ReturnsFailure()
    {
        const long telegramUserId = 8303;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .SaveSession(UserSession.Create(telegramUserId, nameof(WorkflowStep.ProblemCategorySelection)), CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SelectProblemCategoryMessage(telegramUserId, 3, Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(ErrorCode.IncidentCategoryNotFound), result.ErrorCode);
    }
}
