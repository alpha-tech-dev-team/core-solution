using Pandora.Api.Extensions.Http;
using Pandora.Application.Contracts;

namespace Pandora.Api.Filters;

public sealed class RequestContextFilter : IEndpointFilter
{
    private const string ItemIdRouteKey = "itemId";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument is IAdminUserRequest adminUserRequest)
            {
                adminUserRequest.AdminUserId = context.HttpContext.User.GetAdminUserId();
            }

            if (argument is IItemIdRequest itemIdRequest)
            {
                itemIdRequest.ItemId = context.HttpContext.Request.RouteValues[ItemIdRouteKey]?.ToString() ?? string.Empty;
            }
        }

        return await next(context);
    }
}
