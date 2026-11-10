using FluentValidation.Results;

namespace Pandora.Api.Extensions;

public static class ValidationResultExtensions
{
    public static IResult ToValidationProblem(this ValidationResult validation)
    {
        return Results.ValidationProblem(validation.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));
    }
}
