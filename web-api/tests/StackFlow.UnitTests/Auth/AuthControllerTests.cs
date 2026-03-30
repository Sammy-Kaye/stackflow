// AuthControllerTests — unit tests for AuthController.
//
// What is tested here:
//   DevLogin() — environment branch: returns 200 + token body in Development,
//                returns 403 + error body in any other environment.
//   Me()       — claim extraction: reads ClaimTypes.NameIdentifier, ClaimTypes.Email,
//                ClaimTypes.Role, and the custom "workspaceId" claim from the validated
//                principal stored on HttpContext.User; returns them in a 200 OK body.
//
// What is NOT tested here:
//   JWT signing validity, token expiry, or bearer middleware — those are integration
//   concerns covered by AuthEndpointTests. Unit tests only verify the controller's
//   branching logic and claim-to-response mapping.
//
// Dependencies mocked:
//   IConfiguration   — returns the dev JWT secret so BuildDevToken() does not throw.
//   IHostEnvironment — controls IsDevelopment() so both branches are exercisable.
//   HttpContext      — supplies a populated ClaimsPrincipal for Me() tests.
//
// Test name format: {Method}_{Condition}_{ExpectedResult}

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using StackFlow.Api.Controllers;
using StackFlow.Application.Common.Mediator;

namespace StackFlow.UnitTests.Auth;

public class AuthControllerTests
{
    // ── Shared constants ──────────────────────────────────────────────────────
    // Copied from AuthController — used to assert response body values without
    // coupling tests to the controller's private fields.
    private const string StubUserId = "00000000-0000-0000-0000-000000000001";
    private const string StubWorkspaceId = "00000000-0000-0000-0000-000000000002";
    private const string StubEmail = "dev@stackflow.local";
    private const string StubRole = "Admin";

    // A sufficiently long secret — BuildDevToken() requires at least 32 characters
    // for HS256. The exact value does not matter for unit tests; only the integration
    // tests need it to match the middleware validation key.
    private const string TestJwtSecret = "unit-test-secret-that-is-long-enough-for-hs256-signing";

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates an IConfiguration mock that returns TestJwtSecret for "DevAuth:JwtSecret".
    /// </summary>
    private static Mock<IConfiguration> BuildConfigMock()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["DevAuth:JwtSecret"]).Returns(TestJwtSecret);
        return configMock;
    }

    /// <summary>
    /// Creates an IHostEnvironment mock with IsDevelopment() returning the given value.
    /// </summary>
    private static Mock<IHostEnvironment> BuildEnvironmentMock(bool isDevelopment)
    {
        var envMock = new Mock<IHostEnvironment>();
        // EnvironmentName is the string that IsDevelopment() reads internally.
        envMock.Setup(e => e.EnvironmentName)
               .Returns(isDevelopment ? Environments.Development : Environments.Production);
        return envMock;
    }

    /// <summary>
    /// Constructs a controller with optional HttpContext injection for Me() tests.
    /// A real Mediator backed by an empty IServiceProvider is passed to BaseApiController.
    /// AuthController's own logic never dispatches via the mediator, so the provider
    /// returning null for all service requests is safe here.
    /// </summary>
    private static AuthController BuildController(
        Mock<IConfiguration> configMock,
        Mock<IHostEnvironment> envMock,
        ClaimsPrincipal? user = null)
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        var mediator = new Mediator(serviceProviderMock.Object);
        var controller = new AuthController(mediator, configMock.Object, envMock.Object);

        if (user is not null)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        return controller;
    }

    // ── DevLogin — Development environment ───────────────────────────────────

    [Fact]
    public void DevLogin_InDevelopment_Returns200OkResult()
    {
        // Arrange
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: true));

        // Act
        var result = controller.DevLogin();

        // Assert — outer result type is OkObjectResult (200 OK)
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
    }

    [Fact]
    public void DevLogin_InDevelopment_ResponseContainsNonEmptyAccessToken()
    {
        // Arrange
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: true));

        // Act
        var result = controller.DevLogin();

        // Assert — the anonymous response object carries a non-empty accessToken
        var ok = Assert.IsType<OkObjectResult>(result);
        var accessToken = GetProperty<string>(ok.Value!, "accessToken");
        Assert.False(string.IsNullOrEmpty(accessToken));
    }

    [Fact]
    public void DevLogin_InDevelopment_ResponseContainsNonEmptyExpiresAt()
    {
        // Arrange
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: true));

        // Act
        var result = controller.DevLogin();

        // Assert — expiresAt is present and parseable as a date/time
        var ok = Assert.IsType<OkObjectResult>(result);
        var expiresAt = GetProperty<string>(ok.Value!, "expiresAt");
        Assert.False(string.IsNullOrEmpty(expiresAt));
        Assert.True(DateTime.TryParse(expiresAt, out _),
            $"expiresAt '{expiresAt}' could not be parsed as a DateTime.");
    }

    [Fact]
    public void DevLogin_InDevelopment_ExpiresAtIsApproximately24HoursFromNow()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: true));

        // Act
        var result = controller.DevLogin();
        var after = DateTime.UtcNow;

        // Assert — expiresAt falls within [before+24h, after+24h] with a 5-second window
        var ok = Assert.IsType<OkObjectResult>(result);
        var expiresAtStr = GetProperty<string>(ok.Value!, "expiresAt");
        var expiresAt = DateTime.Parse(expiresAtStr).ToUniversalTime();

        Assert.True(expiresAt >= before.AddHours(24).AddSeconds(-5),
            $"expiresAt {expiresAt:O} is earlier than expected lower bound {before.AddHours(24):O}");
        Assert.True(expiresAt <= after.AddHours(24).AddSeconds(5),
            $"expiresAt {expiresAt:O} is later than expected upper bound {after.AddHours(24):O}");
    }

    [Fact]
    public void DevLogin_InDevelopment_UserObjectContainsStubIdentity()
    {
        // Arrange
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: true));

        // Act
        var result = controller.DevLogin();

        // Assert — the nested user object carries the exact hardcoded stub values
        var ok = Assert.IsType<OkObjectResult>(result);
        var user = GetProperty<object>(ok.Value!, "user");
        Assert.NotNull(user);

        Assert.Equal(StubUserId,      GetProperty<string>(user, "id"));
        Assert.Equal(StubEmail,       GetProperty<string>(user, "email"));
        Assert.Equal(StubRole,        GetProperty<string>(user, "role"));
        Assert.Equal(StubWorkspaceId, GetProperty<string>(user, "workspaceId"));
    }

    // ── DevLogin — non-Development environment ────────────────────────────────

    [Fact]
    public void DevLogin_InProduction_Returns403StatusCode()
    {
        // Arrange
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: false));

        // Act
        var result = controller.DevLogin();

        // Assert — ObjectResult (StatusCode 403), not OkObjectResult
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    [Fact]
    public void DevLogin_InProduction_ResponseBodyContainsErrorMessage()
    {
        // Arrange
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: false));

        // Act
        var result = controller.DevLogin();

        // Assert — the error message matches the API contract exactly
        var statusResult = Assert.IsType<ObjectResult>(result);
        var error = GetProperty<string>(statusResult.Value!, "error");
        Assert.Equal("Dev login is not available in this environment", error);
    }

    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    [InlineData("QA")]
    public void DevLogin_InAnyNonDevelopmentEnvironment_Returns403(string environmentName)
    {
        // Arrange — build an environment mock with a specific non-Development name
        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns(environmentName);
        var controller = BuildController(BuildConfigMock(), envMock);

        // Act
        var result = controller.DevLogin();

        // Assert — all non-Development environments must return 403, never 200
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
    }

    // ── Me — claim extraction ─────────────────────────────────────────────────

    [Fact]
    public void Me_WithAllClaimsPresent_Returns200OkResult()
    {
        // Arrange — build a ClaimsPrincipal with the four expected claims
        var user = BuildStubPrincipal();
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: true), user);

        // Act
        var result = controller.Me();

        // Assert — 200 OK
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void Me_WithAllClaimsPresent_ReturnsCorrectIdFromSubClaim()
    {
        // Arrange
        var user = BuildStubPrincipal();
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: true), user);

        // Act
        var result = controller.Me();

        // Assert — "id" in the response comes from ClaimTypes.NameIdentifier (mapped from "sub")
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StubUserId, GetProperty<string>(ok.Value!, "id"));
    }

    [Fact]
    public void Me_WithAllClaimsPresent_ReturnsCorrectEmailClaim()
    {
        // Arrange
        var user = BuildStubPrincipal();
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: true), user);

        // Act
        var result = controller.Me();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StubEmail, GetProperty<string>(ok.Value!, "email"));
    }

    [Fact]
    public void Me_WithAllClaimsPresent_ReturnsCorrectRoleClaim()
    {
        // Arrange
        var user = BuildStubPrincipal();
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: true), user);

        // Act
        var result = controller.Me();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StubRole, GetProperty<string>(ok.Value!, "role"));
    }

    [Fact]
    public void Me_WithAllClaimsPresent_ReturnsCorrectWorkspaceIdClaim()
    {
        // Arrange
        var user = BuildStubPrincipal();
        var controller = BuildController(BuildConfigMock(), BuildEnvironmentMock(isDevelopment: true), user);

        // Act
        var result = controller.Me();

        // Assert — workspaceId is a custom claim name (not a ClaimTypes.* constant)
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StubWorkspaceId, GetProperty<string>(ok.Value!, "workspaceId"));
    }

    [Fact]
    public void Me_WithNoClaimsOnPrincipal_Returns200WithNullFields()
    {
        // Arrange — empty principal: simulates a token with no claims (edge case)
        var emptyPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        var controller = BuildController(
            BuildConfigMock(), BuildEnvironmentMock(isDevelopment: true), emptyPrincipal);

        // Act
        var result = controller.Me();

        // Assert — Me() does not throw; it returns 200 with null fields.
        // The [Authorize] middleware would reject an unauthenticated request before
        // it reaches this point in production, so null values here are acceptable.
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Null(GetProperty<string?>(ok.Value!, "id"));
        Assert.Null(GetProperty<string?>(ok.Value!, "email"));
        Assert.Null(GetProperty<string?>(ok.Value!, "role"));
        Assert.Null(GetProperty<string?>(ok.Value!, "workspaceId"));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds a ClaimsPrincipal with the four stub identity claims that the JWT
    /// bearer middleware would produce after validating a dev-login token.
    ///
    /// ASP.NET Core's JWT bearer middleware maps inbound claim names:
    ///   "sub"   → ClaimTypes.NameIdentifier
    ///   "email" → ClaimTypes.Email
    /// Role and workspaceId are read directly by their claim type names.
    /// </summary>
    private static ClaimsPrincipal BuildStubPrincipal() =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, StubUserId),
            new Claim(ClaimTypes.Email,          StubEmail),
            new Claim(ClaimTypes.Role,            StubRole),
            new Claim("workspaceId",              StubWorkspaceId),
        ], authenticationType: "TestAuth"));

    /// <summary>
    /// Reads a named property from an anonymous object returned by a controller action.
    /// Anonymous types are sealed; reflection is the only way to inspect them from outside
    /// the declaring assembly in a unit test.
    /// </summary>
    private static T GetProperty<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"Property '{propertyName}' not found on {obj.GetType().Name}. " +
                $"Available: {string.Join(", ", obj.GetType().GetProperties().Select(p => p.Name))}");

        return (T)property.GetValue(obj)!;
    }
}
