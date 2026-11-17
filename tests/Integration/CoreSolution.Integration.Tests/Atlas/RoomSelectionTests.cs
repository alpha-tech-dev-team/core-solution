using Atlas.Application.Common;
using Atlas.Application.Features.Rooms;
using Atlas.Domain.Constants;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreSolution.Integration.Tests.AtlasTests;

[Collection(nameof(IntegrationCollection))]
public sealed class RoomSelectionTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task ShowRooms_ReturnsRoomsFromDatabase()
    {
        const long telegramUserId = 8401;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .SaveSession(AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection)), CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new ShowRoomsMessage(telegramUserId, 1, PaginationLimits.FirstPage));

        Assert.True(result.IsSuccess);

        var buttons = result.Value!.ButtonRows!
            .SelectMany(row => row.Buttons)
            .ToList();

        var roomButtons = buttons
            .Where(button => button.CallbackData.StartsWith(CallbackConstants.RoomPrefix, StringComparison.Ordinal))
            .ToList();

        Assert.Equal(PaginationLimits.RoomsPageSize, roomButtons.Count);
        Assert.Contains(roomButtons, button => button.Text == RoomConstants.Office101.Name);
        Assert.Contains(roomButtons, button => button.Text == RoomConstants.Office204.Name);
        Assert.Contains(roomButtons, button => button.Text == RoomConstants.Office305.Name);
        Assert.Contains(roomButtons, button => button.Text == RoomConstants.Office401.Name);
        Assert.Contains(roomButtons, button => button.Text == RoomConstants.Office402.Name);
        Assert.All(roomButtons, button => Assert.StartsWith(CallbackConstants.RoomPrefix, button.CallbackData));
        Assert.Contains(buttons, button => button.CallbackData.StartsWith(CallbackConstants.RoomPagePrefix, StringComparison.Ordinal));
    }

    [Fact]
    public async Task SelectRoom_StoresChoiceAndReportsSelectedRoom()
    {
        const long telegramUserId = 8402;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

        await sessionService.SaveSession(
            AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection)),
            CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var roomId = RoomConstants.Office204.Id;

        var result = await mediator.Send(new SelectRoomMessage(telegramUserId, 2, roomId));

        Assert.True(result.IsSuccess);
        Assert.Contains(RoomConstants.Office204.Name, result.Value!.Text);
        Assert.NotEmpty(result.Value.ButtonRows!);

        var session = await sessionService.GetSession(telegramUserId, CancellationToken.None);

        var atlasSession = Assert.IsType<AtlasUserSession>(session);
        Assert.Equal(roomId, atlasSession.RoomId);
        Assert.Equal(nameof(WorkflowStep.RoomSelected), atlasSession.State);
    }

    [Fact]
    public async Task SelectRoom_UnknownRoom_ReturnsFailure()
    {
        const long telegramUserId = 8403;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .SaveSession(AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection)), CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SelectRoomMessage(telegramUserId, 3, Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(ErrorCode.RoomNotFound), result.ErrorCode);
    }

    [Fact]
    public async Task SelectRoom_InvalidRequest_IsRejectedByValidationBehavior()
    {
        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(
            new SelectRoomMessage(0, 4, RoomConstants.Office101.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(ErrorCode.InvalidTelegramUserId), result.ErrorCode);
    }

    [Fact]
    public async Task ShowRooms_ManyRooms_PaginatesRooms()
    {
        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var roomRepository = scope.ServiceProvider.GetRequiredService<IRoomRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .SaveSession(AtlasUserSession.Create(8404, nameof(WorkflowStep.RoomSelection)), CancellationToken.None);

        var added = new List<Room>();

        try
        {
            for (var index = 0; index < 25; index++)
            {
                var room = new Room
                {
                    Id = Guid.NewGuid(),
                    Name = $"Кабинет пагинация {index:00}",
                    CreatedAt = DateTime.UtcNow
                };

                roomRepository.Add(room);
                added.Add(room);
            }

            await roomRepository.SaveChangesAsync(CancellationToken.None);

            var firstPage = await mediator.Send(new ShowRoomsMessage(8404, 5, PaginationLimits.FirstPage));

            Assert.True(firstPage.IsSuccess);

            var firstPageButtons = firstPage.Value!.ButtonRows!
                .SelectMany(row => row.Buttons)
                .ToList();

            var roomButtons = firstPageButtons
                .Where(button => button.CallbackData.StartsWith(CallbackConstants.RoomPrefix, StringComparison.Ordinal))
                .ToList();

            Assert.Equal(PaginationLimits.RoomsPageSize, roomButtons.Count);

            var nextPageButton = Assert.Single(firstPageButtons, button => button.CallbackData.StartsWith(CallbackConstants.RoomPagePrefix, StringComparison.Ordinal));

            Assert.Equal(CallbackConstants.RoomPagePrefix + (PaginationLimits.FirstPage + 1), nextPageButton.CallbackData);

            var secondPage = await mediator.Send(new ShowRoomsMessage(8404, 6, PaginationLimits.FirstPage + 1));

            Assert.True(secondPage.IsSuccess);

            var secondPageButtons = secondPage.Value!.ButtonRows!
                .SelectMany(row => row.Buttons)
                .ToList();

            var secondPageRoomButtons = secondPageButtons
                .Where(button => button.CallbackData.StartsWith(CallbackConstants.RoomPrefix, StringComparison.Ordinal))
                .ToList();

            Assert.Equal(PaginationLimits.RoomsPageSize, secondPageRoomButtons.Count);
            Assert.Contains(secondPageButtons, button => button.CallbackData.StartsWith(CallbackConstants.RoomPagePrefix, StringComparison.Ordinal));
        }
        finally
        {
            foreach (var room in added)
            {
                roomRepository.Remove(room);
            }

            await roomRepository.SaveChangesAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task SelectRoomByText_RoomNumber_SelectsRoom()
    {
        const long telegramUserId = 8405;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

        await sessionService.SaveSession(
            AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection)),
            CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SelectRoomByTextMessage(telegramUserId, 7, "204", 42));

        Assert.True(result.IsSuccess);
        Assert.Contains(RoomConstants.Office204.Name, result.Value!.Text);

        var session = await sessionService.GetSession(telegramUserId, CancellationToken.None);

        var atlasSession = Assert.IsType<AtlasUserSession>(session);
        Assert.Equal(RoomConstants.Office204.Id, atlasSession.RoomId);
        Assert.Equal(nameof(WorkflowStep.RoomSelected), atlasSession.State);
    }

    [Fact]
    public async Task SelectRoomByText_UnknownText_KeepsRoomMenu()
    {
        const long telegramUserId = 8406;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .SaveSession(AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection)), CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SelectRoomByTextMessage(telegramUserId, 8, "кабинет", 42));

        Assert.True(result.IsSuccess);

        var buttons = result.Value!.ButtonRows!
            .SelectMany(row => row.Buttons)
            .ToList();

        var roomButtons = buttons
            .Where(button => button.CallbackData.StartsWith(CallbackConstants.RoomPrefix, StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(roomButtons);
        Assert.All(roomButtons, button => Assert.StartsWith(CallbackConstants.RoomPrefix, button.CallbackData));
    }

    [Fact]
    public async Task SelectRoomByText_UnknownTextWithoutMenu_ReturnsFailure()
    {
        const long telegramUserId = 8407;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ISessionService>()
            .SaveSession(AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection)), CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SelectRoomByTextMessage(telegramUserId, 9, "кабинет", null));

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(ErrorCode.RoomNotFound), result.ErrorCode);
    }
}
