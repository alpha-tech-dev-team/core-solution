namespace Identity.Application.Constants;

public static class ValidationConstants
{
    public const long TelegramUserIdMinExclusive = 0;
    public const int PageMin = 1;
    public const int PageSizeMin = 1;
    public const int PageSizeMax = 50;
    public const int PageSizeDefault = 20;
    public const int PhoneMaxLength = 20;
    public const int PersonNameMaxLength = 50;
    public const int PositionMaxLength = 50;
    public const int PersonNameMinLength = 2;
    public const int PositionMinLength = 2;
    public const int AccessMaxLength = 50;
    public const int AdminUsernameMaxLength = 50;
    public const int PasswordMaxLength = 128;
    public const string PersonNamePattern = @"^[A-Za-zА-Яа-яЁё][A-Za-zА-Яа-яЁё \-]*$";
    public const string PositionPattern = @"^[A-Za-zА-Яа-яЁё0-9][A-Za-zА-Яа-яЁё0-9 \-().]*$";
    public const string PhonePattern = @"^\+?[0-9]{10,15}$";
}
