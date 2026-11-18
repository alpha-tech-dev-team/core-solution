using FluentValidation;
using Pandora.Application.Features.Permissions;
using Pandora.Domain.Constants;

namespace Pandora.Application.Validators;

public sealed class GrantSecretAccessMessageValidator : AbstractValidator<GrantSecretAccessMessage>
{
    public GrantSecretAccessMessageValidator()
    {
        RuleFor(message => message.TelegramUserId)
            .GreaterThan(0);

        RuleFor(message => message.ItemId)
            .NotEmpty()
            .MaximumLength(SecretMetadataLimits.VaultItemIdMaxLength);
    }
}
