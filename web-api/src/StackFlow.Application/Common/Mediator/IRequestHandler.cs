// IRequestHandler<TRequest, TResponse> — the handler contract every handler must implement.
//
// There is exactly one handler per request type. The mediator resolves the handler from
// the DI container using IRequestHandler<TRequest, TResponse> as the service key.
//
// Handler responsibilities:
//   - Apply business rules
//   - Coordinate domain entities and repositories
//   - Call IUnitOfWork.SaveChangesAsync() explicitly (never in repositories)
//   - Return Result.Ok(...) on success or Result.Fail("...") on failure
//   - Write audit entries for any WorkflowState or WorkflowTaskState mutation
//
// Handlers are discovered and registered automatically at startup via assembly scanning.
// Never register a handler manually in DI.

namespace StackFlow.Application.Common.Mediator;

/// <summary>
/// Contract that every handler must implement.
/// TRequest must be an IRequest&lt;TResponse&gt;.
/// TResponse is always Result or Result&lt;T&gt;.
/// </summary>
public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Executes the use case for the given request.
    /// Never throws business exceptions — return Result.Fail() instead.
    /// </summary>
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}
