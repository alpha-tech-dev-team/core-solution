using Atlas.Application.Features.Rooms;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Constants;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class GetRoomsMessageValidator : AbstractValidator<GetRoomsMessage>
{
    public GetRoomsMessageValidator()
    {
        RuleFor(message => message.Page)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(nameof(ErrorCode.ValidationFailed))
            .GreaterThanOrEqualTo(PaginationLimits.FirstPage)
            .WithErrorCode(nameof(ErrorCode.ValidationFailed));

        RuleFor(message => message.PageSize)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(nameof(ErrorCode.ValidationFailed))
            .InclusiveBetween(PaginationLimits.MinPageSize, PaginationLimits.MaxPageSize)
            .WithErrorCode(nameof(ErrorCode.ValidationFailed));
    }
}
