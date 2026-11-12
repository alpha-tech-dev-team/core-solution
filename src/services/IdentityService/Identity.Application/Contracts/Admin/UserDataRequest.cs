namespace Identity.Application.Contracts.Admin;

public record UserDataRequest(
    string Name,
    string Surname,
    string? Patronymic,
    string Position,
    string Phone);
