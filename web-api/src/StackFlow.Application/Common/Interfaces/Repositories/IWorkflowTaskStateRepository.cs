using StackFlow.Domain.Models;

namespace StackFlow.Application.Common.Interfaces.Repositories;

// Data access contract for WorkflowTaskState instance entities.
// GetByAssignedUserAsync powers the My Tasks dashboard — returns all
// task states assigned to a given email across all workflow instances.
// AddRangeAsync is provided for bulk inserts when spawning a workflow instance.
public interface IWorkflowTaskStateRepository
{
    Task<WorkflowTaskState?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<WorkflowTaskState>> GetByWorkflowStateIdAsync(Guid workflowStateId, CancellationToken ct);
    Task<IReadOnlyList<WorkflowTaskState>> GetByAssignedUserAsync(string email, CancellationToken ct);
    Task AddAsync(WorkflowTaskState taskState, CancellationToken ct);
    Task UpdateAsync(WorkflowTaskState taskState, CancellationToken ct);
    Task AddRangeAsync(IEnumerable<WorkflowTaskState> taskStates, CancellationToken ct);
}
