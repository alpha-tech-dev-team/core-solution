namespace Atlas.Domain.Models;

public sealed record RoomPageItem(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt);
