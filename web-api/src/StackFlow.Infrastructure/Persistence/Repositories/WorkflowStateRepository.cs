using Microsoft.EntityFrameworkCore;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Domain.Enums;
using StackFlow.Domain.Models;
using StackFlow.Infrastructure.Persistence;

namespace StackFlow.Infrastructure.Persistence.Repositories;

// Concrete EF Core implementation of IWorkflowStateRepository.
// GetActiveByWorkspaceAsync filters to WorkflowStatus.InProgress only —
// the status filter is applied in the database via the WHERE clause, not in memory.
public class WorkflowStateRepository : IWorkflowStateRepository
{
    private readonly AppDbContext _context;

    public WorkflowStateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkflowState?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.WorkflowStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<WorkflowState>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken ct)
        => await _context.WorkflowStates
            .AsNoTracking()
            .Where(s => s.WorkspaceId == workspaceId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WorkflowState>> GetActiveByWorkspaceAsync(Guid workspaceId, CancellationToken ct)
        => await _context.WorkflowStates
            .AsNoTracking()
            .Where(s => s.WorkspaceId == workspaceId && s.Status == WorkflowStatus.InProgress)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(ct);

    public async Task AddAsync(WorkflowState state, CancellationToken ct)
        => await _context.WorkflowStates.AddAsync(state, ct);

    public Task UpdateAsync(WorkflowState state, CancellationToken ct)
    {
        _ = ct;
        _context.WorkflowStates.Update(state);
        return Task.CompletedTask;
    }
}
