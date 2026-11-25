using System.Globalization;
using CoreSolution.Shared.Abstractions;
using Pandora.Domain.Constants.Localization;
using Pandora.Domain.Enums;

namespace Pandora.TgBot.Application.Features.Credentials;

public sealed class CredentialAccessNoticeBuilder
{
    private const string RequestedAtFormat = "dd.MM.yyyy HH:mm";

    private readonly ILocalizer _localizer;

    public CredentialAccessNoticeBuilder(ILocalizer localizer)
    {
        _localizer = localizer;
    }

    public string Build(string itemName, DateTime requestedAt)
    {
        var template = _localizer.GetPhrase(
            nameof(WorkflowStep.ServiceSelection),
            LocalizationKeysConstants.ServiceSelection.AccessExpired);

        return string.Format(
            template,
            itemName,
            requestedAt.ToString(RequestedAtFormat, CultureInfo.InvariantCulture));
    }
}
