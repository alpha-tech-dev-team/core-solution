using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Identity.Core.Interfaces.Security;
using Identity.Core.Options;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private static readonly UTF8Encoding PasswordEncoding = new(encoderShouldEmitUTF8Identifier: false);

    private readonly PasswordHasherOptions _options;
    private readonly ILogger<PasswordHasher> _logger;

    public PasswordHasher(IOptions<PasswordHasherOptions> options, ILogger<PasswordHasher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public string HashPassword(string password)
    {
        var parameters = new Argon2Parameters(_options.MemorySize, _options.Iterations, _options.Parallelism);
        var salt = RandomNumberGenerator.GetBytes(_options.SaltSize);
        var hash = ComputeHash(password, parameters, salt, _options.HashSize);

        return string.Format(
            CultureInfo.InvariantCulture,
            PasswordHasherConstants.Format,
            PasswordHasherConstants.Algorithm,
            PasswordHasherConstants.Version,
            parameters.MemorySize,
            parameters.Iterations,
            parameters.Parallelism,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string password, string? passwordHash)
    {
        if (passwordHash is null)
        {
            return false;
        }

        if (!TryParse(passwordHash, out var storedHash))
        {
            _logger.LogWarning("Сохраненный хеш пароля имеет некорректный формат");
            return false;
        }

        var actualHash = ComputeHash(password, storedHash.Parameters, storedHash.Salt, storedHash.Hash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, storedHash.Hash);
    }

    private static byte[] ComputeHash(string password, Argon2Parameters parameters, byte[] salt, int hashSize)
    {
        using var argon2 = new Argon2id(PasswordEncoding.GetBytes(password))
        {
            DegreeOfParallelism = parameters.Parallelism,
            MemorySize = parameters.MemorySize,
            Iterations = parameters.Iterations,
            Salt = salt
        };

        return argon2.GetBytes(hashSize);
    }

    private static bool TryParse(string passwordHash, out StoredHash storedHash)
    {
        storedHash = default;

        if (!passwordHash.StartsWith(PasswordHasherConstants.SectionDelimiter))
        {
            return false;
        }

        var sections = passwordHash.Split(PasswordHasherConstants.SectionDelimiter);

        if (sections.Length != PasswordHasherConstants.ExpectedSectionCount
            || sections[PasswordHasherConstants.AlgorithmSectionIndex] != PasswordHasherConstants.Algorithm
            || sections[PasswordHasherConstants.VersionSectionIndex] != PasswordHasherConstants.Version)
        {
            return false;
        }

        var encodedParameters = sections[PasswordHasherConstants.ParametersSectionIndex]
            .Split(PasswordHasherConstants.ParameterDelimiter);

        if (encodedParameters.Length != PasswordHasherConstants.ExpectedParameterCount
            || !TryReadParameter(encodedParameters, PasswordHasherConstants.MemoryPrefix, out var memorySize)
            || !TryReadParameter(encodedParameters, PasswordHasherConstants.IterationsPrefix, out var iterations)
            || !TryReadParameter(encodedParameters, PasswordHasherConstants.ParallelismPrefix, out var parallelism)
            || memorySize < PasswordHasherConstants.MinMemorySize || memorySize > PasswordHasherConstants.MaxMemorySize
            || iterations < PasswordHasherConstants.MinIterations || iterations > PasswordHasherConstants.MaxIterations
            || parallelism < PasswordHasherConstants.MinParallelism || parallelism > PasswordHasherConstants.MaxParallelism)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(sections[PasswordHasherConstants.SaltSectionIndex]);
            var hash = Convert.FromBase64String(sections[PasswordHasherConstants.HashSectionIndex]);

            if (salt.Length < PasswordHasherConstants.MinSaltSize || salt.Length > PasswordHasherConstants.MaxSaltSize
                || hash.Length < PasswordHasherConstants.MinHashSize || hash.Length > PasswordHasherConstants.MaxHashSize)
            {
                return false;
            }

            storedHash = new StoredHash(new Argon2Parameters(memorySize, iterations, parallelism), salt, hash);

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TryReadParameter(string[] encodedParameters, string prefix, out int value)
    {
        value = 0;

        foreach (var encodedParameter in encodedParameters)
        {
            if (!encodedParameter.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            return int.TryParse(
                encodedParameter.AsSpan(prefix.Length),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out value);
        }

        return false;
    }

    private readonly record struct Argon2Parameters(int MemorySize, int Iterations, int Parallelism);

    private readonly record struct StoredHash(Argon2Parameters Parameters, byte[] Salt, byte[] Hash);
}
