using Atlas.Domain.Constants;
using Atlas.Domain.Enums;
using CoreSolution.Shared.Results;
using FluentValidation;
using MediatR;

namespace Atlas.Application.Behaviors;

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
