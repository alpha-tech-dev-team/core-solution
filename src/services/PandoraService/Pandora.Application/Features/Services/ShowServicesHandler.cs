using Pandora.Core.Enums;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Common;
using Pandora.Core.Constants.Localization;
using CoreSolution.Shared.Abstractions;
using Pandora.Core.Interfaces;

namespace Pandora.Application.Features.Services;

public sealed class ShowServicesHandler : IRequestHandler<ShowServicesMessage, Result<WorkflowResponse>>
{
    private readonly ISecretStore _secretStore;
    private readonly ILocalizer _localizer;

    public ShowServicesHandler(ISecretStore secretStore, ILocalizer localizer)
    {
        _secretStore = secretStore;
        _localizer = localizer;
    }

    public Task<Result<WorkflowResponse>> Handle(ShowServicesMessage request, CancellationToken cancellationToken)
    {
        var services = _secretStore.GetServiceNames();

        if (services.Count == 0)
        {
            return Task.FromResult(Result<WorkflowResponse>.Success(new WorkflowResponse
            {
                Text = _localizer.GetPhrase(nameof(WorkflowStep.Idle), LocalizationKeysConstants.Idle.NoServices)
            }));
        }

        return Task.FromResult(Result<WorkflowResponse>.Success(ServiceMenu.Create(services, _localizer)));
    }
}
