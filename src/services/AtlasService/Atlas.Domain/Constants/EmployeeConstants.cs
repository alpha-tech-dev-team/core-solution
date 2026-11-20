namespace Atlas.Domain.Constants;

/// <summary>
/// Значения для локальной записи сотрудника, когда профиль в Identity недоступен
/// (пользователь ещё не активирован или Identity не отвечает).
/// Поля employees.name/surname обязательные, поэтому нужен запасной вариант.
/// </summary>
public static class EmployeeConstants
{
    public const string FallbackName = "Сотрудник";
    public const string FallbackSurname = "Telegram";
}
