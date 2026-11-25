using Atlas.Application.Common;
using Atlas.Application.Features.Hardware;
using Atlas.Application.Features.Incidents;
using Atlas.Application.Features.Rooms;
using Atlas.Application.Features.Software;
using Atlas.Domain.Constants;
using Atlas.Domain.Entities;
using Atlas.Domain.Enums;
using Atlas.Domain.Interfaces.Repositories;
using Atlas.Domain.Interfaces.Services;
using Atlas.Domain.Models;
using CoreSolution.Shared.Abstractions;
using Identity.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreSolution.Integration.Tests.AtlasTests;

[Collection(nameof(IntegrationCollection))]
public sealed class IncidentCreationTests(IntegrationFixture fixture)
{
    private const long TelegramUserId = 8501;

    [Fact]
    public async Task CreateIncident_FromMenu_ShowsCategories()
    {
        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

        await sessionService.SaveSession(
            AtlasUserSession.Create(TelegramUserId, nameof(WorkflowStep.Menu)),
            CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new IncidentWorkflowMessage(TelegramUserId, 1, IncidentStepAction.Create));

        Assert.True(result.IsSuccess);

        var buttons = result.Value!.ButtonRows!
            .SelectMany(row => row.Buttons)
            .ToList();

        Assert.Contains(
            buttons,
            button => button.CallbackData.StartsWith(CallbackConstants.CategoryPrefix, StringComparison.Ordinal));

        var stored = await sessionService.GetSession(TelegramUserId, CancellationToken.None);

        Assert.Equal(nameof(WorkflowStep.ProblemCategorySelection), stored!.State);
    }

    [Fact]
    public async Task CreateIncident_WithConfirm_PersistsIncidentAndReportsTicket()
    {
        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(TelegramUserId, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Hardware.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var selected = await mediator.Send(new SelectRoomMessage(TelegramUserId, 1, RoomConstants.Office204.Id));

        Assert.True(selected.IsSuccess);

        var hardwareSelected = await mediator.Send(
            new SelectHardwareMessage(TelegramUserId, 2, HardwareConstants.Office204Computer.Id));

        Assert.True(hardwareSelected.IsSuccess);

        var described = await mediator.Send(
            new IncidentWorkflowMessage(TelegramUserId, 3, null, "Не включается компьютер"));

        Assert.True(described.IsSuccess);

        var confirmed = await mediator.Send(
            new IncidentWorkflowMessage(TelegramUserId, 4, IncidentStepAction.Confirm));

        Assert.True(confirmed.IsSuccess);
        Assert.Contains("№", confirmed.Value!.Text);

        var stored = await sessionService.GetSession(TelegramUserId, CancellationToken.None);

        Assert.Equal(nameof(WorkflowStep.IncidentCreated), stored!.State);
    }

    [Fact]
    public async Task CreateIncident_CreatesEmployeeOnFirstIncident()
    {
        const long newUser = 8502;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(newUser, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Hardware.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new SelectRoomMessage(newUser, 1, RoomConstants.Office204.Id));
        await mediator.Send(new SelectHardwareMessage(newUser, 2, HardwareConstants.Office204Computer.Id));
        await mediator.Send(new IncidentWorkflowMessage(newUser, 3, null, "Тестовый инцидент"));
        var result = await mediator.Send(new IncidentWorkflowMessage(newUser, 4, IncidentStepAction.Confirm));

        Assert.True(result.IsSuccess);

        var employees = scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();
        var employee = await employees.GetOrCreateByTelegramUserIdAsync(newUser, null, null, CancellationToken.None);

        Assert.Equal(newUser, employee.TelegramUserId);
        Assert.Equal(EmployeeRole.Employee, employee.Role);
    }

    [Fact]
    public async Task CreateIncident_TakesEmployeeProfileFromIdentity()
    {
        const long identifiedUser = 8506;
        const string phone = "+79001234502";

        await fixture.SeedPendingUserAsync(phone);
        await fixture.SetTelegramUserAsync(phone, identifiedUser, UserStatus.Active);

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(identifiedUser, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Hardware.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new SelectRoomMessage(identifiedUser, 1, RoomConstants.Office204.Id));
        await mediator.Send(new SelectHardwareMessage(identifiedUser, 2, HardwareConstants.Office204Computer.Id));
        await mediator.Send(new IncidentWorkflowMessage(identifiedUser, 3, null, "Не включается компьютер"));
        var result = await mediator.Send(new IncidentWorkflowMessage(
            identifiedUser,
            4,
            IncidentStepAction.Confirm,
            EmployeeName: "Telegram",
            EmployeeSurname: "Профиль"));

        Assert.True(result.IsSuccess);

        var employees = scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();
        var employee = await employees.GetOrCreateByTelegramUserIdAsync(identifiedUser, null, null, CancellationToken.None);

        Assert.Equal("Integration", employee.Name);
        Assert.Equal("Test", employee.Surname);
    }

    [Fact]
    public async Task CreateIncident_Confirm_NotifiesAuthor()
    {
        const long telegramUserId = 8507;

        var notifications = new RecordingIncidentNotifier();

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync(
            configureServices: services => services.AddSingleton<IIncidentNotifier>(notifications));
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Hardware.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new SelectRoomMessage(telegramUserId, 1, RoomConstants.Office204.Id));
        await mediator.Send(new SelectHardwareMessage(telegramUserId, 2, HardwareConstants.Office204Computer.Id));
        await mediator.Send(new IncidentWorkflowMessage(telegramUserId, 3, null, "Не включается компьютер"));
        var confirmed = await mediator.Send(new IncidentWorkflowMessage(telegramUserId, 4, IncidentStepAction.Confirm));

        Assert.True(confirmed.IsSuccess);

        var alert = Assert.Single(notifications.Alerts, item => item.ChatId == telegramUserId);
        Assert.Contains("создана", alert.Text);
    }

    [Fact]
    public async Task CreateIncident_NotifyFails_StillCreatesIncident()
    {
        const long telegramUserId = 8508;

        var notifications = new RecordingIncidentNotifier(failingChatId: telegramUserId);

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync(
            configureServices: services => services.AddSingleton<IIncidentNotifier>(notifications));
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Hardware.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new SelectRoomMessage(telegramUserId, 1, RoomConstants.Office204.Id));
        await mediator.Send(new SelectHardwareMessage(telegramUserId, 2, HardwareConstants.Office204Computer.Id));
        await mediator.Send(new IncidentWorkflowMessage(telegramUserId, 3, null, "Не включается компьютер"));
        var confirmed = await mediator.Send(new IncidentWorkflowMessage(telegramUserId, 4, IncidentStepAction.Confirm));

        Assert.True(confirmed.IsSuccess);
        Assert.Contains(telegramUserId, notifications.AttemptedChatIds);
        Assert.Empty(notifications.Alerts);
    }

    [Fact]
    public async Task CreateIncident_Cancel_ReturnsToMenu()
    {
        const long cancellingUser = 8503;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(cancellingUser, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Hardware.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new SelectRoomMessage(cancellingUser, 1, RoomConstants.Office204.Id));
        await mediator.Send(new SelectHardwareMessage(cancellingUser, 2, HardwareConstants.Office204Computer.Id));
        await mediator.Send(new IncidentWorkflowMessage(cancellingUser, 3, null, "Тестовый инцидент"));
        var cancelled = await mediator.Send(new IncidentWorkflowMessage(cancellingUser, 4, IncidentStepAction.Cancel));

        Assert.True(cancelled.IsSuccess);

        var stored = await sessionService.GetSession(cancellingUser, CancellationToken.None);

        Assert.Equal(nameof(WorkflowStep.Menu), stored!.State);
    }

    [Fact]
    public async Task CreateIncident_EmptyDescription_ReturnsFailure()
    {
        const long invalidUser = 8504;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(invalidUser, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Hardware.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new SelectRoomMessage(invalidUser, 1, RoomConstants.Office204.Id));
        await mediator.Send(new SelectHardwareMessage(invalidUser, 2, HardwareConstants.Office204Computer.Id));
        var result = await mediator.Send(new IncidentWorkflowMessage(invalidUser, 3, null, "   "));

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(ErrorCode.InvalidIncidentDescription), result.ErrorCode);
    }

    [Fact]
    public async Task CreateIncident_InvalidRequest_IsRejectedByValidationBehavior()
    {
        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new IncidentWorkflowMessage(0, 4, IncidentStepAction.Create));

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(ErrorCode.InvalidTelegramUserId), result.ErrorCode);
    }

    [Fact]
    public async Task SelectRoom_HardwareCategory_ShowsOnlyHardwareOfSelectedRoom()
    {
        const long telegramUserId = 8510;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Hardware.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SelectRoomMessage(telegramUserId, 1, RoomConstants.Office204.Id));

        Assert.True(result.IsSuccess);

        var callbacks = result.Value!.ButtonRows!
            .SelectMany(row => row.Buttons)
            .Select(button => button.CallbackData)
            .ToList();

        Assert.Contains(CallbackConstants.HardwarePrefix + HardwareConstants.Office204Computer.Id, callbacks);
        Assert.Contains(CallbackConstants.HardwarePrefix + HardwareConstants.Office204Printer.Id, callbacks);
        Assert.DoesNotContain(CallbackConstants.HardwarePrefix + HardwareConstants.Office305Computer.Id, callbacks);

        var stored = await sessionService.GetSession(telegramUserId, CancellationToken.None);

        Assert.Equal(nameof(WorkflowStep.HardwareSelection), stored!.State);
    }

    [Fact]
    public async Task SelectRoom_SoftwareCategory_ShowsGlobalSoftwareList()
    {
        const long telegramUserId = 8511;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Software.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SelectRoomMessage(telegramUserId, 1, RoomConstants.Office305.Id));

        Assert.True(result.IsSuccess);

        var callbacks = result.Value!.ButtonRows!
            .SelectMany(row => row.Buttons)
            .Select(button => button.CallbackData)
            .ToList();

        Assert.Contains(CallbackConstants.SoftwarePrefix + SoftwareConstants.Accounting.Id, callbacks);
        Assert.Contains(CallbackConstants.SoftwarePrefix + SoftwareConstants.OfficeSuite.Id, callbacks);

        var stored = await sessionService.GetSession(telegramUserId, CancellationToken.None);

        Assert.Equal(nameof(WorkflowStep.SoftwareSelection), stored!.State);
    }

    [Fact]
    public async Task SelectRoom_OtherCategory_GoesStraightToDescription()
    {
        const long telegramUserId = 8512;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Other.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new SelectRoomMessage(telegramUserId, 1, RoomConstants.Office101.Id));

        Assert.True(result.IsSuccess);

        var stored = await sessionService.GetSession(telegramUserId, CancellationToken.None);

        Assert.Equal(nameof(WorkflowStep.IncidentDescription), stored!.State);
    }

    [Fact]
    public async Task SelectHardware_FromAnotherRoom_ReturnsFailure()
    {
        const long telegramUserId = 8513;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.HardwareSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Hardware.Id;
        session.RoomId = RoomConstants.Office204.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(
            new SelectHardwareMessage(telegramUserId, 1, HardwareConstants.Office305Computer.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(ErrorCode.HardwareNotFound), result.ErrorCode);
    }

    [Fact]
    public async Task SelectSoftware_WhenCategoryIsHardware_ReturnsFailure()
    {
        const long telegramUserId = 8514;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.SoftwareSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Hardware.Id;
        session.RoomId = RoomConstants.Office204.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(
            new SelectSoftwareMessage(telegramUserId, 1, SoftwareConstants.Accounting.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal(nameof(ErrorCode.IncidentContextIncomplete), result.ErrorCode);
    }

    [Fact]
    public async Task CreateIncident_HardwareCategory_PersistsSelectedHardware()
    {
        const long telegramUserId = 8515;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Hardware.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new SelectRoomMessage(telegramUserId, 1, RoomConstants.Office204.Id));
        await mediator.Send(new SelectHardwareMessage(telegramUserId, 2, HardwareConstants.Office204Printer.Id));
        await mediator.Send(new IncidentWorkflowMessage(telegramUserId, 3, null, "Замяло бумагу"));
        var confirmed = await mediator.Send(
            new IncidentWorkflowMessage(telegramUserId, 4, IncidentStepAction.Confirm));

        Assert.True(confirmed.IsSuccess);

        var incidents = scope.ServiceProvider.GetRequiredService<IIncidentRepository>();
        var page = await incidents.GetPageAsync(
            new PageRequest(PaginationLimits.FirstPage, PaginationLimits.DefaultPageSize),
            query => query.OrderByDescending(incident => incident.CreatedAt),
            CancellationToken.None);

        var stored = Assert.Single(
            page.Items,
            incident => incident.HardwareId == HardwareConstants.Office204Printer.Id);

        Assert.Null(stored.SoftwareId);
        Assert.Equal(RoomConstants.Office204.Id, stored.RoomId);
    }

    [Fact]
    public async Task CreateIncident_SoftwareCategory_PersistsSelectedSoftware()
    {
        const long telegramUserId = 8516;

        await using var atlas = await fixture.CreateAtlasServiceProviderAsync();
        using var scope = atlas.CreateScope();

        var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();
        var session = AtlasUserSession.Create(telegramUserId, nameof(WorkflowStep.RoomSelection));
        session.IncidentCategoryId = IncidentCategoryConstants.Software.Id;

        await sessionService.SaveSession(session, CancellationToken.None);

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new SelectRoomMessage(telegramUserId, 1, RoomConstants.Office305.Id));
        await mediator.Send(new SelectSoftwareMessage(telegramUserId, 2, SoftwareConstants.Accounting.Id));
        await mediator.Send(new IncidentWorkflowMessage(telegramUserId, 3, null, "Не запускается программа"));
        var confirmed = await mediator.Send(
            new IncidentWorkflowMessage(telegramUserId, 4, IncidentStepAction.Confirm));

        Assert.True(confirmed.IsSuccess);

        var incidents = scope.ServiceProvider.GetRequiredService<IIncidentRepository>();
        var page = await incidents.GetPageAsync(
            new PageRequest(PaginationLimits.FirstPage, PaginationLimits.DefaultPageSize),
            query => query.OrderByDescending(incident => incident.CreatedAt),
            CancellationToken.None);

        var stored = Assert.Single(
            page.Items,
            incident => incident.SoftwareId == SoftwareConstants.Accounting.Id);

        Assert.Null(stored.HardwareId);
    }
}
