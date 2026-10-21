using FluentValidation.Results;

namespace Identity.Api.Extensions;

public static class ValidationResultExtensions
{
    public static IResult ToValidationProblem(this ValidationResult validation)
    {
        return Results.ValidationProblem(validation.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray()));
    }
}
