namespace Atlas.Domain.Constants;

public static class EquipmentTypeConstants
{
    public static class Computer
    {
        public static readonly Guid Id = Guid.Parse("3f2504e0-4f89-41d3-9a0c-0305e82c3301");

        public const string Name = "Компьютер";
        public const string Description = "Рабочая станция или ноутбук";
    }

    public static class Printer
    {
        public static readonly Guid Id = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

        public const string Name = "Принтер";
        public const string Description = "Монохромный или цветной принтер";
    }

    public static class MultifunctionDevice
    {
        public static readonly Guid Id = Guid.Parse("16fd2706-8baf-433b-82eb-8c7fada847da");

        public const string Name = "МФУ";
        public const string Description = "Многофункциональное устройство";
    }

    public static class Monitor
    {
        public static readonly Guid Id = Guid.Parse("7f8c1d64-1d4f-4b9c-8a11-2e5d3c9f4a72");

        public const string Name = "Монитор";
        public const string Description = "Монитор рабочей станции";
    }
}
