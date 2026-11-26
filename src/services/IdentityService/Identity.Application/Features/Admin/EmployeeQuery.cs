using CoreSolution.Shared.Models;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Models;

namespace Identity.Application.Features.Admin;

public static class EmployeeQuery
{
    public static IQueryable<User> Apply(IQueryable<User> users, EmployeeFilter filter)
    {
        return Sort(Filter(users, filter), filter);
    }

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

    private static IQueryable<User> Sort(IQueryable<User> users, EmployeeFilter filter)
    {
        var sortField = EmployeeSortFields.Resolve(filter.SortBy);
        var selector = EmployeeSortFields.Fields[sortField];
        var ordered = filter.SortDirection == SortDirection.Descending
            ? users.OrderByDescending(selector)
            : users.OrderBy(selector);

        return sortField == EmployeeSortFields.Name
            ? ordered.ThenBy(user => user.Surname).ThenBy(user => user.Id)
            : ordered.ThenBy(user => user.Name).ThenBy(user => user.Id);
    }
}
