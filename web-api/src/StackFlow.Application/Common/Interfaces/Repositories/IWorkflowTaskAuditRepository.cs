using StackFlow.Domain.Models;

namespace StackFlow.Application.Common.Interfaces.Repositories;

// Data access contract for WorkflowTaskAudit entries.
// Every mutation to WorkflowTaskState must produce an audit entry written
// through this interface in the same SaveChangesAsync call as the mutation.
public interface IWorkflowTaskAuditRepository
{
    Task AddAsync(WorkflowTaskAudit audit, CancellationToken ct);
    Task<IReadOnlyList<WorkflowTaskAudit>> GetByWorkflowTaskStateIdAsync(Guid workflowTaskStateId, CancellationToken ct);
}
