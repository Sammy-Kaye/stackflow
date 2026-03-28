// Result and Result<T> — the StackFlow error handling primitives.
//
// Every handler returns one of these types. The API layer maps them to HTTP status codes
// via BaseApiController.HandleResult(). Business exceptions are never thrown — all failure
// states are represented as Result.Fail() values.
//
// Usage in a handler:
//   return Result<WorkflowDto>.Ok(dto);       // success with a value
//   return Result<WorkflowDto>.Fail("...");   // failure with an error message
//   return Result.Ok();                        // success with no value
//   return Result.Fail("...");                 // failure with no value
//
// Usage in a controller:
//   return HandleResult(await Mediator.Send(command));
//
// HTTP mapping (applied by BaseApiController.HandleResult):
//   IsSuccess = true  → 200 OK  (with Value as the response body, or empty body for Result)
//   IsSuccess = false → 400 Bad Request with { "error": "<Error>" }

namespace StackFlow.Application.Common;

// Non-generic Result — for commands that return no value on success.
public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Returns a successful result with no value.</summary>
    public static Result Ok() => new(true, string.Empty);

    /// <summary>Returns a failed result carrying the given error message.</summary>
    public static Result Fail(string error) => new(false, error);

    /// <summary>Returns a successful result carrying a value. Convenience factory on the non-generic type.</summary>
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);

    /// <summary>Returns a failed generic result. Convenience factory on the non-generic type.</summary>
    public static Result<T> Fail<T>(string error) => Result<T>.Fail(error);
}

// Generic Result<T> — for queries and commands that return a value on success.
public class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>
    /// The success value. Only valid when IsSuccess is true.
    /// Accessing Value on a failed result will throw InvalidOperationException.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed Result.");

    private Result(bool isSuccess, T? value, string error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>Returns a successful result carrying the given value.</summary>
    public static Result<T> Ok(T value) => new(true, value, string.Empty);

    /// <summary>Returns a failed result carrying the given error message.</summary>
    public new static Result<T> Fail(string error) => new(false, default, error);
}
