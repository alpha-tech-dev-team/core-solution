namespace Identity.Domain.Models;

public sealed record EmployeesPage(IReadOnlyList<EmployeeInfo> Items, int Total, int Page, int PageSize);
