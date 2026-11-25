namespace Pandora.Domain.Models;

public sealed record CredentialMessageClaim(DateTime Now, DateTime ClaimUntil, int Limit, int MaxAttempts);
