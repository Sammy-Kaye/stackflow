// ICurrentUserService — provides the identity of the authenticated user to handlers.
//
// WorkspaceId and UserId are sourced from the JWT bearer token claims that were
// validated by the ASP.NET Core authentication middleware before the request reached
// any handler. Handlers never accept WorkspaceId or UserId from the request body —
// they always read from this service.
//
// The Infrastructure layer implements this using IHttpContextAccessor to extract
// claims from HttpContext.User. Handlers depend on this interface only — they have
// no knowledge of HTTP or claims.

namespace StackFlow.Application.Common.Interfaces;

/// <summary>
/// Provides the authenticated user's identity to application-layer handlers.
/// Implemented in the API layer using JWT claims from HttpContext.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>The authenticated user's ID from the JWT sub claim.</summary>
    Guid UserId { get; }

    /// <summary>The authenticated user's email address from the JWT email claim.</summary>
    string Email { get; }

    /// <summary>The workspace the authenticated user belongs to, from the JWT workspaceId claim.</summary>
    Guid WorkspaceId { get; }
}
