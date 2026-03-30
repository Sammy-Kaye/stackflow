// LoggingBehaviorTests — unit tests for LoggingBehavior<TRequest, TResponse>.
//
// Covered behaviours:
//   1. Success path → logs at Information level; log message contains request type name and elapsed ms
//   2. Failure path → logs at Warning level; log message contains the error message
//
// ILogger<T> is mocked using Moq. Because the structured logging methods on ILogger<T>
// are extension methods that ultimately call ILogger.Log(), we verify calls against the
// underlying Log() method using It.Is<> matchers on LogLevel and the formatted state.
//
// Test name format: {Method}_{Condition}_{ExpectedResult}

using Microsoft.Extensions.Logging;
using Moq;
using StackFlow.Application.Common;
using StackFlow.Application.Common.Behaviors;
using StackFlow.Application.Common.Mediator;

namespace StackFlow.UnitTests.Behaviors;

public class LoggingBehaviorTests
{
    // ── Minimal stub type ─────────────────────────────────────────────────────
    // Must be public so Castle.DynamicProxy (used by Moq) can create proxies for
    // ILogger<LoggingBehavior<SampleCommand, Result<string>>> from the strong-named
    // Microsoft.Extensions.Logging.Abstractions assembly. Private nested types cause
    // an ArgumentException at proxy-creation time.

    public record SampleCommand : ICommand<Result<string>>;

    // ── Helper ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a mock ILogger for LoggingBehavior and returns the behavior and mock together.
    /// </summary>
    private static (LoggingBehavior<SampleCommand, Result<string>> behavior,
                    Mock<ILogger<LoggingBehavior<SampleCommand, Result<string>>>> loggerMock)
        BuildBehaviorWithLogger()
    {
        var loggerMock = new Mock<ILogger<LoggingBehavior<SampleCommand, Result<string>>>>();
        var behavior = new LoggingBehavior<SampleCommand, Result<string>>(loggerMock.Object);
        return (behavior, loggerMock);
    }

    // ── Test 1: Success path ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SuccessResult_LogsAtInformationLevel()
    {
        // Arrange
        var (behavior, loggerMock) = BuildBehaviorWithLogger();
        RequestHandlerDelegate<Result<string>> next =
            () => Task.FromResult(Result.Ok<string>("ok"));

        // Act
        await behavior.Handle(new SampleCommand(), CancellationToken.None, next);

        // Assert — ILogger extension methods delegate to ILogger.Log<TState>().
        // We verify a call was made at the Information level.
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Expected exactly one Information-level log call on success");
    }

    [Fact]
    public async Task Handle_SuccessResult_LogMessageContainsRequestTypeName()
    {
        // Arrange
        var (behavior, loggerMock) = BuildBehaviorWithLogger();
        var loggedMessages = new List<string>();

        loggerMock
            .Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(new InvocationAction(invocation =>
            {
                // invocation.Arguments[2] is the TState (FormattedLogValues).
                // invocation.Arguments[4] is the Func<TState, Exception?, string> formatter.
                // Calling the formatter produces the final rendered log string.
                var state = invocation.Arguments[2];
                var formatter = invocation.Arguments[4];
                var formatterType = formatter.GetType();
                var invokeMethod = formatterType.GetMethod("Invoke")!;
                var rendered = (string)invokeMethod.Invoke(formatter, [state, null])!;
                loggedMessages.Add(rendered);
            }));

        RequestHandlerDelegate<Result<string>> next =
            () => Task.FromResult(Result.Ok<string>("ok"));

        // Act
        await behavior.Handle(new SampleCommand(), CancellationToken.None, next);

        // Assert — the rendered message must contain the request type name
        Assert.True(loggedMessages.Count > 0, "No Information log was captured");
        Assert.Contains("SampleCommand", loggedMessages[0]);
    }

    [Fact]
    public async Task Handle_SuccessResult_LogMessageContainsElapsedMs()
    {
        // Arrange
        var (behavior, loggerMock) = BuildBehaviorWithLogger();
        var loggedMessages = new List<string>();

        loggerMock
            .Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(new InvocationAction(invocation =>
            {
                var state = invocation.Arguments[2];
                var formatter = invocation.Arguments[4];
                var invokeMethod = formatter.GetType().GetMethod("Invoke")!;
                var rendered = (string)invokeMethod.Invoke(formatter, [state, null])!;
                loggedMessages.Add(rendered);
            }));

        RequestHandlerDelegate<Result<string>> next =
            () => Task.FromResult(Result.Ok<string>("ok"));

        // Act
        await behavior.Handle(new SampleCommand(), CancellationToken.None, next);

        // Assert — the structured log value for ElapsedMs must appear in the rendered string.
        // LoggingBehavior uses the parameter name "ElapsedMs".
        Assert.True(loggedMessages.Count > 0, "No Information log was captured");
        Assert.True(loggedMessages[0].Contains("ms"),
            $"Expected 'ms' (elapsed milliseconds) in log message but got: {loggedMessages[0]}");
    }

    [Fact]
    public async Task Handle_SuccessResult_DoesNotLogAtWarningLevel()
    {
        // Arrange
        var (behavior, loggerMock) = BuildBehaviorWithLogger();
        RequestHandlerDelegate<Result<string>> next =
            () => Task.FromResult(Result.Ok<string>("ok"));

        // Act
        await behavior.Handle(new SampleCommand(), CancellationToken.None, next);

        // Assert — no Warning log on success
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            "Expected no Warning-level log call on success");
    }

    // ── Test 2: Failure path ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_FailureResult_LogsAtWarningLevel()
    {
        // Arrange
        var (behavior, loggerMock) = BuildBehaviorWithLogger();
        RequestHandlerDelegate<Result<string>> next =
            () => Task.FromResult(Result.Fail<string>("Something went wrong"));

        // Act
        await behavior.Handle(new SampleCommand(), CancellationToken.None, next);

        // Assert — exactly one Warning-level log call on failure
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Expected exactly one Warning-level log call on failure");
    }

    [Fact]
    public async Task Handle_FailureResult_LogMessageContainsErrorMessage()
    {
        // Arrange
        const string errorMessage = "Workflow not found";
        var (behavior, loggerMock) = BuildBehaviorWithLogger();
        var loggedMessages = new List<string>();

        loggerMock
            .Setup(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(new InvocationAction(invocation =>
            {
                var state = invocation.Arguments[2];
                var formatter = invocation.Arguments[4];
                var invokeMethod = formatter.GetType().GetMethod("Invoke")!;
                var rendered = (string)invokeMethod.Invoke(formatter, [state, null])!;
                loggedMessages.Add(rendered);
            }));

        RequestHandlerDelegate<Result<string>> next =
            () => Task.FromResult(Result.Fail<string>(errorMessage));

        // Act
        await behavior.Handle(new SampleCommand(), CancellationToken.None, next);

        // Assert — the rendered warning message must contain the error
        Assert.True(loggedMessages.Count > 0, "No Warning log was captured");
        Assert.Contains(errorMessage, loggedMessages[0]);
    }

    [Fact]
    public async Task Handle_FailureResult_DoesNotLogAtInformationLevel()
    {
        // Arrange
        var (behavior, loggerMock) = BuildBehaviorWithLogger();
        RequestHandlerDelegate<Result<string>> next =
            () => Task.FromResult(Result.Fail<string>("error"));

        // Act
        await behavior.Handle(new SampleCommand(), CancellationToken.None, next);

        // Assert — no Information log on failure
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            "Expected no Information-level log call on failure");
    }

    [Fact]
    public async Task Handle_FailureResult_StillReturnsHandlerResult()
    {
        // Arrange — failure behavior must not swallow the response
        var (behavior, _) = BuildBehaviorWithLogger();
        var failResult = Result.Fail<string>("handler failure");
        RequestHandlerDelegate<Result<string>> next = () => Task.FromResult(failResult);

        // Act
        var result = await behavior.Handle(new SampleCommand(), CancellationToken.None, next);

        // Assert — the same failed result is returned to the caller unchanged
        Assert.False(result.IsSuccess);
        Assert.Equal("handler failure", result.Error);
    }
}
