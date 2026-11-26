using System.Linq.Expressions;
using Identity.Domain.Entities;

namespace Identity.Application.Features.Admin;

public static class EmployeeSortFields
{
    public const string Surname = nameof(User.Surname);
    public const string Name = nameof(User.Name);
    public const string Default = Surname;

    public static readonly IReadOnlyDictionary<string, Expression<Func<User, string>>> Fields =
        new Dictionary<string, Expression<Func<User, string>>>(StringComparer.OrdinalIgnoreCase)
        {
            [Surname] = user => user.Surname,
            [Name] = user => user.Name
        };

    public static bool IsSupported(string? sortBy)
    {
        return sortBy is not null && Fields.ContainsKey(sortBy);
    }

    public static string Resolve(string? sortBy)
    {
        if (sortBy is null)
        {
            return Default;
        }

        return Fields.Keys.FirstOrDefault(key => string.Equals(key, sortBy, StringComparison.OrdinalIgnoreCase))
            ?? Default;
    }
}
