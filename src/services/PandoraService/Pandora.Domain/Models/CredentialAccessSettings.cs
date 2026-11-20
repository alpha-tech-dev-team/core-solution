namespace Pandora.Domain.Models;

public sealed record CredentialAccessSettings(int TtlMinutes, DateTime? UpdatedAt);
