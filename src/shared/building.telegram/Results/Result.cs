namespace Building.Telegram.Results;

public class Result<T> : Result
{
    public T? Value { get; init; }

    public static Result<T> Success(T value)
    {
        return new Result<T>
        {
            Value = value,
            IsSuccess = true
        };
    }
}
