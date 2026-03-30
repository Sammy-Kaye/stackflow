// ValidationBehavior — first step in the StackFlow mediator pipeline.
//
// Resolves all IValidator<TRequest> instances registered for the incoming request type,
// runs them all in parallel, and short-circuits the pipeline if any validation failures
// are found. The handler's Handle() method is never called when validation fails.
//
// The TResponse : Result constraint is required so we can construct a Result.Fail(...)
// return value. All StackFlow handlers return Result or Result<T>, so this constraint
// is always satisfied.
//
// Validation error messages from all validators are joined into a single string
// separated by "; " so the API caller receives all problems in one response body,
// not just the first one.

using FluentValidation;
using StackFlow.Application.Common.Mediator;

namespace StackFlow.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that runs FluentValidation validators before the handler.
/// Short-circuits with Result.Fail if any validator reports failures.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        // If no validators are registered for this request type, proceed immediately.
        if (!_validators.Any())
            return await next();

        // Run all validators and collect every failure from every validator.
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var errors = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => f.ErrorMessage)
            .ToList();

        if (errors.Count > 0)
        {
            // Construct a failed Result of the correct type (Result or Result<T>).
            //
            // TResponse is constrained to Result, so at runtime it is either:
            //   a) Result         — return Result.Fail(errorMessage) directly
            //   b) Result<T>      — must call Result.Fail<T>(errorMessage) via reflection
            //                       because (TResponse)(object)Result.Fail(...) would throw
            //                       InvalidCastException: you cannot downcast a base class
            //                       instance to a derived class at runtime.
            //
            // Reflection approach: inspect whether TResponse is a closed generic type
            // whose generic type definition is Result<>. If so, extract T and invoke
            // the static Result.Fail<T>(string) factory through reflection so the
            // returned instance is already of type Result<T>, making the cast safe.
            var errorMessage = string.Join("; ", errors);

            var responseType = typeof(TResponse);

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                // TResponse is Result<T> — get T and call Result.Fail<T>(errorMessage)
                var innerType = responseType.GetGenericArguments()[0];
                var failMethod = typeof(Result)
                    .GetMethod(nameof(Result.Fail), 1, [typeof(string)])!
                    .MakeGenericMethod(innerType);
                return (TResponse)failMethod.Invoke(null, [errorMessage])!;
            }

            // TResponse is the non-generic Result — direct construction and cast is safe
            return (TResponse)(object)Result.Fail(errorMessage);
        }

        return await next();
    }
}
