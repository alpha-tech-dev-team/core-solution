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

    private sealed class NoFieldsSorting : SortFields<string>
    {
        protected override IOrderedQueryable<string> TieBreak(IOrderedQueryable<string> ordered, string field) =>
            ordered;
    }
}
