namespace Identity.Infrastructure.Models;

internal readonly record struct StoredHash(Argon2Parameters Parameters, byte[] Salt, byte[] Hash);
