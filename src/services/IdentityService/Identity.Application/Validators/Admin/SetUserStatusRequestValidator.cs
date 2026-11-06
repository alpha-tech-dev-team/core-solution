using FluentValidation;
using Identity.Application.Contracts.Admin;
using Identity.Domain.Enums;

namespace Identity.Application.Validators.Admin;

public class SetUserStatusRequestValidator : AbstractValidator<SetUserStatusRequest>
{
    public SetUserStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .Must(status => status is UserStatus.Active or UserStatus.Blocked);
    }
}
