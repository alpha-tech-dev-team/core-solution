namespace Atlas.Domain.Constants;

public static class SoftwareConstants
{
    public static class Accounting
    {
        public static readonly Guid Id = Guid.Parse("c4d5e6f7-0819-4ca2-9d34-56e7f8091a2b");

        public const string Name = "1С:Бухгалтерия";
        public const string Version = "8.3";
    }

    public static class OfficeSuite
    {
        public static readonly Guid Id = Guid.Parse("d5e6f708-192a-4db3-8e45-67f8091a2b3c");

        public const string Name = "Microsoft 365";
        public const string Version = "2024";
    }
}
