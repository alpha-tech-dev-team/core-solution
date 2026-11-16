using Pandora.Application.Features.Secrets.CreateSecret;
using Pandora.Application.Features.Secrets.UpdateSecret;
using Pandora.Domain.Models;

namespace Pandora.Application.Features.Secrets.Common;

public static class SecretMessageExtensions
{
    public static SecretName Normalize(this CreateSecretMessage message)
    {
        return new SecretName(message.Name.Trim(), message.Username.Trim());
    }

    public static SecretName Normalize(this UpdateSecretMessage message)
    {
        return new SecretName(message.Name.Trim(), message.Username.Trim());
    }
}
