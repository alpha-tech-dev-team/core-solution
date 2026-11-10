using FluentValidation;
using Pandora.Api.Extensions;

namespace Pandora.Api.Filters;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
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
