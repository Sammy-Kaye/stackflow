// CurrentUserService — reads the authenticated user's identity from the JWT claims
// that ASP.NET Core has already validated and stored on HttpContext.User.
//
// This implementation lives in the API layer because it depends on IHttpContextAccessor,
// which is an HTTP concern. Handlers depend on ICurrentUserService (Application layer)
// and know nothing about HTTP or claims.
//
// Claim mapping:
//   ClaimTypes.NameIdentifier  → maps from the JWT "sub" claim (set in AuthController)
//   ClaimTypes.Email           → maps from the JWT "email" claim
//   "workspaceId"              → custom claim set in AuthController.BuildDevToken()
//
// These claim names must match exactly what AuthController embeds in the token.
// In Phase 2, when real auth replaces the dev stub, the same claim names apply.

using System.Security.Claims;
using StackFlow.Application.Common.Interfaces;

namespace StackFlow.Api.Services;

/// <summary>
/// Reads the authenticated user's identity from JWT claims on the current HTTP request.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id)
                ? id
                : throw new InvalidOperationException("UserId claim is missing or invalid.");
        }
    }

    public string Email
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.Email)
                ?? throw new InvalidOperationException("Email claim is missing.");
        }
    }

    public Guid WorkspaceId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirstValue("workspaceId");

            return Guid.TryParse(value, out var id)
                ? id
                : throw new InvalidOperationException("WorkspaceId claim is missing or invalid.");
        }
    }
}
