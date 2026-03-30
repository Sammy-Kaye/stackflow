// Application-layer DI registrations.
// Called once from Program.cs: builder.Services.AddApplication()
//
// What is registered here:
//   - Mediator (Scoped) — the CQRS dispatch hub
//   - All IRequestHandler<,> implementations in this assembly (Scoped, via assembly scanning)
//   - ValidationBehavior (Scoped, first — runs before LoggingBehavior)
//   - LoggingBehavior (Scoped, second — runs after validation, wraps the handler)
//
// Pipeline execution order when a request is dispatched:
//   ValidationBehavior → LoggingBehavior → Handler
//
// Handler registration uses open-generic assembly scanning:
//   Every class in StackFlow.Application that implements IRequestHandler<TRequest, TResponse>
//   is registered automatically. Adding a new handler requires zero changes here.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StackFlow.Application.Common.Behaviors;
using StackFlow.Application.Common.Mediator;

namespace StackFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ── Mediator ──────────────────────────────────────────────────────────
        // Scoped: one instance per HTTP request. The mediator holds a reference to
        // IServiceProvider so it can resolve handlers and behaviors on demand.
        services.AddScoped<Mediator>();

        // ── Handler assembly scanning ─────────────────────────────────────────
        // Discovers every IRequestHandler<TRequest, TResponse> implementation in this
        // assembly and registers it as Scoped. This means adding a new handler in any
        // Features/ subfolder requires no manual DI wiring — it is picked up automatically.
        var assembly = Assembly.GetExecutingAssembly();

        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Implementation = t, Interface = i }));

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }

        // ── Pipeline behaviors ────────────────────────────────────────────────
        // Registration order determines execution order.
        // ValidationBehavior runs first so invalid requests never reach a handler.
        // LoggingBehavior runs second so it times only requests that passed validation.
        //
        // Both are registered as open-generic so they apply to every TRequest/TResponse pair.
        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));

        return services;
    }
}
