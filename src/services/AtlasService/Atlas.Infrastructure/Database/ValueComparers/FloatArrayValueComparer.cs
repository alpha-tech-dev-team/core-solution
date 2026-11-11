using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Atlas.Infrastructure.Database.ValueComparers;

public static class FloatArrayValueComparer
{
    public static ValueComparer<float[]> Instance { get; } = new(
        (left, right) => ReferenceEquals(left, right)
            || (left != null && right != null && left.SequenceEqual(right)),
        values => values == null
            ? 0
            : values.Aggregate(0, (hash, value) => HashCode.Combine(hash, value.GetHashCode())),
        values => values == null ? null! : values.ToArray());
}
