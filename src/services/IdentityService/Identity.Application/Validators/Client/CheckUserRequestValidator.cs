using FluentValidation;
using Identity.Application.Constants;
using Identity.Application.Contracts.Client;

namespace Identity.Application.Validators.Client;

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
