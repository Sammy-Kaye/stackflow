using Microsoft.EntityFrameworkCore;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Domain.Models;
using StackFlow.Infrastructure.Persistence;

namespace StackFlow.Infrastructure.Persistence.Repositories;

// Concrete EF Core implementation of IWorkflowTaskAuditRepository.
// Audit entries are append-only — there is no Update or Delete method.
// Results are ordered by Timestamp ascending so callers see the history
// in chronological order.
public class WorkflowTaskAuditRepository : IWorkflowTaskAuditRepository
{
    private readonly AppDbContext _context;

    public WorkflowTaskAuditRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(WorkflowTaskAudit audit, CancellationToken ct)
        => await _context.WorkflowTaskAudits.AddAsync(audit, ct);

    public async Task<IReadOnlyList<WorkflowTaskAudit>> GetByWorkflowTaskStateIdAsync(Guid workflowTaskStateId, CancellationToken ct)
        => await _context.WorkflowTaskAudits
            .AsNoTracking()
            .Where(a => a.WorkflowTaskStateId == workflowTaskStateId)
            .OrderBy(a => a.Timestamp)
            .ToListAsync(ct);
}
