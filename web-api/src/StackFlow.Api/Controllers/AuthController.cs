// AuthController — Feature 2: Dev Auth Stub
//
// Provides two endpoints:
//   POST /api/auth/dev-login  — issues a signed JWT for the hardcoded dev identity.
//                               Only active when ASPNETCORE_ENVIRONMENT = Development.
//   GET  /api/auth/me         — returns the decoded identity from the bearer token.
//                               Requires a valid JWT; returns 401 if absent or invalid.
//
// This controller is intentionally designed to be deleted in one operation when
// Phase 2 real authentication replaces it. Nothing outside this file depends on it.
//
// Design note: JWT generation lives here as a private method rather than in a handler
// because this is a dev-only stub with no domain entities, no repository access, and
// no business rules. Putting it in a handler would create infrastructure for zero gain.
// Once Phase 2 auth exists, this entire controller is replaced.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StackFlow.Application.Common.Mediator;
using StackFlow.Domain.Constants;

namespace StackFlow.Api.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    // ── Hardcoded stub identity ───────────────────────────────────────────────
    // These values are fixed constants for Phase 1 developer convenience.
    // They are not read from any database — there is no User entity at this stage.
    // Phase 2 will replace this controller entirely with real user lookup + JWT issuance.
    //
    // WellKnownIds.DemoWorkspaceId  = 00000000-0000-0000-0000-000000000001 (stub user)
    // WellKnownIds.GlobalWorkspaceId = 00000000-0000-0000-0000-000000000002 (stub workspace)
    private static readonly Guid StubUserId = WellKnownIds.DemoWorkspaceId;
    private static readonly Guid StubWorkspaceId = WellKnownIds.GlobalWorkspaceId;
    private const string StubEmail = "dev@stackflow.local";
    private const string StubRole = "Admin";
    private const int TokenExpiryHours = 24;

    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public AuthController(
        Mediator mediator,
        IConfiguration configuration,
        IHostEnvironment environment) : base(mediator)
    {
        _configuration = configuration;
        _environment = environment;
    }

    // ── POST /api/auth/dev-login ──────────────────────────────────────────────
    // Public endpoint — no [Authorize] attribute.
    // Only issues a token when running in the Development environment.
    // Returns 403 in any other environment so the route is harmless if somehow
    // reached outside Development (belt-and-suspenders on top of env checks).
    [HttpPost("dev-login")]
    public IActionResult DevLogin()
    {
        if (!_environment.IsDevelopment())
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { error = "Dev login is not available in this environment" });
        }

        var expiresAt = DateTime.UtcNow.AddHours(TokenExpiryHours);
        var token = BuildDevToken(expiresAt);

        return Ok(new
        {
            accessToken = token,
            expiresAt = expiresAt.ToString("O"),   // ISO 8601 with offset
            user = new
            {
                id = StubUserId.ToString(),
                email = StubEmail,
                role = StubRole,
                workspaceId = StubWorkspaceId.ToString()
            }
        });
    }

    // ── GET /api/auth/me ──────────────────────────────────────────────────────
    // JWT required — [Authorize] delegates 401 to the JwtBearerEvents.OnChallenge
    // handler registered in Program.cs, which returns { "error": "Unauthorised" }.
    // This endpoint reads claims directly from the validated token — no database hit.
    //
    // Claim name note: ASP.NET Core's JWT bearer middleware maps inbound JWT claim
    // names to .NET ClaimTypes by default (e.g. "sub" → ClaimTypes.NameIdentifier,
    // "email" → ClaimTypes.Email). We read using ClaimTypes.* to match what the
    // middleware actually stores on HttpContext.User after validation.
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var workspaceId = User.FindFirstValue("workspaceId");

        return Ok(new
        {
            id = userId,
            email,
            role,
            workspaceId
        });
    }

    // ── Private helper ────────────────────────────────────────────────────────
    // Builds and signs an HS256 JWT containing the hardcoded stub claims.
    // The secret is read from DevAuth:JwtSecret in appsettings.Development.json.
    // This method is only ever called from DevLogin() after the IsDevelopment() check.
    private string BuildDevToken(DateTime expiresAt)
    {
        var secret = _configuration["DevAuth:JwtSecret"]
            ?? throw new InvalidOperationException("DevAuth:JwtSecret is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, StubUserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, StubEmail),
            new Claim(ClaimTypes.Role, StubRole),
            new Claim("workspaceId", StubWorkspaceId.ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
