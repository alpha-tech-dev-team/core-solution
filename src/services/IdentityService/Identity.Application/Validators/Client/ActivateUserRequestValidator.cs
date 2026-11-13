using FluentValidation;
using Identity.Application.Constants;
using Identity.Application.Contracts.Client;

namespace Identity.Application.Validators.Client;

public class ActivateUserRequestValidator : AbstractValidator<ActivateUserRequest>
{
    public ActivateUserRequestValidator()
    {
        RuleFor(x => x.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(ValidationConstants.PhoneMaxLength)
            .Matches(ValidationConstants.PhonePattern);
    }
}
