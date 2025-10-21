using System.Security.Claims;
using PhotoCloud.Extensions;
using PhotoCloud.Interfaces;

namespace PhotoCloud.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClaimsPrincipal? _principal;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _principal = _httpContextAccessor.HttpContext?.User;
    }

    public ClaimsPrincipal? Principal => _principal;
    public bool IsAuthenticated => _principal?.Identity?.IsAuthenticated == true;
    public Guid? UserId => _principal?.GetUserId();
    public string? Email => _principal?.GetUserEmail();
    public string? Name => _principal?.GetUserName();
    public IReadOnlyList<string> Roles => _principal?.GetRoles() ?? new List<string>();
    public bool HasRole(string role) => Principal?.IsInRole(role) ?? false;

    public T? GetClaim<T>(string claimType) => _principal.GetClaim<T>(claimType);
}