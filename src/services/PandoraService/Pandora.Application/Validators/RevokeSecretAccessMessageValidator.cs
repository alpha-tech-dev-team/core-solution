using FluentValidation;
using Pandora.Application.Features.Permissions;
using Pandora.Domain.Constants;

namespace Pandora.Application.Validators;

public sealed class RevokeSecretAccessMessageValidator : AbstractValidator<RevokeSecretAccessMessage>
{
    public RevokeSecretAccessMessageValidator()
    {
        RuleFor(message => message.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive);

        RuleFor(message => message.ItemId)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.VaultItemIdMaxLength);
    }
}
