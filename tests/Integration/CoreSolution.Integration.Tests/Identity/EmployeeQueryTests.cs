using CoreSolution.Shared.Models;
using Identity.Application.Features.Admin;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Models;
using Xunit;

namespace CoreSolution.Integration.Tests.IdentityTests;

public sealed class EmployeeQueryTests
{
    [Fact]
    public void Apply_WithSearch_MatchesNameOrSurnameIgnoringCase()
    {
        var users = new[]
        {
            User("Иван", "Петров"),
            User("Пётр", "Иванов"),
            User("Сидор", "Сидоров")
        };

        var filter = new EmployeeFilter("иВан", null, SortDirection.Ascending, IncludeArchived: false);

        var matched = EmployeeQuery.Apply(users.AsQueryable(), filter).ToList();

        Assert.Equal(2, matched.Count);
        Assert.All(matched, user => Assert.Contains("иван", $"{user.Surname}{user.Name}", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Apply_WithSortBySurnameDescending_OrdersDescending()
    {
        var users = new[] { User("А", "Яковлев"), User("Б", "Белов"), User("В", "Громов") };
        var filter = new EmployeeFilter(null, EmployeeSortFields.Surname, SortDirection.Descending, IncludeArchived: false);

        var surnames = EmployeeQuery.Apply(users.AsQueryable(), filter).Select(user => user.Surname).ToList();

        Assert.Equal(["Яковлев", "Громов", "Белов"], surnames);
    }

    [Fact]
    public void Apply_WithSortBySurnameAscending_OrdersAscending()
    {
        var users = new[] { User("А", "Яковлев"), User("Б", "Белов") };
        var filter = new EmployeeFilter(null, EmployeeSortFields.Surname, SortDirection.Ascending, IncludeArchived: false);

        var surnames = EmployeeQuery.Apply(users.AsQueryable(), filter).Select(user => user.Surname).ToList();

        Assert.Equal(["Белов", "Яковлев"], surnames);
    }

    [Fact]
    public void Apply_ByDefault_ExcludesArchivedUsers()
    {
        var active = User("Иван", "Петров");
        var archived = User("Пётр", "Иванов", UserStatus.Archived);
        var filter = new EmployeeFilter(null, null, SortDirection.Ascending, IncludeArchived: false);

        var users = EmployeeQuery.Apply(new[] { active, archived }.AsQueryable(), filter).ToList();

        Assert.Equal([active], users);
    }

    [Fact]
    public void Apply_WithIncludeArchived_KeepsArchivedUsers()
    {
        var active = User("Иван", "Петров");
        var archived = User("Пётр", "Иванов", UserStatus.Archived);
        var filter = new EmployeeFilter(null, null, SortDirection.Ascending, IncludeArchived: true);

        var users = EmployeeQuery.Apply(new[] { active, archived }.AsQueryable(), filter).ToList();

        Assert.Equal(2, users.Count);
    }

    private static User User(string name, string surname, UserStatus status = UserStatus.Active)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Surname = surname,
            Status = status
        };
    }
}
