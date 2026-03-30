// LoggingBehavior — second step in the StackFlow mediator pipeline.
//
// Wraps each handler invocation with structured logging:
//   - Before:  records a Stopwatch start
//   - After success:  logs at Information with request type name and elapsed milliseconds
//   - After failure:  logs at Warning with request type name, elapsed ms, and error message
//
// This gives a clear, searchable log of every command/query that flows through the system,
// with timing information for performance monitoring and the error message for debugging.
//
// "Success" and "failure" are determined by the Result.IsSuccess property on the response.
// If TResponse is not a Result (should never happen in StackFlow), the behavior logs
// at Information without checking success status.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StackFlow.Application.Common.Mediator;

namespace StackFlow.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that logs every request with elapsed time and success/failure status.
/// Logs at Information on success, Warning on failure.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        TResponse response = await next();

        stopwatch.Stop();

        // Determine success by checking whether the response is a Result instance.
        // All StackFlow responses are Result or Result<T>, so this path is always taken.
        if (response is Result result)
        {
            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Request {RequestName} completed successfully in {ElapsedMs}ms",
                    requestName, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "Request {RequestName} failed in {ElapsedMs}ms — {Error}",
                    requestName, stopwatch.ElapsedMilliseconds, result.Error);
            }
        }
        else
        {
            _logger.LogInformation(
                "Request {RequestName} completed in {ElapsedMs}ms",
                requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
