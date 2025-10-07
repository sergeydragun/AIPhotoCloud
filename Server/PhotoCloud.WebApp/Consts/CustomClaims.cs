using System.Security.Claims;

namespace PhotoCloud.Consts;

public static class CustomClaims
{
    public const string UserId = "user_id";
    public const string Email = ClaimTypes.Email;
    public const string Name = ClaimTypes.Name;
    public const string Role = ClaimTypes.Role; // роли
}