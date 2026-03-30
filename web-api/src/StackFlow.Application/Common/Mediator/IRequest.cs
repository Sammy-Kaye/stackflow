// IRequest<TResponse> — the root marker interface for all CQRS requests.
//
// Every command and query in StackFlow implements this interface (indirectly, via
// ICommand<TResponse> or IQuery<TResponse>). The TResponse type parameter tells
// the mediator what type the handler for this request produces.
//
// Handlers are discovered at startup by scanning the Application assembly for
// IRequestHandler<TRequest, TResponse> implementations — no manual registration needed.

namespace StackFlow.Application.Common.Mediator;

/// <summary>
/// Marker interface for all mediator requests (commands and queries).
/// TResponse is the type the corresponding handler returns.
/// </summary>
public interface IRequest<TResponse> { }
