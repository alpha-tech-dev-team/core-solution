using Atlas.Api.Constants;
using Atlas.Application.Contracts;

namespace Atlas.Api.Filters;

public sealed class RoomRequestContextFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var routeValue = context.HttpContext.Request.RouteValues[ApiRoutes.RoomIdRouteKey]?.ToString();

        if (Guid.TryParse(routeValue, out var itemId))
        {
            foreach (var argument in context.Arguments)
            {
                if (argument is IItemIdRequest request)
                {
                    request.ItemId = itemId;
                }
            }
        }

        return await next(context);
    }
}
