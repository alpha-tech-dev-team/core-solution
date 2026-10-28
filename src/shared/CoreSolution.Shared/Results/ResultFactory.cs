using System.Reflection;

namespace CoreSolution.Shared.Results;

public static class ResultFactory
{
    public static TResponse Failure<TResponse>(string errorCode)
        where TResponse : Result
    {
        var responseType = typeof(TResponse);

        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new InvalidOperationException($"Type {responseType.Name} must be a generic Result<T> type.");
        }

        var failure = responseType
            .GetMethod(nameof(Result<object>.Failure), BindingFlags.Public | BindingFlags.Static, [typeof(string)])!
            .Invoke(null, [errorCode]);

        return (TResponse)failure!;
    }

    public static TResponse Success<TResponse, TValue>(TValue value)
        where TResponse : Result
    {
        var responseType = typeof(TResponse);

        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new InvalidOperationException($"Type {responseType.Name} must be a generic Result<T> type.");
        }

        var success = responseType
            .GetMethod(nameof(Result<object>.Success), BindingFlags.Public | BindingFlags.Static, [typeof(TValue)])!
            .Invoke(null, [value]);

        return (TResponse)success!;
    }
}
