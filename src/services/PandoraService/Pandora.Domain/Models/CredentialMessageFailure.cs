namespace Pandora.Domain.Models;

public sealed record CredentialMessageFailure(int ExpectedAttempts, string Error, DateTime NextAttemptAt);
