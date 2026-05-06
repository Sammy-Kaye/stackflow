// DeleteWorkflowCommandHandler — hard-deletes a Workflow template.
//
// Business rules (in order):
//   1. Fetch the workflow by ID.
//   2. If not found or belongs to a different workspace → "Workflow not found" (→ 404).
//      Using the same "not found" message for both cases prevents workspace enumeration.
//   3. If the workflow's WorkspaceId == WellKnownIds.GlobalWorkspaceId
//      → "Global starter templates cannot be deleted" (→ 400).
//      This check happens after the workspace-scope check: if someone tries to delete
//      a global template, they first fail the workspace check (returns 404), which means
//      this 400 is only reachable by the workspace that actually owns the global workspace.
//      In Phase 1 no user owns GlobalWorkspaceId, so this guard is belt-and-suspenders.
//   4. Hard delete via repository. No soft-delete in Phase 1.
//
// No audit entry required — template operations are not audited in Phase 1.

using StackFlow.Application.Common;
using StackFlow.Application.Common.Interfaces;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Application.Common.Mediator;
using StackFlow.Domain.Constants;

namespace StackFlow.Application.Features.Workflows.Commands.DeleteWorkflow;

public sealed class DeleteWorkflowCommandHandler
    : IRequestHandler<DeleteWorkflowCommand, Result>
{
    private readonly IWorkflowRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteWorkflowCommandHandler(
        IWorkflowRepository repo,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(
        DeleteWorkflowCommand command, CancellationToken ct)
    {
        var workflow = await _repo.GetByIdAsync(command.Id, ct);

        if (workflow is null || workflow.WorkspaceId != _currentUser.WorkspaceId)
            return Result.Fail("Workflow not found");

        if (workflow.WorkspaceId == WellKnownIds.GlobalWorkspaceId)
            return Result.Fail("Global starter templates cannot be deleted");

        await _repo.DeleteAsync(workflow, ct);
        await _uow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
