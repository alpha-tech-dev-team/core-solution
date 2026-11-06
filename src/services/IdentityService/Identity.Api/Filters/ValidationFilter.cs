using FluentValidation;
using Identity.Api.Constants;
using Identity.Api.Models;
using Identity.Application.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Identity.Api.Filters;

public sealed class ValidationFilter<T>(IValidator<T> validator) : IAsyncActionFilter where T : class
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.ActionArguments.Values.OfType<T>().FirstOrDefault();
        if (request is null)
        {
            await next();
            return;
        }

        var validation = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (validation.IsValid)
        {
            await next();
            return;
        }

        var code = nameof(IdentityErrorCode.ValidationFailed);
        var errors = validation.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorCode).Distinct().ToArray());

        context.Result = new ObjectResult(new ValidationErrorResponse(code, errors))
        {
            StatusCode = IdentityErrorStatusCodes.Get(code)
        };
    }
}
