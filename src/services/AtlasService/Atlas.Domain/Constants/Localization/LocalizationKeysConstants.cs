namespace Atlas.Domain.Constants.Localization;

public static class LocalizationKeysConstants
{
    public static class Idle
    {
        public const string PhoneRequest = "PhoneRequest";
        public const string AccessNotConfirmed = "AccessNotConfirmed";
        public const string SessionNotFound = "SessionNotFound";
        public const string UnexpectedError = "UnexpectedError";
        public const string ValidationFailed = "ValidationFailed";
        public const string InvalidTelegramUserId = "InvalidTelegramUserId";
        public const string InvalidMessageId = "InvalidMessageId";
        public const string InvalidPageNumber = "InvalidPageNumber";
        public const string InvalidCategoryId = "InvalidCategoryId";
    }

    public static class ProblemCategorySelection
    {
        public const string Text = "Text";
        public const string CategoriesNotFound = "CategoriesNotFound";
        public const string CategoryNotFound = "CategoryNotFound";
        public const string NextPage = "NextPage";
    }

    public static class CategorySelected
    {
        public const string Text = "Text";
        public const string Button = "Button";
    }
}
