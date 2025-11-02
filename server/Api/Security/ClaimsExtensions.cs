using System.Security.Claims;
using Api.Models.Dtos.Responses;

namespace Api.Security;

public static class ClaimExtensions
{
    public static string GetUserId(this ClaimsPrincipal claims) =>
        claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    public static IEnumerable<Claim> ToClaims(this AuthUserInfo user)
    {
        yield return new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());
        yield return new Claim(ClaimTypes.Role, user.Role ?? string.Empty);
        // also add 'sub' if you prefer JWT standard claim name:
        yield return new Claim("sub", user.Id.ToString());
    }

    public static ClaimsPrincipal ToPrincipal(this AuthUserInfo user) =>
        new ClaimsPrincipal(new ClaimsIdentity(user.ToClaims(), "jwt"));
}