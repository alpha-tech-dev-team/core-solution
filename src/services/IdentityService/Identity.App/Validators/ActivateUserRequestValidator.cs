using FluentValidation;
using Identity.App.Contracts;

namespace Identity.App.Validators;

public class ActivateUserRequestValidator : AbstractValidator<ActivateUserRequest>
{
    public ActivateUserRequestValidator()
    {
        RuleFor(x => x.TelegramUserId)
            .GreaterThan(0);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(20);
    }
}
