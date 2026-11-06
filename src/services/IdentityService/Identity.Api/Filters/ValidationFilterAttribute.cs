using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ValidationFilterAttribute<T> : TypeFilterAttribute where T : class
{
    public ValidationFilterAttribute() : base(typeof(ValidationFilter<T>))
    {
    }
}
