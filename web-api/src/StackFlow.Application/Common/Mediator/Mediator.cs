// Mediator — the CQRS dispatch hub for StackFlow.
//
// Receives a request via Send<TResponse>(), resolves the registered handler from DI,
// wraps it with all registered IPipelineBehavior<TRequest, TResponse> instances in
// registration order, and executes the resulting chain.
//
// Pipeline execution for a given request:
//   Mediator.Send()
//     → ValidationBehavior.Handle()   (short-circuits if validation fails)
//       → LoggingBehavior.Handle()    (records type name + elapsed ms)
//         → IRequestHandler.Handle()  (the actual business logic)
//
// Behaviors are resolved as IEnumerable<IPipelineBehavior<TRequest, TResponse>>.
// The order of execution matches the registration order in DependencyInjection.cs.
//
// This mediator is intentionally hand-rolled (no MediatR) so every dispatch path
// is readable and debuggable without stepping into a black-box library.

using Microsoft.Extensions.DependencyInjection;

namespace StackFlow.Application.Common.Mediator;

/// <summary>
/// Dispatches requests to their registered handlers, passing through the pipeline behaviors.
/// Registered as Scoped — one instance per HTTP request.
/// </summary>
public sealed class Mediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Dispatches <paramref name="request"/> through the pipeline and returns the handler's result.
    /// </summary>
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        // Resolve the concrete handler for this specific TRequest/TResponse pair.
        // The handler type is IRequestHandler<TRequest, TResponse> where TRequest
        // is the runtime type of the request object.
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetRequiredService(handlerType);

        // Resolve all behaviors registered for this TRequest/TResponse pair.
        // Behaviors are registered as IEnumerable<IPipelineBehavior<TRequest, TResponse>>.
        var behaviorType = typeof(IEnumerable<>).MakeGenericType(
            typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse)));

        var behaviors = (IEnumerable<object>)_serviceProvider.GetRequiredService(behaviorType);

        // Build the pipeline chain from the inside out:
        //   Start with the handler invocation as the innermost delegate.
        //   Wrap each behavior around it, last-registered behavior is outermost.
        //   After reversing, first-registered behavior is outermost — i.e. runs first.
        //
        // The cast through dynamic is required because we are working with open-generic
        // types resolved at runtime. The concrete types are correct — this is verified
        // by the DI registrations and the assembly scanning in DependencyInjection.cs.
        RequestHandlerDelegate<TResponse> pipeline = () =>
        {
            dynamic concreteHandler = handler;
            dynamic concreteRequest = request;
            return (Task<TResponse>)concreteHandler.Handle(concreteRequest, ct);
        };

        foreach (var behavior in behaviors.Reverse())
        {
            var currentNext = pipeline;
            dynamic concreteBehavior = behavior;
            dynamic concreteRequest = request;
            pipeline = () => (Task<TResponse>)concreteBehavior.Handle(concreteRequest, ct, currentNext);
        }

        return pipeline();
    }
}
