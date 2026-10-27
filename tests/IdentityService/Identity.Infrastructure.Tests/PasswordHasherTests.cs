using Identity.Core.Options;
using Identity.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_WritesParametersAndSixSections()
    {
        var hash = CreateHasher().HashPassword("secret");

        Assert.StartsWith("$argon2id$v=19$m=1024,t=1,p=1$", hash);
        Assert.Equal(6, hash.Split('$').Length);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrue_ForOriginalPassword()
    {
        var hasher = CreateHasher();
        var hash = hasher.HashPassword("secret");

        Assert.True(hasher.VerifyPassword("secret", hash));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForDifferentPassword()
    {
        var hasher = CreateHasher();
        var hash = hasher.HashPassword("secret");

        Assert.False(hasher.VerifyPassword("wrong", hash));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForTamperedHash()
    {
        var hasher = CreateHasher();
        var hash = hasher.HashPassword("secret");
        var tampered = hash[..^4] + "AAAA";

        Assert.False(hasher.VerifyPassword("secret", tampered));
    }

    [Fact]
    public void VerifyPassword_UsesParametersFromStoredHash()
    {
        var hash = CreateHasher(1024).HashPassword("secret");
        var hasherWithOtherParameters = CreateHasher(2048);

        Assert.True(hasherWithOtherParameters.VerifyPassword("secret", hash));
    }

    [Fact]
    public void HashPassword_UsesRandomSalt()
    {
        var hasher = CreateHasher();

        Assert.NotEqual(hasher.HashPassword("secret"), hasher.HashPassword("secret"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("$argon2id$v=19$m=1024,t=1,p=1$not-base64$hash")]
    [InlineData("$argon2id$v=19$m=0,t=1,p=1$c2FsdHNhbHQ=$aGFzaGhhc2hoYXNoaGFzaA==")]
    [InlineData("$argon2i$v=19$m=1024,t=1,p=1$c2FsdHNhbHQ=$aGFzaGhhc2hoYXNoaGFzaA==")]
    [InlineData("$argon2id$v=16$m=1024,t=1,p=1$c2FsdHNhbHQ=$aGFzaGhhc2hoYXNoaGFzaA==")]
    [InlineData("$argon2id$v=19$m=262145,t=1,p=1$c2FsdHNhbHQ=$aGFzaGhhc2hoYXNoaGFzaA==")]
    [InlineData("$argon2id$v=19$m=1024,t=11,p=1$c2FsdHNhbHQ=$aGFzaGhhc2hoYXNoaGFzaA==")]
    [InlineData("$argon2id$v=19$m=1024,t=1,p=9$c2FsdHNhbHQ=$aGFzaGhhc2hoYXNoaGFzaA==")]
    [InlineData("$argon2id$v=19$m=1024,t=1,p=1$c2FsdHNhbHQ=")]
    public void VerifyPassword_ReturnsFalse_ForMalformedHash(string? passwordHash)
    {
        Assert.False(CreateHasher().VerifyPassword("secret", passwordHash));
    }

    private static PasswordHasher CreateHasher(int memorySize = 1024)
    {
        var options = Options.Create(new PasswordHasherOptions
        {
            MemorySize = memorySize,
            Iterations = 1,
            Parallelism = 1,
            SaltSize = 16,
            HashSize = 32
        });

        return new PasswordHasher(options, NullLogger<PasswordHasher>.Instance);
    }
}
