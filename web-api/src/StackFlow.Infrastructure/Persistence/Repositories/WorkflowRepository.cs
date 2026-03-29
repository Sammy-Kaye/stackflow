using Microsoft.EntityFrameworkCore;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Domain.Models;
using StackFlow.Infrastructure.Persistence;

namespace StackFlow.Infrastructure.Persistence.Repositories;

// Concrete EF Core implementation of IWorkflowRepository.
//
// Read methods use AsNoTracking() — entities are not tracked by EF's change
// tracker, which reduces memory overhead for queries that don't need to mutate.
//
// Write methods do NOT call SaveChangesAsync. Transaction control belongs to
// the handler via IUnitOfWork. This allows multiple repository operations in
// one handler to be committed atomically.
public class WorkflowRepository : IWorkflowRepository
{
    private readonly AppDbContext _context;

    public WorkflowRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Workflow?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.Workflows
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, ct);

    public async Task<IReadOnlyList<Workflow>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken ct)
        => await _context.Workflows
            .AsNoTracking()
            .Where(w => w.WorkspaceId == workspaceId)
            .OrderBy(w => w.Name)
            .ToListAsync(ct);

    public async Task AddAsync(Workflow workflow, CancellationToken ct)
        => await _context.Workflows.AddAsync(workflow, ct);

    public Task UpdateAsync(Workflow workflow, CancellationToken ct)
    {
        _ = ct;
        _context.Workflows.Update(workflow);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Workflow workflow, CancellationToken ct)
    {
        _ = ct;
        _context.Workflows.Remove(workflow);
        return Task.CompletedTask;
    }
}
