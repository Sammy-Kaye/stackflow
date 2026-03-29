// AuthEndpointTests — integration tests for POST /api/auth/dev-login and GET /api/auth/me.
//
// Covered acceptance criteria:
//   AC1: POST /api/auth/dev-login in Development → 200 with correct response shape
//   AC2: GET  /api/auth/me with a valid bearer token → 200 with correct claims
//   AC3: GET  /api/auth/me with no bearer token → 401 with { "error": "Unauthorised" }
//   AC4: POST /api/auth/dev-login in Production → 403 with error message
//
// The _devFactory overrides ASPNETCORE_ENVIRONMENT to "Development", which causes
// appsettings.Development.json (copied to the test output directory from the API project)
// to be loaded. Both the controller (signing) and the JWT bearer middleware (validating)
// read DevAuth:JwtSecret from the same configuration, so the keys always match.
//
// For AC4, a separate factory overrides the environment to "Production". The JWT bearer
// middleware in that factory uses the placeholder key from Program.cs (no DevAuth:JwtSecret
// is provided), but since no token is ever issued, this is irrelevant.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StackFlow.IntegrationTests.Api;

public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    // ── Shared test constants ─────────────────────────────────────────────────
    // Stub identity values copied from AuthController — used to assert claim content.
    private const string StubUserId = "00000000-0000-0000-0000-000000000001";
    private const string StubEmail = "dev@stackflow.local";
    private const string StubRole = "Admin";
    private const string StubWorkspaceId = "00000000-0000-0000-0000-000000000002";

    private readonly WebApplicationFactory<Program> _baseFactory;
    private readonly WebApplicationFactory<Program> _devFactory;
    private readonly HttpClient _devClient;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;

        // Child factory: Development environment only.
        // Setting Development causes appsettings.Development.json (copied to the test
        // output directory) to be loaded. Both the controller's BuildDevToken() and the
        // JWT bearer middleware read DevAuth:JwtSecret from the same IConfiguration,
        // so the signing and validation keys always match.
        _devFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        });

        _devClient = _devFactory.CreateClient();
    }

    public void Dispose() => _devFactory.Dispose();

    // ── AC1 ───────────────────────────────────────────────────────────────────
    // POST /api/auth/dev-login in Development returns 200 with a non-empty token
    // and a user object matching the hardcoded stub identity.
    [Fact]
    public async Task POST_DevLogin_InDevelopment_Returns200_WithCorrectShape()
    {
        // Act
        var response = await _devClient.PostAsync("/api/auth/dev-login", content: null);

        // Assert — status
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert — body shape and stub values
        var body = await response.Content.ReadFromJsonAsync<DevLoginResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.ExpiresAt.Should().NotBeNullOrEmpty();
        body.User.Should().NotBeNull();
        body.User!.Id.Should().Be(StubUserId);
        body.User.Email.Should().Be(StubEmail);
        body.User.Role.Should().Be(StubRole);
        body.User.WorkspaceId.Should().Be(StubWorkspaceId);
    }

    // ── AC2 ───────────────────────────────────────────────────────────────────
    // GET /api/auth/me with a valid bearer token returns 200 and the claims in the
    // response body match the stub identity that was encoded into the token.
    [Fact]
    public async Task GET_Me_WithValidToken_Returns200_WithCorrectClaims()
    {
        // Arrange — obtain a token via dev-login
        var loginResponse = await _devClient.PostAsync("/api/auth/dev-login", content: null);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<DevLoginResponse>();
        loginBody.Should().NotBeNull();

        // Act — call /me with the bearer token
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);
        var response = await _devClient.SendAsync(request);

        // Assert — status
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert — claims match the stub identity
        var body = await response.Content.ReadFromJsonAsync<MeResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(StubUserId);
        body.Email.Should().Be(StubEmail);
        body.Role.Should().Be(StubRole);
        body.WorkspaceId.Should().Be(StubWorkspaceId);
    }

    // ── AC3 ───────────────────────────────────────────────────────────────────
    // GET /api/auth/me with no Authorization header returns 401.
    // The JwtBearerEvents.OnChallenge handler in Program.cs writes the exact JSON body.
    [Fact]
    public async Task GET_Me_WithNoToken_Returns401_WithErrorBody()
    {
        // Act — no Authorization header set
        var response = await _devClient.GetAsync("/api/auth/me");

        // Assert — status
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Assert — body matches the exact JSON the OnChallenge handler writes
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Should().Be("Unauthorised");
    }

    // ── AC4 ───────────────────────────────────────────────────────────────────
    // POST /api/auth/dev-login in Production returns 403.
    // The controller's IsDevelopment() check returns false and the route is harmless.
    [Fact]
    public async Task POST_DevLogin_InProduction_Returns403_WithErrorBody()
    {
        // Arrange — production-environment factory derived from the base fixture
        await using var productionFactory = _baseFactory.WithWebHostBuilder(
            builder => builder.UseEnvironment("Production"));
        var productionClient = productionFactory.CreateClient();

        // Act
        var response = await productionClient.PostAsync("/api/auth/dev-login", content: null);

        // Assert — status
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Assert — body matches the controller's 403 response shape
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Should().Be("Dev login is not available in this environment");
    }

    // ── Local record types ────────────────────────────────────────────────────
    // Defined here to keep this file self-contained.
    // Property names are PascalCase — System.Text.Json deserialisation is case-insensitive
    // by default via ReadFromJsonAsync, so they match the camelCase API responses.
    private record DevLoginResponse(string AccessToken, string ExpiresAt, UserDto? User);
    private record UserDto(string Id, string Email, string Role, string WorkspaceId);
    private record MeResponse(string? Id, string? Email, string? Role, string? WorkspaceId);
    private record ErrorResponse(string Error);
}
