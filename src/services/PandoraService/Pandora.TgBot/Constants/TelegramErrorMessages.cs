using Pandora.Core.Enums;

namespace Pandora.TgBot.Constants;

internal static class TelegramErrorMessages
{
    private const string Unexpected = "Произошла ошибка. Попробуйте позже.";

    private static readonly IReadOnlyDictionary<ErrorCode, string> Messages =
        new Dictionary<ErrorCode, string>
        {
            [ErrorCode.AccessDenied] = "У вас нет доступа к этому сервису.",
            [ErrorCode.AccessNotConfirmed] = "Доступ не подтверждён. Обратитесь к администратору.",
            [ErrorCode.SessionNotFound] = "Доступ не подтверждён. Отправьте /start.",
            [ErrorCode.SecretNotFound] = "Учётные данные для этого сервиса не найдены. Обратитесь к администратору."
        };

    public static string Get(string? errorCode)
    {
        return Enum.TryParse<ErrorCode>(errorCode, out var code)
            && Messages.TryGetValue(code, out var text)
                ? text
                : Unexpected;
    }
}
