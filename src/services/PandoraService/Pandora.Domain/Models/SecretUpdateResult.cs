namespace Pandora.Domain.Models;

public enum SecretUpdateResult
{
    Success,
    NoPassword,
    NotFound,
    GatewayError
}
