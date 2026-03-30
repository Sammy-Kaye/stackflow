// ValidationBehaviorTests — unit tests for ValidationBehavior<TRequest, TResponse>.
//
// Covered behaviours:
//   1. No validators registered → next() is called, handler result is returned unchanged
//   2. Validator fails, TResponse is Result (non-generic) → Result.Fail returned, next() NOT called
//   3. Validator fails, TResponse is Result<T> (generic) → Result.Fail<T> returned, no InvalidCastException, next() NOT called
//   4. Validator passes → next() is called, handler result is returned unchanged
//
// The test fixtures use minimal command records and inline validators so this file is
// self-contained and does not depend on any production command/query type.
//
// Test name format: {Method}_{Condition}_{ExpectedResult}

using FluentValidation;
using FluentValidation.Results;
using Moq;
using StackFlow.Application.Common;
using StackFlow.Application.Common.Behaviors;
using StackFlow.Application.Common.Mediator;

namespace StackFlow.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    // ── Minimal stub types used only in this test class ───────────────────────
    // These records must be public so Castle.DynamicProxy (used by Moq) can create
    // proxies for IValidator<T> from the strong-named FluentValidation assembly.
    // Private nested types are not accessible to DynamicProxyGenAssembly2 and cause
    // an ArgumentException at proxy-creation time.

    // A command that returns the non-generic Result
    public record NonGenericCommand : ICommand<Result>;

    // A command that returns the generic Result<string>
    public record GenericCommand : ICommand<Result<string>>;

    // ── Test 1: No validators registered → passes through ────────────────────

    [Fact]
    public async Task Handle_NoValidatorsRegistered_CallsNextAndReturnsHandlerResult()
    {
        // Arrange
        var behavior = new ValidationBehavior<GenericCommand, Result<string>>(
            validators: []);  // empty — no IValidator<GenericCommand> registered

        var expectedResult = Result.Ok<string>("ok-value");
        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await behavior.Handle(new GenericCommand(), CancellationToken.None, next);

        // Assert
        Assert.True(nextCalled, "next() must be called when no validators are registered");
        Assert.True(result.IsSuccess);
        Assert.Equal("ok-value", result.Value);
    }

    // ── Test 2: Validator fails, non-generic Result → short-circuit ──────────

    [Fact]
    public async Task Handle_ValidatorFails_NonGenericResponse_ReturnsFailResult_DoesNotCallNext()
    {
        // Arrange — a validator that always fails
        var validatorMock = new Mock<IValidator<NonGenericCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<NonGenericCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure("SomeField", "SomeField is required")
            ]));

        var behavior = new ValidationBehavior<NonGenericCommand, Result>(
            validators: [validatorMock.Object]);

        var nextCalled = false;
        RequestHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Ok());
        };

        // Act
        var result = await behavior.Handle(new NonGenericCommand(), CancellationToken.None, next);

        // Assert
        Assert.False(nextCalled, "next() must NOT be called when validation fails");
        Assert.False(result.IsSuccess);
        Assert.Contains("SomeField is required", result.Error);
    }

    // ── Test 3: Validator fails, generic Result<T> → no InvalidCastException ─

    [Fact]
    public async Task Handle_ValidatorFails_GenericResponse_ReturnsFailResultT_NoInvalidCastException()
    {
        // Arrange — a validator that always fails
        var validatorMock = new Mock<IValidator<GenericCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<GenericCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure("Name", "Name is required"),
                new ValidationFailure("Name", "Name must not exceed 200 characters")
            ]));

        var behavior = new ValidationBehavior<GenericCommand, Result<string>>(
            validators: [validatorMock.Object]);

        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Ok<string>("should not reach here"));
        };

        // Act — must not throw InvalidCastException
        var result = await behavior.Handle(new GenericCommand(), CancellationToken.None, next);

        // Assert — returned value is Result<string>, not base Result
        Assert.IsType<Result<string>>(result);
        Assert.False(nextCalled, "next() must NOT be called when validation fails");
        Assert.False(result.IsSuccess);

        // Both error messages must be joined with "; "
        Assert.Contains("Name is required", result.Error);
        Assert.Contains("Name must not exceed 200 characters", result.Error);
    }

    // ── Test 4: Validator passes → calls next ─────────────────────────────────

    [Fact]
    public async Task Handle_ValidatorPasses_CallsNextAndReturnsHandlerResult()
    {
        // Arrange — a validator that always passes (empty failures list)
        var validatorMock = new Mock<IValidator<GenericCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<GenericCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());  // no failures

        var behavior = new ValidationBehavior<GenericCommand, Result<string>>(
            validators: [validatorMock.Object]);

        var expectedResult = Result.Ok<string>("handler-value");
        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await behavior.Handle(new GenericCommand(), CancellationToken.None, next);

        // Assert
        Assert.True(nextCalled, "next() must be called when all validators pass");
        Assert.True(result.IsSuccess);
        Assert.Equal("handler-value", result.Value);
    }

    // ── Test 5: Multiple validators — all errors are joined ───────────────────

    [Fact]
    public async Task Handle_MultipleValidators_AllErrors_AreJoinedIntoSingleMessage()
    {
        // Arrange — two validators, each contributing one failure
        var firstValidator = new Mock<IValidator<GenericCommand>>();
        firstValidator
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<GenericCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("A", "Error from first")]));

        var secondValidator = new Mock<IValidator<GenericCommand>>();
        secondValidator
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<GenericCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("B", "Error from second")]));

        var behavior = new ValidationBehavior<GenericCommand, Result<string>>(
            validators: [firstValidator.Object, secondValidator.Object]);

        var nextCalled = false;
        RequestHandlerDelegate<Result<string>> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Ok<string>("nope"));
        };

        // Act
        var result = await behavior.Handle(new GenericCommand(), CancellationToken.None, next);

        // Assert
        Assert.False(nextCalled);
        Assert.False(result.IsSuccess);
        Assert.Contains("Error from first", result.Error);
        Assert.Contains("Error from second", result.Error);
    }
}
