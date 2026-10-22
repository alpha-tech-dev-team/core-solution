using System.Reflection;

namespace CoreSolution.Shared.Results;

public static class ResultFactory
{
    public static TResponse Failure<TResponse>(string errorCode, IEnumerable<string>? errors = null)
        where TResponse : Result
    {
        var responseType = typeof(TResponse);

        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new InvalidOperationException($"Type {responseType.Name} must be a generic Result<T> type.");
        }

        var failure = typeof(Result)
            .GetMethod(nameof(Result.Failure), BindingFlags.Public | BindingFlags.Static, [typeof(string), typeof(IEnumerable<string>)])!
            .MakeGenericMethod(responseType.GetGenericArguments())
            .Invoke(null, [errorCode, errors]);

        return (TResponse)failure!;
    }
}
