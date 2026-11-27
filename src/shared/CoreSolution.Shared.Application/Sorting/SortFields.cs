using System.Linq.Expressions;
using CoreSolution.Shared.Models;

namespace CoreSolution.Shared.Application.Sorting;

public abstract class SortFields<T>
{
    private readonly Dictionary<string, Expression<Func<T, string>>> _fields;

    protected SortFields(
        string? defaultField = null,
        params (string Name, Expression<Func<T, string>> Selector)[] fields)
    {
        if (fields.Length == 0)
        {
            throw new ArgumentException("At least one sort field is required", nameof(fields));
        }

        DefaultField = defaultField ?? fields.First().Name;
        _fields = fields.ToDictionary(field => field.Name, field => field.Selector, StringComparer.OrdinalIgnoreCase);

        if (!_fields.ContainsKey(DefaultField))
        {
            throw new ArgumentException("Default sort field must be one of the declared fields", nameof(defaultField));
        }
    }

    private string DefaultField { get; }

    public IReadOnlyCollection<string> SupportedFields => _fields.Keys.ToArray();

    public IOrderedQueryable<T> Apply(IQueryable<T> source, string? sortBy, SortDirection? direction)
    {
        var (field, selector) = Resolve(sortBy);
        var ordered = direction == SortDirection.Descending
            ? source.OrderByDescending(selector)
            : source.OrderBy(selector);

        return TieBreak(ordered, field);
    }

    protected abstract IOrderedQueryable<T> TieBreak(IOrderedQueryable<T> ordered, string field);

    private (string Name, Expression<Func<T, string>> Selector) Resolve(string? sortBy)
    {
        if (sortBy is not null)
        {
            foreach (var field in _fields)
            {
                if (string.Equals(field.Key, sortBy, StringComparison.OrdinalIgnoreCase))
                {
                    return (field.Key, field.Value);
                }
            }
        }

        return (DefaultField, _fields[DefaultField]);
    }
}
