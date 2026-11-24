namespace Atlas.Api.Constants;

public static class ApiRoutes
{
    public const string AdminGroup = "/api/admin";
    public const string Incidents = "incidents";
    public const string Rooms = "rooms";
    public const string RoomIdRouteKey = "roomId";
    public const string RoomById = $"rooms/{{{RoomIdRouteKey}:guid}}";
}
