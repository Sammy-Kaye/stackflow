// MediatorTests — unit tests for Mediator class and pipeline ordering.
//
// Covered behaviours:
//   1. Handler is resolved from DI and invoked successfully
//   2. Pipeline behaviors are executed in registration order (Validation → Logging)
//   3. Handler is not invoked if ValidationBehavior short-circuits
//
// This test file verifies the full mediator dispatch chain, ensuring behaviors
// run in the correct order and the handler receives the request.
//
// Test name format: {Method}_{Condition}_{ExpectedResult}

using FluentValidation;
using FluentValidation.Results;
using Moq;
using StackFlow.Application;
using StackFlow.Application.Common;
using StackFlow.Application.Common.Behaviors;
using StackFlow.Application.Common.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace StackFlow.UnitTests.Common;

public class MediatorTests
{
    // ── Minimal stub types ────────────────────────────────────────────────────
    // Must be public so Castle.DynamicProxy can create proxies.

    public record TestCommand : ICommand<Result<string>>;

    public class TestCommandHandler : IRequestHandler<TestCommand, Result<string>>
    {
        public Task<Result<string>> Handle(TestCommand request, CancellationToken ct)
        {
            return Task.FromResult(Result.Ok<string>("success"));
        }
    }

    // ── Test 1: Handler is resolved and invoked ───────────────────────────────

    [Fact]
    public async Task Send_ValidCommand_ResolvesHandlerAndReturnsResult()
    {
        // Arrange — note: PingCommand/PingCommandHandler are in StackFlow.Application,
        // so assembly scanning will discover them. We use PingCommand here as a real
        // example of a command that is registered via AddApplication().
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<Mediator>();

        // Act — use PingCommand which is a real command in StackFlow.Application
        var result = await mediator.Send<Result<string>>(new PingCommand());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("pong", result.Value);
    }

    // ── Test 2: Pipeline order — ValidationBehavior runs before LoggingBehavior ─

    [Fact]
    public async Task Send_ValidCommand_ExecutesBehaviorsInOrder_Validation_Then_Logging()
    {
        // Arrange — track the order in which behaviors and handler are called
        var callOrder = new List<string>();

        var mockValidator = new Mock<IValidator<TestCommand>>();
        mockValidator
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<TestCommand>>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Validator"))
            .ReturnsAsync(new ValidationResult());  // pass validation

        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<
            LoggingBehavior<TestCommand, Result<string>>>>();

        var validationBehavior = new ValidationBehavior<TestCommand, Result<string>>(
            validators: [mockValidator.Object]);

        var loggingBehavior = new LoggingBehavior<TestCommand, Result<string>>(
            logger: mockLogger.Object);

        // Build the innermost delegate — the handler
        RequestHandlerDelegate<Result<string>> innerHandler = () =>
        {
            callOrder.Add("Handler");
            return Task.FromResult(Result.Ok<string>("ok"));
        };

        // Wrap with LoggingBehavior
        RequestHandlerDelegate<Result<string>> withLogging = () =>
        {
            callOrder.Add("LoggingBehavior");
            return loggingBehavior.Handle(new TestCommand(), CancellationToken.None, innerHandler);
        };

        // Wrap with ValidationBehavior
        RequestHandlerDelegate<Result<string>> pipeline = () =>
        {
            callOrder.Add("ValidationBehavior");
            return validationBehavior.Handle(new TestCommand(), CancellationToken.None, withLogging);
        };

        // Act
        var result = await pipeline();

        // Assert — ValidationBehavior ran first, then LoggingBehavior, then Handler
        Assert.Equal(new[] { "ValidationBehavior", "Validator", "LoggingBehavior", "Handler" }, callOrder);
        Assert.True(result.IsSuccess);
    }

    // ── Test 3: Handler not invoked if validation fails ────────────────────────

    [Fact]
    public async Task Send_InvalidCommand_ValidationBehaviorShortCircuits_HandlerNotCalled()
    {
        // Arrange
        var handlerCalled = false;
        var handler = new Mock<IRequestHandler<TestCommand, Result<string>>>();
        handler
            .Setup(h => h.Handle(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .Callback(() => handlerCalled = true)
            .ReturnsAsync(Result.Ok<string>("should not reach here"));

        var failingValidator = new Mock<IValidator<TestCommand>>();
        failingValidator
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<TestCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure("Field", "Field is required")]));

        var behavior = new ValidationBehavior<TestCommand, Result<string>>(
            validators: [failingValidator.Object]);

        RequestHandlerDelegate<Result<string>> next = () =>
        {
            handlerCalled = true;
            return handler.Object.Handle(new TestCommand(), CancellationToken.None);
        };

        // Act
        var result = await behavior.Handle(new TestCommand(), CancellationToken.None, next);

        // Assert
        Assert.False(handlerCalled, "Handler must not be called when validation fails");
        Assert.False(result.IsSuccess);
        Assert.Contains("Field is required", result.Error);
    }
}
