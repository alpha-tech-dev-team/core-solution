namespace Pandora.Application.Contracts;

public sealed record CreateSecretRequest(string Name, string Username, string Password);
