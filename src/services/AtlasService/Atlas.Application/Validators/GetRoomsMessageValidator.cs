using Atlas.Application.Features.Rooms;
using CoreSolution.Shared.Application.Validation;
using FluentValidation;

namespace Atlas.Application.Validators;

public sealed class GetRoomsMessageValidator : AbstractValidator<GetRoomsMessage>
{
    public GetRoomsMessageValidator()
    {
        this.ApplyPaginationRules();
    }
}
