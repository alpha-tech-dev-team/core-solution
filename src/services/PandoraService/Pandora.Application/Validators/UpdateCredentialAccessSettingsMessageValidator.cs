using FluentValidation;
using Pandora.Application.Features.Settings.CredentialAccess;
using Pandora.Domain.Constants;

namespace Pandora.Application.Validators;

public sealed class UpdateCredentialAccessSettingsMessageValidator
    : AbstractValidator<UpdateCredentialAccessSettingsMessage>
{
    public UpdateCredentialAccessSettingsMessageValidator()
    {
        RuleFor(x => x.TtlMinutes)
            .Must(ttl => CredentialAccessConstants.AllowedTtlMinutes.Contains(ttl));
    }
}
