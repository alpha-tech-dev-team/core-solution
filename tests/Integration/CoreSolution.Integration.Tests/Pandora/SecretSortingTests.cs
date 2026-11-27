using CoreSolution.Shared.Models;
using Pandora.Application.Features.Secrets.Common;
using Pandora.Domain.Entities;
using Xunit;

namespace CoreSolution.Integration.Tests.PandoraTests;

public sealed class SecretSortingTests
{
    [Fact]
    public void For_ByResourceNameAscending_OrdersByName()
    {
        var secrets = new[] { Secret("c"), Secret("a"), Secret("b") };

        var ordered = SecretSorting
            .For(nameof(SecretMetadata.ResourceName), SortDirection.Ascending)(secrets.AsQueryable())
            .Select(secret => secret.ResourceName)
            .ToList();

        Assert.Equal(["a", "b", "c"], ordered);
    }

    [Fact]
    public void For_ByResourceNameDescending_OrdersDescending()
    {
        var secrets = new[] { Secret("a"), Secret("c"), Secret("b") };

        var ordered = SecretSorting
            .For(nameof(SecretMetadata.ResourceName), SortDirection.Descending)(secrets.AsQueryable())
            .Select(secret => secret.ResourceName)
            .ToList();

        Assert.Equal(["c", "b", "a"], ordered);
    }

    [Fact]
    public void For_ByUsername_OrdersByUsername()
    {
        var secrets = new[] { Secret("a", "zzz"), Secret("b", "aaa") };

        var ordered = SecretSorting
            .For(nameof(SecretMetadata.Username), SortDirection.Ascending)(secrets.AsQueryable())
            .Select(secret => secret.Username)
            .ToList();

        Assert.Equal(["aaa", "zzz"], ordered);
    }

    [Fact]
    public void For_WithUnknownField_FallsBackToDefaultField()
    {
        var secrets = new[] { Secret("b"), Secret("a") };

        var ordered = SecretSorting
            .For(nameof(SecretMetadata.VaultItemId), SortDirection.Ascending)(secrets.AsQueryable())
            .Select(secret => secret.ResourceName)
            .ToList();

        Assert.Equal(["a", "b"], ordered);
    }

    [Fact]
    public void SupportedFields_ContainsResourceNameAndUsername()
    {
        Assert.Contains(nameof(SecretMetadata.ResourceName), SecretSorting.Instance.SupportedFields);
        Assert.Contains(nameof(SecretMetadata.Username), SecretSorting.Instance.SupportedFields);
    }

    [Fact]
    public void For_ByResourceNameAscending_WithTiedNames_OrdersByVaultItemId()
    {
        var secrets = new[] { Secret("same", vaultItemId: "b"), Secret("same", vaultItemId: "a") };

        var ordered = SecretSorting
            .For(nameof(SecretMetadata.ResourceName), SortDirection.Ascending)(secrets.AsQueryable())
            .Select(secret => secret.VaultItemId)
            .ToList();

        Assert.Equal(["a", "b"], ordered);
    }

    [Fact]
    public void For_ByResourceNameDescending_WithTiedNames_OrdersByVaultItemId()
    {
        var secrets = new[] { Secret("same", vaultItemId: "b"), Secret("same", vaultItemId: "a") };

        var ordered = SecretSorting
            .For(nameof(SecretMetadata.ResourceName), SortDirection.Descending)(secrets.AsQueryable())
            .Select(secret => secret.VaultItemId)
            .ToList();

        Assert.Equal(["a", "b"], ordered);
    }

    private static SecretMetadata Secret(string resourceName, string username = "login", string? vaultItemId = null)
    {
        return new SecretMetadata(vaultItemId ?? Guid.NewGuid().ToString("D"), resourceName, username);
    }
}
