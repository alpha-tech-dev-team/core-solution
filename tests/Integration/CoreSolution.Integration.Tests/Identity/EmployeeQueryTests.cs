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
        var filter = new EmployeeFilter(null, EmployeeSortFields.Surname.ToUpperInvariant(), SortDirection.Ascending, IncludeArchived: false);

        var surnames = EmployeeQuery.Apply(users.AsQueryable(), filter).Select(user => user.Surname).ToList();

        Assert.Equal(["Белов", "Яковлев"], surnames);
    }

    [Fact]
    public void Apply_WithSortByName_OrdersBySurnameThenId()
    {
        var first = User("Иван", "Яковлев", id: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var second = User("Иван", "Белов", id: Guid.Parse("00000000-0000-0000-0000-000000000002"));
        var third = User("Иван", "Белов", id: Guid.Parse("00000000-0000-0000-0000-000000000003"));
        var filter = new EmployeeFilter(null, EmployeeSortFields.Name.ToLowerInvariant(), SortDirection.Ascending, IncludeArchived: false);

        var ordered = EmployeeQuery.Apply(new[] { first, third, second }.AsQueryable(), filter).ToList();

        Assert.Equal([second.Id, third.Id, first.Id], ordered.Select(user => user.Id).ToList());
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

    [Fact]
    public void Apply_WithTiedSortKeysAscending_OrdersById()
    {
        var first = User("Иван", "Петров", id: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var second = User("Иван", "Петров", id: Guid.Parse("00000000-0000-0000-0000-000000000002"));
        var filter = new EmployeeFilter(null, EmployeeSortFields.Surname, SortDirection.Ascending, IncludeArchived: false);

        var firstRun = EmployeeQuery.Apply(new[] { second, first }.AsQueryable(), filter).Select(user => user.Id).ToList();
        var secondRun = EmployeeQuery.Apply(new[] { second, first }.AsQueryable(), filter).Select(user => user.Id).ToList();

        Assert.Equal([first.Id, second.Id], firstRun);
        Assert.Equal(firstRun, secondRun);
    }

    [Fact]
    public void Apply_WithTiedSortKeysDescending_OrdersById()
    {
        var first = User("Иван", "Петров", id: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var second = User("Иван", "Петров", id: Guid.Parse("00000000-0000-0000-0000-000000000002"));
        var filter = new EmployeeFilter(null, EmployeeSortFields.Surname, SortDirection.Descending, IncludeArchived: false);

        var firstRun = EmployeeQuery.Apply(new[] { second, first }.AsQueryable(), filter).Select(user => user.Id).ToList();
        var secondRun = EmployeeQuery.Apply(new[] { second, first }.AsQueryable(), filter).Select(user => user.Id).ToList();

        Assert.Equal([first.Id, second.Id], firstRun);
        Assert.Equal(firstRun, secondRun);
    }

    private static User User(string name, string surname, UserStatus status = UserStatus.Active, Guid? id = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Surname = surname,
            Status = status
        };
    }
}
