using CoreSolution.Shared.Enums;

namespace CoreSolution.Shared.Constants;

public static class LocalizationConstants
{
    public const Language DefaultLanguage = Language.Ru;
    public const string IdleStateName = "Idle";

    public static class Idle
    {
        public const string PhoneRequest = "PhoneRequest";
        public const string BtnShareContact = "BtnShareContact";
        public const string AccessNotConfirmed = "AccessNotConfirmed";
        public const string UnexpectedError = "UnexpectedError";
    }
}
