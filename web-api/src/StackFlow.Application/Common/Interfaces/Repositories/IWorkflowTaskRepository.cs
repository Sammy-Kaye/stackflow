using StackFlow.Domain.Models;

namespace StackFlow.Application.Common.Interfaces.Repositories;

// Data access contract for WorkflowTask template step entities.
// AddRangeAsync is provided for bulk inserts when creating a workflow
// with multiple tasks in a single operation.
public interface IWorkflowTaskRepository
{
    Task<IReadOnlyList<WorkflowTask>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken ct);
    Task AddAsync(WorkflowTask task, CancellationToken ct);
    Task UpdateAsync(WorkflowTask task, CancellationToken ct);
    Task DeleteAsync(WorkflowTask task, CancellationToken ct);
    Task AddRangeAsync(IEnumerable<WorkflowTask> tasks, CancellationToken ct);
}
