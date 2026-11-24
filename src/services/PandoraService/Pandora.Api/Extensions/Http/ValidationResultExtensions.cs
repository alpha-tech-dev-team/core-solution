using FluentValidation.Results;
using Pandora.Api.Constants;
using Pandora.Domain.Enums;

namespace Pandora.Api.Extensions.Http;

public static class ValidationResultExtensions
{
    public static IResult ToValidationProblem(this ValidationResult validation)
    {
        var errors = validation.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

        var code = validation.Errors
            .Select(error => error.ErrorCode)
            .FirstOrDefault(errorCode => !string.IsNullOrWhiteSpace(errorCode))
            ?? nameof(ErrorCode.MalformedRequest);

        return Results.ValidationProblem(
            errors,
            extensions: new Dictionary<string, object?> { [ProblemFields.Code] = code });
    }
}
