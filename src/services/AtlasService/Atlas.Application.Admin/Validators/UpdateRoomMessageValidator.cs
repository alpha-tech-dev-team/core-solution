using Atlas.Application.Features.Rooms;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class UpdateRoomMessageValidator : AbstractValidator<UpdateRoomMessage>
{
    public UpdateRoomMessageValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(nameof(ErrorCode.ValidationFailed))
            .MaximumLength(RoomLimits.NameMaxLength)
            .WithErrorCode(nameof(ErrorCode.ValidationFailed));

        RuleFor(x => x.Description)
            .MaximumLength(RoomLimits.DescriptionMaxLength)
            .WithErrorCode(nameof(ErrorCode.ValidationFailed))
            .When(x => x.Description is not null);
    }
}
