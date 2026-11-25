namespace Pandora.Domain.Models;

public sealed record CredentialFailure(int ExpectedAttempts, string Error, DateTime NextAttemptAt);
