// ResultTests — unit tests for the Result and Result<T> types.
//
// These tests lock in the contract between the Result primitives and every handler
// that uses them. If the Result implementation changes in a way that breaks any of
// these behaviours, these tests will fail immediately.
//
// Covered behaviours:
//   Result.Ok()           → IsSuccess = true, Error = empty string
//   Result.Fail("msg")    → IsSuccess = false, Error = "msg"
//   Result<T>.Ok(value)   → IsSuccess = true, Value = value, Error = empty string
//   Result<T>.Fail("msg") → IsSuccess = false, Error = "msg"
//   Result<T>.Value       → throws InvalidOperationException when result is failed

using StackFlow.Application.Common;

namespace StackFlow.UnitTests.Common;

public class ResultTests
{
    // ── Result (non-generic) ──────────────────────────────────────────────────

    [Fact]
    public void Result_Ok_SetsIsSuccessTrue()
    {
        // Act
        var result = Result.Ok();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Result_Fail_SetsIsSuccessFalse_AndError()
    {
        // Act
        var result = Result.Fail("Something went wrong");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Something went wrong", result.Error);
    }

    // ── Result<T> (generic) ───────────────────────────────────────────────────

    [Fact]
    public void ResultT_Ok_SetsValueAndIsSuccess()
    {
        // Arrange
        const string value = "test-value";

        // Act
        var result = Result<string>.Ok(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void ResultT_Fail_SetsIsSuccessFalse_AndError()
    {
        // Act
        var result = Result<string>.Fail("Workflow not found");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Workflow not found", result.Error);
    }

    [Fact]
    public void ResultT_AccessValue_WhenFailed_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<string>.Fail("Workflow not found");

        // Act & Assert — accessing Value on a failed result must always throw.
        // This prevents callers from silently reading a null/default value
        // when they forgot to check IsSuccess first.
        var exception = Assert.Throws<InvalidOperationException>(() => _ = result.Value);
        Assert.Contains("Cannot access Value on a failed Result", exception.Message);
    }
}
