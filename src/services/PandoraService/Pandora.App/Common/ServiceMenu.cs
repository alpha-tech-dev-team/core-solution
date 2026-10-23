using CoreSolution.Shared.Models;
using Pandora.Core.Constants.Localization;
using Pandora.Core.Enums;
using Pandora.Core.Interfaces;

namespace Pandora.App.Common;

internal static class ServiceMenu
{
    public static TelegramMessageContent Create(IReadOnlyList<string> roles, ILocalizer localizer)
    {
        var buttonRows = roles
            .Select(role => new TelegramButtonRow(new TelegramButton(role, CallbackConstants.ServicePrefix + role)))
            .ToList();

        return new TelegramMessageContent
        {
            Text = localizer.GetPhrase(WorkflowStep.ServiceSelection, LocalizationKeysConstants.ServiceSelection.Text),
            ButtonRows = buttonRows
        };
    }
}
