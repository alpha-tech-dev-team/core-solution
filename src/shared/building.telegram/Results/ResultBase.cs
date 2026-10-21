namespace Building.Telegram.Results;

public class Result
{
    public bool IsSuccess { get; init; }

    public string? ErrorCode { get; init; }

    public string[] ErrorsList { get; init; } = [];

    public static Result<T> Failure<T>(string errorCode, IEnumerable<string>? errors = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);

        return new Result<T>
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorsList = errors?.ToArray() ?? [errorCode]
        };
    }
}
