namespace CoreSolution.Shared.Results;

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? errorCode)
        : base(isSuccess, errorCode)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static new Result<T> Failure(string errorCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);

        return new Result<T>(false, default, errorCode);
    }
}
