namespace CoreSolution.Shared.Results;

public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? errorCode)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(string errorCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);

        return new Result(false, errorCode);
    }
}
