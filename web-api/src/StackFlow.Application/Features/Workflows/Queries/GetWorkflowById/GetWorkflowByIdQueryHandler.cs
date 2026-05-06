// GetWorkflowByIdQueryHandler — returns a single workflow with its full task list.
//
// Access rules:
//   - Allowed if workflow.WorkspaceId == caller's WorkspaceId (workspace-owned workflow).
//   - Allowed if workflow.WorkspaceId == GlobalWorkspaceId (global starter template — readable
//     by all authenticated users).
//   - Denied (→ 404) in all other cases. Returning 404 instead of 403 prevents enumeration
//     of other workspaces' workflow IDs.
//
// Tasks are loaded via IWorkflowTaskRepository and ordered by OrderIndex ascending.

using StackFlow.Application.Common;
using StackFlow.Application.Common.Interfaces;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Application.Common.Mediator;
using StackFlow.Application.Features.Workflows.DTOs;
using StackFlow.Domain.Constants;

namespace StackFlow.Application.Features.Workflows.Queries.GetWorkflowById;

public sealed class GetWorkflowByIdQueryHandler
    : IRequestHandler<GetWorkflowByIdQuery, Result<WorkflowDto>>
{
    private readonly IWorkflowRepository _repo;
    private readonly IWorkflowTaskRepository _taskRepo;
    private readonly ICurrentUserService _currentUser;

    public GetWorkflowByIdQueryHandler(
        IWorkflowRepository repo,
        IWorkflowTaskRepository taskRepo,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _taskRepo = taskRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkflowDto>> Handle(
        GetWorkflowByIdQuery query, CancellationToken ct)
    {
        var workflow = await _repo.GetByIdAsync(query.Id, ct);

        // Allow access if the workflow belongs to the caller's workspace OR is a global template.
        var isAccessible = workflow is not null
            && (workflow.WorkspaceId == _currentUser.WorkspaceId
                || workflow.WorkspaceId == WellKnownIds.GlobalWorkspaceId);

        if (!isAccessible)
            return Result<WorkflowDto>.Fail("Workflow not found");

        var tasks = await _taskRepo.GetByWorkflowIdAsync(workflow!.Id, ct);
        var taskDtos = tasks.Select(t => t.ToTaskDto()).ToList();

        return Result.Ok(workflow.ToDto(taskDtos));
    }
}
