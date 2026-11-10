using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Application.Behaviors;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Pandora.Application.Behaviors;
using Pandora.Application.Common.Services;
using Pandora.Application.Features.Credentials;
using Pandora.Application.Features.Services;
using Pandora.Domain.Constants;
using Pandora.Domain.Entities;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;
using Xunit;

namespace CoreSolution.Integration.Tests;

public sealed class UserActivityBehaviorTests
{
    private const long TelegramUserId = 8301;
    private const int MessageId = 1;
    private const string ItemId = "TestService";

    [Fact]
    public async Task Handle_UserActionSucceeds_WritesEntryAndReturnsResponse()
    {
        var repository = new RecordingAuditRepository();
        var behavior = CreateBehavior(repository);
        var request = new GetCredentialsMessage(TelegramUserId, MessageId, ItemId);
        var response = Result<WorkflowResponse>.Success(new WorkflowResponse { Text = "ok" });

        var result = await behavior.Handle(request, _ => Task.FromResult(response), CancellationToken.None);

        Assert.Same(response, result);

        var entry = Assert.Single(repository.Entries);
        Assert.Equal(TelegramUserId, entry.TelegramUserId);
        Assert.Equal(AuditConstants.Actions.GetCredentials, entry.Action);
        Assert.Equal(ItemId, entry.TargetItemId);
        Assert.Equal(AuditConstants.Sources.Telegram, entry.RequestSource);
        Assert.NotEqual(default, entry.Timestamp);
    }

    [Fact]
    public async Task Handle_UserActionFails_WritesNothing()
    {
        var repository = new RecordingAuditRepository();
        var response = Result<WorkflowResponse>.Failure("AccessDenied");

        await CreateBehavior(repository).Handle(
            new GetCredentialsMessage(TelegramUserId, MessageId, ItemId),
            _ => Task.FromResult(response),
            CancellationToken.None);

        Assert.Empty(repository.Entries);
    }

    [Fact]
    public async Task Handle_AuditWriteFails_ReturnsHandlerResponse()
    {
        var response = Result<WorkflowResponse>.Success(new WorkflowResponse { Text = "ok" });

        var result = await CreateBehavior(new FailingAuditRepository()).Handle(
            new GetCredentialsMessage(TelegramUserId, MessageId, ItemId),
            _ => Task.FromResult(response),
            CancellationToken.None);

        Assert.Same(response, result);
    }

    [Fact]
    public async Task ActivityBehaviorBeforeSessionBehavior_AuditsActionThatNeverRan()
    {
        var repository = new RecordingAuditRepository();
        var mediator = BuildMediator(repository, activityFirst: true);

        var result = await mediator.Send(new GetCredentialsMessage(TelegramUserId, MessageId, ItemId));

        Assert.True(result.Value!.RequestContact);
        Assert.Single(repository.Entries);
    }

    [Fact]
    public async Task SessionBehaviorBeforeActivityBehavior_PhoneRequestIsNotAudited()
    {
        var repository = new RecordingAuditRepository();
        var mediator = BuildMediator(repository, activityFirst: false);

        var result = await mediator.Send(new GetCredentialsMessage(TelegramUserId, MessageId, ItemId));

        Assert.True(result.Value!.RequestContact);
        Assert.Empty(repository.Entries);
    }

    private static UserActivityBehavior<GetCredentialsMessage, Result<WorkflowResponse>> CreateBehavior(IAuditRepository repository)
    {
        var auditLogger = new UserActivityAuditLogger(repository, NullLogger<UserActivityAuditLogger>.Instance);

        return new UserActivityBehavior<GetCredentialsMessage, Result<WorkflowResponse>>(auditLogger);
    }

    private static IMediator BuildMediator(IAuditRepository repository, bool activityFirst)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ShowServicesHandler>());
        services.AddSingleton(new SessionBehaviorOptions(nameof(Pandora.Domain.Enums.WorkflowStep.ServiceSelection), PandoraConstants.ServiceName));
        services.AddSingleton(repository);
        services.AddSingleton(new UserActivityAuditLogger(repository, NullLogger<UserActivityAuditLogger>.Instance));
        services.AddScoped<ISessionService, MissingSessionService>();
        services.AddScoped<IIdentityClient, DenyingIdentityClient>();
        services.AddScoped<IRequestContext, EmptyRequestContext>();
        services.AddScoped<ISecretCatalog, EmptySecretCatalog>();
        services.AddScoped<IPermissionRepository, EmptyPermissionRepository>();
        services.AddScoped<ILocalizer, EmptyLocalizer>();
        services.AddScoped<IPhoneConfirmationService, RejectingPhoneConfirmationService>();

        if (activityFirst)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UserActivityBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));
        }
        else
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SessionBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UserActivityBehavior<,>));
        }

        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    private sealed class RecordingAuditRepository : IAuditRepository
    {
        public List<AuditEntry> Entries { get; } = [];

        public Task Add(AuditEntry entry, CancellationToken cancellationToken)
        {
            Entries.Add(entry);

            return Task.CompletedTask;
        }
    }

    private sealed class FailingAuditRepository : IAuditRepository
    {
        public Task Add(AuditEntry entry, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("audit table is unavailable");
        }
    }

    private sealed class MissingSessionService : ISessionService
    {
        public Task<UserSession?> GetSession(long telegramUserId, CancellationToken cancellationToken) => Task.FromResult<UserSession?>(null);

        public Task SaveSession(UserSession session, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task DeleteSession(long telegramUserId, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class DenyingIdentityClient : IIdentityClient
    {
        public Task<bool> ActivateUser(long telegramUserId, string phone, CancellationToken cancellationToken) => Task.FromResult(false);

        public Task<bool> CheckUser(long telegramUserId, string access, CancellationToken cancellationToken) => Task.FromResult(false);
    }

    private sealed class EmptyRequestContext : IRequestContext
    {
        public UserSession Session { get; set; } = default!;
    }

    private sealed class EmptySecretCatalog : ISecretCatalog
    {
        public Task<Result<IReadOnlyList<SecretItem>>> GetItems(CancellationToken cancellationToken) =>
            Task.FromResult(Result<IReadOnlyList<SecretItem>>.Success([]));
    }

    private sealed class EmptyPermissionRepository : IPermissionRepository
    {
        public Task<IReadOnlyList<string>> GetAllowedItemIds(long telegramUserId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<string>>([]);

        public Task<bool> HasAccess(long telegramUserId, string itemId, CancellationToken cancellationToken) =>
            Task.FromResult(false);
    }

    private sealed class EmptyLocalizer : ILocalizer
    {
        public string GetPhrase(string state, string key) => key;

        public string GetPhrase(string state, string key, CoreSolution.Shared.Enums.Language language) => key;
    }

    private sealed class RejectingPhoneConfirmationService : IPhoneConfirmationService
    {
        public Task<bool> ConfirmAsync(long telegramUserId, string? phone, string initialStep, CancellationToken cancellationToken) =>
            Task.FromResult(false);
    }
}
