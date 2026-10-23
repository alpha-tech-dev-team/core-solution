using CoreSolution.Shared.Models;
using Pandora.Core.Constants.Localization;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.App.Common;

internal static class ServiceMenu
{
    public static TelegramMessageContent Create(IReadOnlyList<string> services, ILocalizer localizer)
    {
        var buttonRows = services
            .Select(service => new TelegramButtonRow(new TelegramButton(service, CallbackConstants.ServicePrefix + service)))
            .ToList();

        return new TelegramMessageContent
        {
            Text = localizer.GetPhrase(WorkflowStep.ServiceSelection, LocalizationKeysConstants.ServiceSelection.Text),
            ButtonRows = buttonRows
        };
    }
}
