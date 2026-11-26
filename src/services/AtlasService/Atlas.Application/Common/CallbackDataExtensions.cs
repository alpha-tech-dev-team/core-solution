using Atlas.Domain.Constants;
using Atlas.Domain.Enums;

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

    public static int ParseRoomPage(this string data)
    {
        var value = data[CallbackConstants.RoomPagePrefix.Length..];

        return int.TryParse(value, out var page) ? page : PaginationLimits.FirstPage;
    }

    public static Guid ParseRoomId(this string data)
    {
        var value = data[CallbackConstants.RoomPrefix.Length..];

        return Guid.TryParse(value, out var roomId) ? roomId : Guid.Empty;
    }

    public static Guid ParseHardwareId(this string data)
    {
        var value = data[CallbackConstants.HardwarePrefix.Length..];

        return Guid.TryParse(value, out var hardwareId) ? hardwareId : Guid.Empty;
    }

    public static int ParseHardwarePage(this string data)
    {
        var value = data[CallbackConstants.HardwarePagePrefix.Length..];

        return int.TryParse(value, out var page) ? page : PaginationLimits.FirstPage;
    }

    public static Guid ParseSoftwareId(this string data)
    {
        var value = data[CallbackConstants.SoftwarePrefix.Length..];

        return Guid.TryParse(value, out var softwareId) ? softwareId : Guid.Empty;
    }

    public static int ParseSoftwarePage(this string data)
    {
        var value = data[CallbackConstants.SoftwarePagePrefix.Length..];

        return int.TryParse(value, out var page) ? page : PaginationLimits.FirstPage;
    }

    public static IncidentStepAction? ParseIncidentStepAction(this string data)
    {
        var value = data[CallbackConstants.IncidentPrefix.Length..];

        return Enum.TryParse<IncidentStepAction>(value, ignoreCase: true, out var action) ? action : null;
    }
}
