using StackFlow.Domain.Models;

namespace StackFlow.Application.Common.Interfaces.Repositories;

// Data access contract for WorkflowAudit entries.
// Every mutation to WorkflowState must produce an audit entry written
// through this interface in the same SaveChangesAsync call as the mutation.
public interface IWorkflowAuditRepository
{
    Task AddAsync(WorkflowAudit audit, CancellationToken ct);
    Task<IReadOnlyList<WorkflowAudit>> GetByWorkflowStateIdAsync(Guid workflowStateId, CancellationToken ct);
}
