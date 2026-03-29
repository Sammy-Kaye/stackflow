using Microsoft.EntityFrameworkCore;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Domain.Models;
using StackFlow.Infrastructure.Persistence;

namespace StackFlow.Infrastructure.Persistence.Repositories;

// Concrete EF Core implementation of IWorkflowTaskRepository.
// Results are ordered by OrderIndex so callers always receive steps
// in their intended execution sequence without extra sorting.
public class WorkflowTaskRepository : IWorkflowTaskRepository
{
    private readonly AppDbContext _context;

    public WorkflowTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WorkflowTask>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken ct)
        => await _context.WorkflowTasks
            .AsNoTracking()
            .Where(t => t.WorkflowId == workflowId)
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(ct);

    public async Task AddAsync(WorkflowTask task, CancellationToken ct)
        => await _context.WorkflowTasks.AddAsync(task, ct);

    public Task UpdateAsync(WorkflowTask task, CancellationToken ct)
    {
        _ = ct;
        _context.WorkflowTasks.Update(task);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(WorkflowTask task, CancellationToken ct)
    {
        _ = ct;
        _context.WorkflowTasks.Remove(task);
        return Task.CompletedTask;
    }

    public async Task AddRangeAsync(IEnumerable<WorkflowTask> tasks, CancellationToken ct)
        => await _context.WorkflowTasks.AddRangeAsync(tasks, ct);
}
