using FluentValidation;
using Identity.Application.Constants;
using Identity.Application.Contracts;

namespace Identity.Application.Validators;

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
