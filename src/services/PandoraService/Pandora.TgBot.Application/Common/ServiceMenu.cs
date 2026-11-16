using CoreSolution.Shared.Abstractions;
using CoreSolution.Shared.Models;
using Pandora.Domain.Constants.Localization;
using Pandora.Domain.Enums;
using Pandora.Domain.Models;

namespace Pandora.TgBot.Application.Common;

internal static class ServiceMenu
{
    public static WorkflowResponse Create(IReadOnlyList<SecretItem> items, ILocalizer localizer)
    {
        var buttonRows = items
            .Select(item => new WorkflowButtonRow(new WorkflowButton(item.Name, CallbackConstants.ServicePrefix + item.Id)))
            .ToList();

        return new WorkflowResponse
        {
            Text = localizer.GetPhrase(nameof(WorkflowStep.ServiceSelection), LocalizationKeysConstants.ServiceSelection.Text),
            ButtonRows = buttonRows
        };
    }
}
