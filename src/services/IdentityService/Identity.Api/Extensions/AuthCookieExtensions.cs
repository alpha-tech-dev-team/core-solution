using Identity.Api.Constants;

namespace Identity.Api.Extensions;

public static class AuthCookieExtensions
{
    public static void SetAccessTokenCookie(this HttpResponse response, string accessToken, DateTimeOffset expiresAt)
    {
        response.Cookies.Append(AuthCookieConstants.AccessTokenName, accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = AuthCookieConstants.AccessTokenPath,
            Expires = expiresAt
        });
    }

    public static void SetRefreshTokenCookie(this HttpResponse response, string refreshToken, DateTimeOffset expiresAt)
    {
        response.Cookies.Append(AuthCookieConstants.RefreshTokenName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = ApiRoutes.AdminGroup,
            Expires = expiresAt
        });
    }

    public static void ClearAuthCookies(this HttpResponse response)
    {
        response.Cookies.Delete(
            AuthCookieConstants.AccessTokenName,
            new CookieOptions { Path = AuthCookieConstants.AccessTokenPath });
        response.Cookies.Delete(
            AuthCookieConstants.RefreshTokenName,
            new CookieOptions { Path = ApiRoutes.AdminGroup });
    }

    public static string? GetRefreshTokenCookie(this HttpRequest request)
    {
        return request.Cookies[AuthCookieConstants.RefreshTokenName];
    }
}
