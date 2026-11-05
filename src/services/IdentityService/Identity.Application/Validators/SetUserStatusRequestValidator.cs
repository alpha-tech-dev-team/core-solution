using FluentValidation;
using Identity.Application.Contracts.Admin;
using Identity.Domain.Enums;

namespace Identity.Application.Validators;

public class SetUserStatusRequestValidator : AbstractValidator<SetUserStatusRequest>
{
    public SetUserStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => Enum.TryParse<UserStatus>(status, ignoreCase: true, out var parsed)
                && parsed is UserStatus.Active or UserStatus.Blocked);
    }
}
