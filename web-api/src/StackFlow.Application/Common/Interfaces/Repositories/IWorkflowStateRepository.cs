using StackFlow.Domain.Models;

namespace StackFlow.Application.Common.Interfaces.Repositories;

// Data access contract for WorkflowState instance entities.
// GetActiveByWorkspaceAsync returns only InProgress instances —
// the filter logic lives in the implementation, not in handlers.
public interface IWorkflowStateRepository
{
    Task<WorkflowState?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<WorkflowState>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken ct);
    Task<IReadOnlyList<WorkflowState>> GetActiveByWorkspaceAsync(Guid workspaceId, CancellationToken ct);
    Task AddAsync(WorkflowState state, CancellationToken ct);
    Task UpdateAsync(WorkflowState state, CancellationToken ct);
}
