using FluentValidation;
using Identity.App.Constants;
using Identity.App.Contracts;

namespace Identity.App.Validators;

public class ActivateUserRequestValidator : AbstractValidator<ActivateUserRequest>
{
    public ActivateUserRequestValidator()
    {
        RuleFor(x => x.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(ValidationConstants.PhoneMaxLength);
    }
}
