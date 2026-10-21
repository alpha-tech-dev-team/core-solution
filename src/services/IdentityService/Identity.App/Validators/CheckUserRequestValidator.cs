using FluentValidation;
using Identity.App.Contracts;

namespace Identity.App.Validators;

public class CheckUserRequestValidator : AbstractValidator<CheckUserRequest>
{
    public CheckUserRequestValidator()
    {
        RuleFor(x => x.TelegramUserId)
            .GreaterThan(0);

        RuleFor(x => x.Access)
            .NotEmpty()
            .MaximumLength(50);
    }
}
