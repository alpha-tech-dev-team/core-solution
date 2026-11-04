namespace Pandora.Api.Constants;

public static class JwtConstants
{
    public const string SigningAlgorithm = "HS256";
    public const string SubjectClaim = "sub";
    public const string UniqueNameClaim = "unique_name";
    public const string RoleClaim = "role";
    public const bool MapInboundClaims = false;
    public const bool ValidateIssuer = true;
    public const bool ValidateAudience = true;
    public const bool ValidateIssuerSigningKey = true;
    public const bool ValidateLifetime = true;

    public static readonly string[] ValidAlgorithms = [SigningAlgorithm];
    public static readonly TimeSpan ClockSkew = TimeSpan.Zero;
}
