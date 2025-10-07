using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PhotoCloud.Interfaces;

namespace PhotoCloud.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly ICurrentUserService CurrentUserService;

    public BaseController(ICurrentUserService currentUserService)
    {
        CurrentUserService = currentUserService;
    }

    protected Guid? CurrentUserId => CurrentUserService.UserId;
    protected string? Email => CurrentUserService.Email;
    protected string? Name => CurrentUserService.Name;
    protected IReadOnlyCollection<string> Roles => CurrentUserService.Roles;
    protected bool IsAuthenticated => CurrentUserService.IsAuthenticated;

    protected void RequireAuthentification()
    {
        if (!IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated");
    }

    protected ActionResult<T> ForbitIfNoRole<T>(string role)
    {
        if (CurrentUserService.HasRole(role)) return Forbid();
        return null!;
    }
}