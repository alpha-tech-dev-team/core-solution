using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Results;
using FluentValidation;
using MediatR;

namespace Atlas.Application.Behaviors;

/// <summary>
/// Validates every request that has a registered validator before the handler runs.
/// A request that fails validation never reaches the handler: the pipeline returns
/// a failed result with the validation error code, so the bot answers the user
/// instead of crashing inside the handler.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var validators = _validators.ToList();

        if (validators.Count == 0)
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        if (results.Any(result => !result.IsValid))
        {
            return ResultFactory.Failure<TResponse>(nameof(ErrorCode.ValidationFailed));
        }

        return await next(cancellationToken);
    }
}
