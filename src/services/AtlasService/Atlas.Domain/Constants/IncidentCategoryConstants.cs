namespace Atlas.Domain.Constants;

public static class IncidentCategoryConstants
{
    public static class Hardware
    {
        public static readonly Guid Id = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");

        public const string Name = "hardware";
        public const string Description = "Hardware incidents";
    }

    public static class Software
    {
        public static readonly Guid Id = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7");

        public const string Name = "software";
        public const string Description = "Software incidents";
    }

    public static class Other
    {
        public static readonly Guid Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479");

        public const string Name = "other";
        public const string Description = "Other incidents";
    }
}
