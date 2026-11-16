using Atlas.Domain.Constants;

namespace Atlas.Application.Common;

public static class CallbackDataExtensions
{
    public static int ParsePage(this string data)
    {
        var value = data[CallbackConstants.CategoryPagePrefix.Length..];

        return int.TryParse(value, out var page) ? page : PaginationLimits.FirstPage;
    }

    public static Guid ParseCategoryId(this string data)
    {
        var value = data[CallbackConstants.CategoryPrefix.Length..];

        return Guid.TryParse(value, out var categoryId) ? categoryId : Guid.Empty;
    }
}
