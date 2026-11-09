namespace CoreSolution.Shared.Constants;

public static class JwtValidationConstants
{
    public const string SigningAlgorithm = "HS256";
    public const bool MapInboundClaims = false;
    public const bool ValidateIssuer = true;
    public const bool ValidateAudience = true;
    public const bool ValidateIssuerSigningKey = true;
    public const bool ValidateLifetime = true;

    public static readonly string[] ValidAlgorithms = [SigningAlgorithm];
    public static readonly TimeSpan ClockSkew = TimeSpan.Zero;
}
