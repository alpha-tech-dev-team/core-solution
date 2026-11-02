using Pandora.Core.Enums;
using CoreSolution.Shared.Models;
using Pandora.Core.Constants.Localization;
using CoreSolution.Shared.Abstractions;
using Pandora.Core.Interfaces;

namespace Pandora.Application.Common;

internal static class ServiceMenu
{
    public static WorkflowResponse Create(IReadOnlyList<string> services, ILocalizer localizer)
    {
        var buttonRows = services
            .Select(service => new WorkflowButtonRow(new WorkflowButton(service, CallbackConstants.ServicePrefix + service)))
            .ToList();

        return new WorkflowResponse
        {
            Text = localizer.GetPhrase(nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.Text),
            ButtonRows = buttonRows
        };
    }
}
