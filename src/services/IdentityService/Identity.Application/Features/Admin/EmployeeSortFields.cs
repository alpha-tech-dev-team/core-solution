using CoreSolution.Shared.Application.Sorting;
using Identity.Domain.Entities;

namespace Identity.Application.Features.Admin;

public sealed class EmployeeSortFields : SortFields<User>
{
    public const string Surname = nameof(User.Surname);
    public const string Name = nameof(User.Name);
    public const string Default = Surname;

    public static readonly EmployeeSortFields Instance = new();

    private EmployeeSortFields()
        : base(Default, (Surname, user => user.Surname), (Name, user => user.Name))
    {
    }

    protected override IOrderedQueryable<User> TieBreak(IOrderedQueryable<User> ordered, string field)
    {
        return field == Name
            ? ordered.ThenBy(user => user.Surname).ThenBy(user => user.Id)
            : ordered.ThenBy(user => user.Name).ThenBy(user => user.Id);
    }
}
