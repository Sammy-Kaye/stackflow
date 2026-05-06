// GetWorkflowsQueryHandler — returns all workflows visible to the authenticated user.
//
// Business rules:
//   - Returns the workspace's own workflows AND global starter templates
//     (WorkspaceId == GlobalWorkspaceId) in a single response.
//   - Workspace-owned workflows appear first (ordered by CreatedAt descending within each group).
//   - Global templates appear after workspace workflows (ordered by CreatedAt descending).
//   - Each item includes TaskCount (from IWorkflowTaskRepository) and IsGlobal flag.
//   - Items use WorkflowSummaryDto — the lightweight shape without the full task list.
//   - WorkspaceId comes from ICurrentUserService — never from the query.

using StackFlow.Application.Common;
using StackFlow.Application.Common.Interfaces;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Application.Common.Mediator;
using StackFlow.Application.Features.Workflows.DTOs;
using StackFlow.Domain.Constants;

namespace StackFlow.Application.Features.Workflows.Queries.GetWorkflows;

public sealed class GetWorkflowsQueryHandler
    : IRequestHandler<GetWorkflowsQuery, Result<WorkflowListDto>>
{
    private readonly IWorkflowRepository _repo;
    private readonly IWorkflowTaskRepository _taskRepo;
    private readonly ICurrentUserService _currentUser;

    public GetWorkflowsQueryHandler(
        IWorkflowRepository repo,
        IWorkflowTaskRepository taskRepo,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _taskRepo = taskRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkflowListDto>> Handle(
        GetWorkflowsQuery query, CancellationToken ct)
    {
        // Sequential queries — EF Core DbContext is not thread-safe; Task.WhenAll on the
        // same scoped context causes a concurrent operation exception.
        var workspaceWorkflows = await _repo.GetByWorkspaceAsync(_currentUser.WorkspaceId, ct);
        var globalWorkflows = await _repo.GetByWorkspaceAsync(WellKnownIds.GlobalWorkspaceId, ct);

        // Build summary DTOs — for each workflow fetch its task count.
        // Workspace workflows first (isGlobal = false), global templates after (isGlobal = true).
        var summaries = new List<WorkflowSummaryDto>();

        foreach (var workflow in workspaceWorkflows)
        {
            var tasks = await _taskRepo.GetByWorkflowIdAsync(workflow.Id, ct);
            summaries.Add(workflow.ToSummaryDto(tasks.Count, isGlobal: false));
        }

        // Only add global workflows that are not already in the list.
        // Guards against the edge case where the user's workspace IS the global workspace
        // (e.g. dev auth stub misconfiguration), which would otherwise return duplicates.
        var seen = new HashSet<Guid>(summaries.Select(s => s.Id));

        foreach (var workflow in globalWorkflows)
        {
            if (!seen.Add(workflow.Id)) continue;
            var tasks = await _taskRepo.GetByWorkflowIdAsync(workflow.Id, ct);
            summaries.Add(workflow.ToSummaryDto(tasks.Count, isGlobal: true));
        }

        return Result.Ok(new WorkflowListDto(summaries, summaries.Count));
    }
}
