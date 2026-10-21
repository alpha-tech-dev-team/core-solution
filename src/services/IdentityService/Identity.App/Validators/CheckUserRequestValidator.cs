using FluentValidation;
using Identity.App.Constants;
using Identity.App.Contracts;

namespace Identity.App.Validators;

public class CheckUserRequestValidator : AbstractValidator<CheckUserRequest>
{
    public CheckUserRequestValidator()
    {
        RuleFor(x => x.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive);

        RuleFor(x => x.Access)
            .NotEmpty()
            .MaximumLength(ValidationConstants.AccessMaxLength);
    }
}
