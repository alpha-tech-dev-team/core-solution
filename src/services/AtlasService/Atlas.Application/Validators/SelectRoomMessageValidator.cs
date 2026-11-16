using Atlas.Application.Features.Rooms;
using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class SelectRoomMessageValidator : AbstractValidator<SelectRoomMessage>
{
    public SelectRoomMessageValidator()
    {
        RuleFor(message => message.TelegramUserId)
            .GreaterThan(ValidationConstants.TelegramUserIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidTelegramUserId));

        RuleFor(message => message.MessageId)
            .GreaterThan(ValidationConstants.MessageIdMinExclusive)
            .WithErrorCode(nameof(ErrorCode.InvalidMessageId));

        RuleFor(message => message.RoomId)
            .NotEqual(Guid.Empty)
            .WithErrorCode(nameof(ErrorCode.InvalidRoomId));
    }
}
