using FluentValidation;
using Identity.Api.Extensions;

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
            : validation.ToValidationProblem();
    }
}
