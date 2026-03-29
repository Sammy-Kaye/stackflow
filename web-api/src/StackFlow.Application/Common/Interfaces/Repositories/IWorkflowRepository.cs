using StackFlow.Domain.Models;

namespace StackFlow.Application.Common.Interfaces.Repositories;

// Data access contract for Workflow template entities.
// Handlers call this interface — never EF Core directly.
// SaveChangesAsync is NOT on this interface: transaction control belongs
// to the handler via IUnitOfWork, not to the repository.
public interface IWorkflowRepository
{
    Task<Workflow?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Workflow>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken ct);
    Task AddAsync(Workflow workflow, CancellationToken ct);
    Task UpdateAsync(Workflow workflow, CancellationToken ct);
    Task DeleteAsync(Workflow workflow, CancellationToken ct);
}
