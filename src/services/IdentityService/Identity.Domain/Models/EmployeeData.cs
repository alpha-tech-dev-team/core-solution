namespace Identity.Domain.Models;

public sealed record EmployeeData(
    string Name,
    string Surname,
    string? Patronymic,
    string Position,
    string Phone);
