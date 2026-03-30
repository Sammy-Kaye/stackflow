// IPipelineBehavior<TRequest, TResponse> — middleware that wraps every handler invocation.
//
// Behaviors are executed in the order they are registered in DI. StackFlow registers:
//   1. ValidationBehavior — validates the request, short-circuits if validation fails
//   2. LoggingBehavior    — records request type name and elapsed milliseconds
//
// This ordering ensures that invalid requests never reach a handler, and that every
// request passing validation is timed and logged.
//
// To add new cross-cutting concerns (e.g. transaction management, caching), implement
// this interface and register it in DependencyInjection.cs in the desired position.

namespace StackFlow.Application.Common.Mediator;

/// <summary>
/// Delegate that invokes the next step in the pipeline (either the next behavior or the handler itself).
/// </summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Middleware contract for the mediator pipeline.
/// Implement to add cross-cutting concerns that wrap every handler invocation.
/// </summary>
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Wraps the handler invocation. Call <paramref name="next"/> to proceed to the next step.
    /// Short-circuit by returning without calling <paramref name="next"/> to block execution.
    /// </summary>
    Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next);
}
