using CoreSolution.Shared.Models;
using Identity.Application.Constants;
using Identity.Application.Contracts.Admin;
using Identity.Application.Features.Admin;
using Identity.Application.Validators.Admin;
using Xunit;

namespace CoreSolution.Integration.Tests.IdentityTests;

public sealed class GetUsersRequestValidatorTests
{
    private readonly GetUsersRequestValidator _validator = new();

    [Fact]
    public void Validates_DefaultRequest()
    {
        Assert.True(_validator.Validate(new GetUsersRequest()).IsValid);
    }

    [Fact]
    public void Rejects_PageBelowFirst()
    {
        var request = new GetUsersRequest { Page = ValidationConstants.PageMin - 1 };

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Rejects_PageSizeAboveIdentityLimit()
    {
        var request = new GetUsersRequest { PageSize = ValidationConstants.PageSizeMax + 1 };

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Accepts_PageSizeAtIdentityLimit()
    {
        var request = new GetUsersRequest { PageSize = ValidationConstants.PageSizeMax };

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Accepts_SortByKnownField()
    {
        var request = new GetUsersRequest { SortBy = EmployeeSortFields.Surname };

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Accepts_SortByKnownFieldIgnoringCase()
    {
        var request = new GetUsersRequest { SortBy = EmployeeSortFields.Surname.ToUpperInvariant() };

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Rejects_SortByUnknownField()
    {
        var request = new GetUsersRequest { SortBy = nameof(GetUsersRequestValidator) };

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Rejects_UndefinedSortDirection()
    {
        var request = new GetUsersRequest { SortDirection = (SortDirection)int.MaxValue };

        Assert.False(_validator.Validate(request).IsValid);
    }
}
