namespace Atlas.Application.Common;

// Telegram caps callback_data at 64 bytes, so the workflow markers stay short:
// state name (24) + a dashed Guid (36) would exceed the limit.
public static class CallbackConstants
{
    public const string CategoryPrefix = "category:";
    public const string CategoryPagePrefix = "category-page:";
}
