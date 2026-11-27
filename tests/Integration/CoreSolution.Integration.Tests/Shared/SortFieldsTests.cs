using CoreSolution.Shared.Application.Sorting;
using Xunit;

namespace CoreSolution.Integration.Tests.Shared;

public sealed class SortFieldsTests
{
    [Fact]
    public void Constructor_WithoutFields_Throws()
    {
        Assert.Throws<ArgumentException>(() => new NoFieldsSorting());
    }

    [Fact]
    public void Constructor_WithUndeclaredDefaultField_Throws()
    {
        Assert.Throws<ArgumentException>(() => new UndeclaredDefaultSorting("Missing"));
    }

    [Theory]
    [InlineData(null, "ab")]
    [InlineData("Name", "ab")]
    [InlineData("Reverse", "ba")]
    public void Apply_WithoutSortBy_UsesDeclaredDefaultField(string? defaultField, string expectedFirst)
    {
        var sorting = new DeclaredDefaultSorting(defaultField);

        var ordered = sorting.Apply(new[] { "ab", "ba" }.AsQueryable(), null, null);

        Assert.Equal(expectedFirst, ordered.First());
    }

    private sealed class NoFieldsSorting : SortFields<string>
    {
        protected override IOrderedQueryable<string> TieBreak(IOrderedQueryable<string> ordered, string field) =>
            ordered;
    }

    private sealed class UndeclaredDefaultSorting : SortFields<string>
    {
        public UndeclaredDefaultSorting(string defaultField)
            : base(defaultField, ("Name", value => value))
        {
        }

        protected override IOrderedQueryable<string> TieBreak(IOrderedQueryable<string> ordered, string field) =>
            ordered;
    }

    private sealed class DeclaredDefaultSorting : SortFields<string>
    {
        public DeclaredDefaultSorting(string? defaultField)
            : base(defaultField, ("Name", value => value), ("Reverse", value => new string(value.Reverse().ToArray())))
        {
        }

        protected override IOrderedQueryable<string> TieBreak(IOrderedQueryable<string> ordered, string field) =>
            ordered;
    }
}
