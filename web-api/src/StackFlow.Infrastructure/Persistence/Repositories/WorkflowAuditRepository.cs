using Microsoft.EntityFrameworkCore;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Domain.Models;
using StackFlow.Infrastructure.Persistence;

namespace StackFlow.Infrastructure.Persistence.Repositories;

// Concrete EF Core implementation of IWorkflowAuditRepository.
// Audit entries are append-only — there is no Update or Delete method.
// Results are ordered by Timestamp ascending so callers see the history
// in chronological order.
public class WorkflowAuditRepository : IWorkflowAuditRepository
{
    private readonly AppDbContext _context;

    public WorkflowAuditRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(WorkflowAudit audit, CancellationToken ct)
        => await _context.WorkflowAudits.AddAsync(audit, ct);

    public async Task<IReadOnlyList<WorkflowAudit>> GetByWorkflowStateIdAsync(Guid workflowStateId, CancellationToken ct)
        => await _context.WorkflowAudits
            .AsNoTracking()
            .Where(a => a.WorkflowStateId == workflowStateId)
            .OrderBy(a => a.Timestamp)
            .ToListAsync(ct);
}
