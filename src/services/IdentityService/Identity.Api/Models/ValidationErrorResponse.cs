namespace Identity.Api.Models;

public sealed record ValidationErrorResponse(string Code, IReadOnlyDictionary<string, string[]> Errors);
