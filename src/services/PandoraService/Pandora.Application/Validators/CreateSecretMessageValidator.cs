using FluentValidation;
using Pandora.Application.Features.Secrets.CreateSecret;
using Pandora.Domain.Constants;

namespace Pandora.Application.Validators;

public class CreateSecretMessageValidator : AbstractValidator<CreateSecretMessage>
{
    public CreateSecretMessageValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.ResourceNameMaxLength);

        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.UsernameMaxLength);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.PasswordMaxLength);
    }
}
