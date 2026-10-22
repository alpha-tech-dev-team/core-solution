using CoreSolution.Shared.Models;
using Pandora.App.Features.Start;

namespace Pandora.App.Common;

internal static class ServiceMenu
{
    public static TelegramMessageContent Create(IReadOnlyList<string> roles)
    {
        var buttonRows = roles
            .Select(role => new TelegramButtonRow(new TelegramButton(role, CallbackConstants.ServicePrefix + role)))
            .ToList();

        return new TelegramMessageContent
        {
            Text = StartConstants.ServicesText,
            ButtonRows = buttonRows
        };
    }
}
