using FluentValidation;
using Pandora.Application.Features.Secrets.UpdateSecret;
using Pandora.Domain.Constants;

namespace Pandora.Application.Validators;

public class UpdateSecretMessageValidator : AbstractValidator<UpdateSecretMessage>
{
    public UpdateSecretMessageValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.ResourceNameMaxLength);

        RuleFor(x => x.ItemId)
            .NotEmpty();

        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.UsernameMaxLength);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.PasswordMaxLength)
            .When(x => x.Password is not null);
    }
}
