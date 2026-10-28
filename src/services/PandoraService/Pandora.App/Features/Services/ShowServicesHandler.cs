using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.App.Common;
using Pandora.Core.Constants.Localization;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.App.Features.Services;

public sealed class ShowServicesHandler : IRequestHandler<ShowServicesMessage, Result<TelegramMessageContent>>
{
    private readonly ISecretStore _secretStore;
    private readonly ILocalizer _localizer;

    public ShowServicesHandler(ISecretStore secretStore, ILocalizer localizer)
    {
        _secretStore = secretStore;
        _localizer = localizer;
    }

    public Task<Result<TelegramMessageContent>> Handle(ShowServicesMessage request, CancellationToken cancellationToken)
    {
        var services = _secretStore.GetServiceNames();

        if (services.Count == 0)
        {
            return Task.FromResult(Result<TelegramMessageContent>.Success(new TelegramMessageContent
            {
                Text = _localizer.GetPhrase(WorkflowStep.Idle, LocalizationKeysConstants.Idle.NoServices)
            }));
        }

        return Task.FromResult(Result<TelegramMessageContent>.Success(ServiceMenu.Create(services, _localizer)));
    }
}
