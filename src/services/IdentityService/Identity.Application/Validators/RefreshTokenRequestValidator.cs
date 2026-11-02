using FluentValidation;
using Identity.Application.Constants;
using Identity.Application.Contracts;

namespace Identity.Application.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MaximumLength(ValidationConstants.RefreshTokenMaxLength);
    }
}
