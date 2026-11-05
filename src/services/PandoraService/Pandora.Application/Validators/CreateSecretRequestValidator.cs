using FluentValidation;
using Pandora.Application.Contracts;
using Pandora.Domain.Constants;

namespace Pandora.Application.Validators;

public class CreateSecretRequestValidator : AbstractValidator<CreateSecretRequest>
{
    public CreateSecretRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.ResourceNameMaxLength);

        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.UsernameMaxLength);

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
