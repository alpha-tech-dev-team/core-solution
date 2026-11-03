using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using CoreSolution.Shared.Results;
using MediatR;
using Pandora.Application.Common;
using Pandora.Domain.Constants.Localization;
using Pandora.Domain.Enums;
using Pandora.Domain.Interfaces;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Services;

public sealed class ShowServicesHandler : IRequestHandler<ShowServicesMessage, Result<WorkflowResponse>>
{
    private readonly ISecretCatalog _secretCatalog;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ILocalizer _localizer;

    public ShowServicesHandler(ISecretCatalog secretCatalog, IPermissionRepository permissionRepository, ILocalizer localizer)
    {
        _secretCatalog = secretCatalog;
        _permissionRepository = permissionRepository;
        _localizer = localizer;
    }

    public async Task<Result<WorkflowResponse>> Handle(ShowServicesMessage request, CancellationToken cancellationToken)
    {
        var allowedItemIds = await _permissionRepository.GetAllowedItemIds(request.TelegramUserId, cancellationToken);
        var items = await _secretCatalog.GetItems(cancellationToken);

        IReadOnlyList<SecretItem> services = items
            .Where(item => allowedItemIds.Contains(item.Id))
            .ToList();

        if (services.Count == 0)
        {
            return Result<WorkflowResponse>.Success(new WorkflowResponse
            {
                Text = _localizer.GetPhrase(nameof(WorkflowStep.Idle), LocalizationKeysConstants.Idle.NoServices)
            });
        }

        return Result<WorkflowResponse>.Success(ServiceMenu.Create(services, _localizer));
    }
}
