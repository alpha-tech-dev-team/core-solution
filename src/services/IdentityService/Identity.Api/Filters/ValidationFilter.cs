using FluentValidation;
using FluentValidation.Results;
using Identity.Api.Constants;
using Identity.Api.Models;
using Identity.Application.Errors;

namespace Identity.Api.Filters;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        var request = context.Arguments.OfType<T>().FirstOrDefault();

        if (validator is null || request is null)
        {
            return await next(context);
        }

        var validation = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

        return validation.IsValid
            ? await next(context)
            : ToValidationError(validation);
    }

    private static IResult ToValidationError(ValidationResult validation)
    {
        var code = nameof(IdentityErrorCode.ValidationFailed);
        var errors = validation.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorCode).Distinct().ToArray());

        return Results.Json(
            new ValidationErrorResponse(code, errors),
            statusCode: IdentityErrorStatusCodes.Get(code));
    }
}
