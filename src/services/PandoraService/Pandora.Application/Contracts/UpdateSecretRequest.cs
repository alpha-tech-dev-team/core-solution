namespace Pandora.Application.Contracts;

public sealed record UpdateSecretRequest(string Name, string Username, string? Password);
