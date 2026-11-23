using Atlas.Api.Constants;
using Atlas.Api.Extensions.Http;
using Atlas.Api.Filters;
using Atlas.Application.Features.Rooms;
using CoreSolution.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Endpoints;

public static class RoomsEndpoints
{
    public static IEndpointRouteBuilder MapRoomsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(ApiRoutes.AdminGroup).RequireAuthorization(nameof(UserRole.AtlasAdmin));

        group.MapGet(ApiRoutes.Rooms, GetRoomsList)
            .AddEndpointFilter<ValidationFilter<GetRoomsMessage>>();
        group.MapPost(ApiRoutes.Rooms, CreateRoom)
            .AddEndpointFilter<ValidationFilter<CreateRoomMessage>>();
        group.MapPatch(ApiRoutes.RoomById, UpdateRoom)
            .AddEndpointFilter<ValidationFilter<UpdateRoomMessage>>();
        group.MapDelete(ApiRoutes.RoomById, DeleteRoom);

        return endpoints;
    }

    private static async Task<IResult> GetRoomsList(
        [FromServices] IMediator mediator,
        [AsParameters] GetRoomsMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateRoom(
        [FromServices] IMediator mediator,
        CreateRoomMessage message,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> UpdateRoom(
        [FromServices] IMediator mediator,
        [FromRoute] Guid roomId,
        UpdateRoomMessage message,
        CancellationToken cancellationToken = default)
    {
        message.RoomId = roomId;
        var result = await mediator.Send(message, cancellationToken);

        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteRoom(
        [FromServices] IMediator mediator,
        [FromRoute] Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeleteRoomMessage(roomId), cancellationToken);

        return result.ToHttpResult();
    }
}
