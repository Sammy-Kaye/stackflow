using Microsoft.EntityFrameworkCore;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Domain.Models;
using StackFlow.Infrastructure.Persistence;

namespace StackFlow.Infrastructure.Persistence.Repositories;

// Concrete EF Core implementation of IWorkflowTaskStateRepository.
// GetByAssignedUserAsync uses EF.Functions.ILike so the case-insensitive
// email comparison is pushed entirely to PostgreSQL rather than evaluated
// in-process via ToLower().
public class WorkflowTaskStateRepository : IWorkflowTaskStateRepository
{
    private readonly AppDbContext _context;

    public WorkflowTaskStateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WorkflowTaskState?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.WorkflowTaskStates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<WorkflowTaskState>> GetByWorkflowStateIdAsync(Guid workflowStateId, CancellationToken ct)
        => await _context.WorkflowTaskStates
            .AsNoTracking()
            .Where(t => t.WorkflowStateId == workflowStateId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WorkflowTaskState>> GetByAssignedUserAsync(string email, CancellationToken ct)
        => await _context.WorkflowTaskStates
            .AsNoTracking()
            .Where(t => t.AssignedToEmail != null &&
                        EF.Functions.ILike(t.AssignedToEmail, email))
            .ToListAsync(ct);

    public async Task AddAsync(WorkflowTaskState taskState, CancellationToken ct)
        => await _context.WorkflowTaskStates.AddAsync(taskState, ct);

    public Task UpdateAsync(WorkflowTaskState taskState, CancellationToken ct)
    {
        _ = ct;
        _context.WorkflowTaskStates.Update(taskState);
        return Task.CompletedTask;
    }

    public async Task AddRangeAsync(IEnumerable<WorkflowTaskState> taskStates, CancellationToken ct)
        => await _context.WorkflowTaskStates.AddRangeAsync(taskStates, ct);
}
