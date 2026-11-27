using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Models;

namespace Identity.Application.Features.Admin;

public static class EmployeeQuery
{
    public static IQueryable<User> Apply(IQueryable<User> users, EmployeeFilter filter) =>
        EmployeeSortFields.Instance.Apply(Filter(users, filter), filter.SortBy, filter.SortDirection);

    private static IQueryable<User> Filter(IQueryable<User> users, EmployeeFilter filter)
    {
        if (!filter.IncludeArchived)
        {
            users = users.Where(user => user.Status != UserStatus.Archived);
        }

        if (string.IsNullOrWhiteSpace(filter.Search))
        {
            return users;
        }

        var term = filter.Search.Trim().ToLower();

        return users.Where(user =>
            user.Surname.ToLower().Contains(term) || user.Name.ToLower().Contains(term));
    }
}
