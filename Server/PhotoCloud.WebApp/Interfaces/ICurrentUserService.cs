using System.Security.Claims;

namespace PhotoCloud.Interfaces;

public interface ICurrentUserService
{
    ClaimsPrincipal? Principal { get; }
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    string? Email { get; }
    string? Name { get; }
    IReadOnlyList<string> Roles { get; }

    bool HasRole(string role);
    T? GetClaim<T>(string claimType);
}