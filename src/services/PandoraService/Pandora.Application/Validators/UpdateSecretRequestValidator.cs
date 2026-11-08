using FluentValidation;
using Pandora.Application.Contracts;
using Pandora.Domain.Constants;

namespace Pandora.Application.Validators;

public class UpdateSecretRequestValidator : AbstractValidator<UpdateSecretRequest>
{
    public UpdateSecretRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.ResourceNameMaxLength);

        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.UsernameMaxLength);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.PasswordMaxLength)
            .When(x => x.Password is not null);
    }
}
