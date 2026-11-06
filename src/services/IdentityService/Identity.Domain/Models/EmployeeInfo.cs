namespace Identity.Domain.Models;

public sealed record EmployeeInfo(
    Guid Id,
    string Name,
    string Surname,
    string? Patronymic,
    string Position,
    string Phone,
    string Status,
    IReadOnlyList<string> Roles,
    string? AdminUsername);
