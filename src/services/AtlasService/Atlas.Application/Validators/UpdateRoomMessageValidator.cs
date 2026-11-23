using Atlas.Application.Features.Rooms;
using Atlas.Domain.Constants;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class UpdateRoomMessageValidator : AbstractValidator<UpdateRoomMessage>
{
    public UpdateRoomMessageValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(RoomLimits.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(RoomLimits.DescriptionMaxLength)
            .When(x => x.Description is not null);
    }
}
