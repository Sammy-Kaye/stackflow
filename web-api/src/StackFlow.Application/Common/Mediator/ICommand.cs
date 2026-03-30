// ICommand<TResponse> — marker interface for write operations.
//
// Implement this interface on any record that represents a state-mutating operation.
// Examples: CreateWorkflowCommand, CompleteTaskCommand, CancelWorkflowStateCommand.
//
// By convention:
//   - Commands are records (immutable value objects carrying the operation's inputs)
//   - TResponse is always Result or Result<T> — never a raw domain type
//   - One handler per command, resolved automatically by the mediator via DI

namespace StackFlow.Application.Common.Mediator;

/// <summary>
/// Marker interface for write operations (state mutations).
/// Implement on command records. TResponse must be Result or Result&lt;T&gt;.
/// </summary>
public interface ICommand<TResponse> : IRequest<TResponse> { }
