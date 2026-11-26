namespace Pandora.Domain.Models;

public sealed record CredentialClaim(DateTime Now, DateTime ClaimUntil, int Limit, int MaxAttempts);
