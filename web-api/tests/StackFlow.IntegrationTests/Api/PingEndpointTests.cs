// PingEndpointTests — integration tests for GET /api/ping.
//
// Covered acceptance criteria:
//   AC1: GET /api/ping with a valid dev JWT → 200 OK with body { "message": "pong" }
//   AC2: GET /api/ping with no JWT → 401 Unauthorized with { "error": "Unauthorised" }
//
// The _devFactory overrides ASPNETCORE_ENVIRONMENT to "Development" so the
// dev-login endpoint issues a signed token and the JWT bearer middleware validates
// it — both sides read DevAuth:JwtSecret from the same appsettings.Development.json.
// This is identical to the pattern used in AuthEndpointTests.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StackFlow.IntegrationTests.Api;

public class PingEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _devFactory;
    private readonly HttpClient _devClient;

    public PingEndpointTests(WebApplicationFactory<Program> factory)
    {
        // Child factory with Development environment so appsettings.Development.json
        // is loaded and DevAuth:JwtSecret is available for both token issuance and validation.
        _devFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        });

        _devClient = _devFactory.CreateClient();
    }

    public void Dispose() => _devFactory.Dispose();

    // ── AC1 ───────────────────────────────────────────────────────────────────
    // GET /api/ping with a valid bearer token → 200 OK with { "message": "pong" }

    [Fact]
    public async Task GET_Ping_WithValidToken_Returns200_WithPongBody()
    {
        // Arrange — obtain a dev token first
        var loginResponse = await _devClient.PostAsync("/api/auth/dev-login", content: null);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "dev-login must succeed before the ping test can proceed");

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<DevLoginResponse>();
        loginBody.Should().NotBeNull();
        loginBody!.AccessToken.Should().NotBeNullOrEmpty();

        // Act — call /api/ping with the bearer token
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/ping");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginBody.AccessToken);
        var response = await _devClient.SendAsync(request);

        // Assert — status
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert — body shape: { "message": "pong" }
        var body = await response.Content.ReadFromJsonAsync<PingResponse>();
        body.Should().NotBeNull();
        body!.Message.Should().Be("pong");
    }

    // ── AC2 ───────────────────────────────────────────────────────────────────
    // GET /api/ping with no Authorization header → 401 with { "error": "Unauthorised" }

    [Fact]
    public async Task GET_Ping_WithNoToken_Returns401_WithErrorBody()
    {
        // Act — no Authorization header
        var response = await _devClient.GetAsync("/api/ping");

        // Assert — status
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Assert — body matches the exact JSON the OnChallenge handler in Program.cs writes
        var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Should().Be("Unauthorised");
    }

    // ── Local record types ────────────────────────────────────────────────────
    // Defined here to keep this file self-contained.
    // Property names are PascalCase — System.Text.Json deserialisation is case-insensitive
    // by default via ReadFromJsonAsync, so they match the camelCase API responses.
    private record DevLoginResponse(string AccessToken, string ExpiresAt);
    private record PingResponse(string Message);
    private record ErrorResponse(string Error);
}
