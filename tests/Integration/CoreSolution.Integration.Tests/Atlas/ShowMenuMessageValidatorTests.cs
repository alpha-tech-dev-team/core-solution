using Atlas.Application.Features.Menu;
using Atlas.Application.Validators;
using Atlas.Domain.Enums;
using Xunit;

namespace CoreSolution.Integration.Tests.AtlasTests;

public sealed class ShowMenuMessageValidatorTests
{
    [Fact]
    public void ShowMenuMessage_RejectsPageBelowFirst_WithPageErrorCode()
    {
        var validator = new ShowMenuMessageValidator();
        var result = validator.Validate(new ShowMenuMessage(1, 1, 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorCode == nameof(ErrorCode.InvalidPageNumber));
    }
}
