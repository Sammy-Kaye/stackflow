// IQuery<TResponse> — marker interface for read operations.
//
// Implement this interface on any record that represents a data-fetching operation.
// Examples: GetWorkflowByIdQuery, ListWorkflowsQuery, GetMyTasksQuery.
//
// By convention:
//   - Queries are records (immutable, carry the query's filter/paging inputs)
//   - TResponse is always Result<T> carrying the DTO — never void, never a raw entity
//   - Queries must never mutate state
//   - One handler per query, resolved automatically by the mediator via DI

namespace StackFlow.Application.Common.Mediator;

/// <summary>
/// Marker interface for read operations (state queries).
/// Implement on query records. TResponse must be Result&lt;T&gt; carrying a DTO.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse> { }
