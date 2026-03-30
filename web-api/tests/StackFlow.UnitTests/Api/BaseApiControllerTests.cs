// BaseApiControllerTests — unit tests for BaseApiController.HandleResult mapping.
//
// Covered behaviours:
//   1. Success result → 200 OK with the value as the response body
//   2. Success result (non-generic) → 200 OK with empty body
//   3. Failure with "not found" substring (case-insensitive) → 404 Not Found with { "error": "..." }
//   4. Failure with "forbidden" substring (case-insensitive) → 403 Forbidden with no body
//   5. Failure with any other message → 400 Bad Request with { "error": "..." }
//
// The test uses a concrete implementation of BaseApiController since the class is abstract.
// All tests use HandleResult directly with mocked Result objects.
//
// Test name format: {Method}_{Condition}_{ExpectedResult}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using StackFlow.Application.Common;
using StackFlow.Application.Common.Mediator;
using StackFlow.Api.Controllers;

namespace StackFlow.UnitTests.Api;

public class BaseApiControllerTests
{
    // ── Test implementation of BaseApiController ──────────────────────────────
    // Required because BaseApiController is abstract; this minimal subclass is
    // used to test HandleResult directly.

    private class TestController : BaseApiController
    {
        public TestController(Mediator mediator) : base(mediator)
        {
        }

        // Expose HandleResult as public for testing
        public IActionResult TestHandleResultGeneric<T>(Result<T> result) => HandleResult(result);
        public IActionResult TestHandleResultNonGeneric(Result result) => HandleResult(result);
    }

    // ── Helper to create a TestController ───────────────────────────────────
    private static TestController CreateTestController()
    {
        var services = new ServiceCollection();
        services.AddScoped<Mediator>();
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();
        return new TestController(mediator);
    }

    // ── Test 1: Success generic result → 200 OK ────────────────────────────────

    [Fact]
    public void HandleResultT_SuccessResult_Returns200OkWithValue()
    {
        // Arrange
        var controller = CreateTestController();
        var successResult = Result.Ok<string>("test-value");

        // Act
        var response = controller.TestHandleResultGeneric(successResult);

        // Assert — response is OkObjectResult with the value
        var okResult = Assert.IsType<OkObjectResult>(response);
        Assert.Equal("test-value", okResult.Value);
        Assert.Equal(200, okResult.StatusCode);
    }

    // ── Test 2: Success non-generic result → 200 OK ────────────────────────────

    [Fact]
    public void HandleResult_SuccessResult_NonGeneric_Returns200Ok()
    {
        // Arrange
        var controller = CreateTestController();
        var successResult = Result.Ok();

        // Act
        var response = controller.TestHandleResultNonGeneric(successResult);

        // Assert — response is OkResult (no body)
        var okResult = Assert.IsType<OkResult>(response);
        Assert.Equal(200, okResult.StatusCode);
    }

    // ── Test 3: Failure "not found" (lowercase) → 404 Not Found ────────────────

    [Fact]
    public void HandleResultT_FailureWithNotFoundLowercase_Returns404NotFound()
    {
        // Arrange
        var controller = CreateTestController();
        var failureResult = Result.Fail<string>("Workflow not found");

        // Act
        var response = controller.TestHandleResultGeneric(failureResult);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.IsType<object>(notFoundResult.Value);  // { "error": "..." }
    }

    // ── Test 4: Failure "NOT FOUND" (uppercase) → 404 Not Found ────────────────

    [Fact]
    public void HandleResultT_FailureWithNotFoundUppercase_Returns404NotFound()
    {
        // Arrange
        var controller = CreateTestController();
        var failureResult = Result.Fail<string>("Entity NOT FOUND in database");

        // Act
        var response = controller.TestHandleResultGeneric(failureResult);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    // ── Test 5: Failure "forbidden" (lowercase) → 403 Forbidden ────────────────

    [Fact]
    public void HandleResultT_FailureWithForbiddenLowercase_Returns403Forbidden()
    {
        // Arrange
        var controller = CreateTestController();
        var failureResult = Result.Fail<string>("Access forbidden: insufficient permissions");

        // Act
        var response = controller.TestHandleResultGeneric(failureResult);

        // Assert
        var forbiddenResult = Assert.IsType<StatusCodeResult>(response);
        Assert.Equal(403, forbiddenResult.StatusCode);
    }

    // ── Test 6: Failure "FORBIDDEN" (uppercase) → 403 Forbidden ────────────────

    [Fact]
    public void HandleResultT_FailureWithForbiddenUppercase_Returns403Forbidden()
    {
        // Arrange
        var controller = CreateTestController();
        var failureResult = Result.Fail<string>("You do not have FORBIDDEN access to this resource");

        // Act
        var response = controller.TestHandleResultGeneric(failureResult);

        // Assert
        var forbiddenResult = Assert.IsType<StatusCodeResult>(response);
        Assert.Equal(403, forbiddenResult.StatusCode);
    }

    // ── Test 7: Failure generic error message → 400 Bad Request ────────────────

    [Fact]
    public void HandleResultT_FailureWithGenericError_Returns400BadRequest()
    {
        // Arrange
        var controller = CreateTestController();
        var failureResult = Result.Fail<string>("Validation failed: name is required");

        // Act
        var response = controller.TestHandleResultGeneric(failureResult);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.IsType<object>(badRequestResult.Value);  // { "error": "..." }
    }

    // ── Test 8: Non-generic failure with "not found" → 404 Not Found ──────────

    [Fact]
    public void HandleResult_NonGeneric_FailureWithNotFound_Returns404NotFound()
    {
        // Arrange
        var controller = CreateTestController();
        var failureResult = Result.Fail("Resource not found");

        // Act
        var response = controller.TestHandleResultNonGeneric(failureResult);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    // ── Test 9: Non-generic failure with "forbidden" → 403 Forbidden ──────────

    [Fact]
    public void HandleResult_NonGeneric_FailureWithForbidden_Returns403Forbidden()
    {
        // Arrange
        var controller = CreateTestController();
        var failureResult = Result.Fail("You are forbidden from doing this");

        // Act
        var response = controller.TestHandleResultNonGeneric(failureResult);

        // Assert
        var forbiddenResult = Assert.IsType<StatusCodeResult>(response);
        Assert.Equal(403, forbiddenResult.StatusCode);
    }

    // ── Test 10: Non-generic failure generic error → 400 Bad Request ────────────

    [Fact]
    public void HandleResult_NonGeneric_FailureWithGenericError_Returns400BadRequest()
    {
        // Arrange
        var controller = CreateTestController();
        var failureResult = Result.Fail("Operation failed");

        // Act
        var response = controller.TestHandleResultNonGeneric(failureResult);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    // ── Test 11: "not found" takes precedence over "forbidden" ─────────────────
    // If an error message contains both "not found" and "forbidden", the "not found"
    // check runs first and returns 404.

    [Fact]
    public void HandleResultT_ErrorContainsBothNotFoundAndForbidden_ReturnsNotFoundTakesPrecedence()
    {
        // Arrange
        var controller = CreateTestController();
        var failureResult = Result.Fail<string>("The forbidden resource was not found");

        // Act
        var response = controller.TestHandleResultGeneric(failureResult);

        // Assert — "not found" check runs first
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}
