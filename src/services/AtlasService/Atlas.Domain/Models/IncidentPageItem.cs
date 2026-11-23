using Atlas.Domain.Enums;

namespace Atlas.Domain.Models;

public sealed record IncidentPageItem(
    Guid Id,
    long TicketNumber,
    string EmployeeFullName,
    string CategoryName,
    string? RoomName,
    string? HardwareName,
    string? SoftwareName,
    string UserDescription,
    IncidentStatus Status,
    IncidentPriority Priority,
    string? AssignedEngineerFullName,
    DateTime CreatedAt);
