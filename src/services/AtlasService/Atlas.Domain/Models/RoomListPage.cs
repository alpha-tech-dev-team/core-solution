namespace Atlas.Domain.Models;

public sealed record RoomListPage(
    IReadOnlyList<RoomPageItem> Items,
    int Page,
    int PageSize,
    int Total);
