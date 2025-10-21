using System.Security.Claims;
using PhotoCloud.Consts;

namespace PhotoCloud.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var claimId = principal.FindFirst(CustomClaims.UserId);
        if (Guid.TryParse(claimId?.Value, out var id)) return id;
        return null;
    }

    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        var email = principal.FindFirst(CustomClaims.Email)?.Value;
        return email;
    }

    public static string? GetUserName(this ClaimsPrincipal principal)
    {
        var name = principal.FindFirst(CustomClaims.Name)?.Value;
        return name;
    }

    public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal principal)
    {
        var roles = principal.FindAll(CustomClaims.Role)
            .Select(x => x.Value)
            .ToList();
        return roles;
    }

    public static T? GetClaim<T>(this ClaimsPrincipal? principal, string claimType)
    {
        var v = principal?.FindFirst(claimType);
        if (v == null) return default;
        return (T?)Convert.ChangeType(v.Value, typeof(T));
    }
}