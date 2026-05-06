// UpdateWorkflowCommandHandler — updates a Workflow template's header fields and replaces
// its task list in one atomic operation.
//
// Business rules (in order):
//   1. Fetch the workflow by ID.
//   2. If not found or belongs to a different workspace → "Workflow not found" (→ 404).
//      Using the same error for both cases prevents enumeration of other workspaces' workflows.
//   3. Apply header fields: Name, Description, Category, IsActive.
//   4. Set UpdatedAt to DateTime.UtcNow — the handler always stamps this, never trusting the client.
//   5. Delete all existing WorkflowTask records for this workflow.
//   6. Insert the new task list. Each task receives a new server-generated Guid.
//   7. Persist all changes in one SaveChangesAsync call.
//   8. Return the updated WorkflowDto with the new task list.
//
// No audit entry required — template operations are not audited in Phase 1.

using StackFlow.Application.Common;
using StackFlow.Application.Common.Interfaces;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Application.Common.Mediator;
using StackFlow.Application.Features.Workflows.DTOs;
using StackFlow.Domain.Models;

namespace StackFlow.Application.Features.Workflows.Commands.UpdateWorkflow;

public sealed class UpdateWorkflowCommandHandler
    : IRequestHandler<UpdateWorkflowCommand, Result<WorkflowDto>>
{
    private readonly IWorkflowRepository _repo;
    private readonly IWorkflowTaskRepository _taskRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateWorkflowCommandHandler(
        IWorkflowRepository repo,
        IWorkflowTaskRepository taskRepo,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _repo = repo;
        _taskRepo = taskRepo;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<WorkflowDto>> Handle(
        UpdateWorkflowCommand command, CancellationToken ct)
    {
        var workflow = await _repo.GetByIdAsync(command.Id, ct);

        if (workflow is null || workflow.WorkspaceId != _currentUser.WorkspaceId)
            return Result<WorkflowDto>.Fail("Workflow not found");

        // Update header fields.
        workflow.Name = command.Name;
        workflow.Description = command.Description ?? string.Empty;
        workflow.Category = command.Category;
        workflow.IsActive = command.IsActive;
        workflow.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(workflow, ct);

        // Replace task list: delete all existing tasks, insert the new ones.
        var existingTasks = await _taskRepo.GetByWorkflowIdAsync(command.Id, ct);
        foreach (var existing in existingTasks)
            await _taskRepo.DeleteAsync(existing, ct);

        var newTasks = command.Tasks
            .Select(dto => new WorkflowTask
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                AssigneeType = dto.AssigneeType,
                DefaultAssignedToEmail = dto.DefaultAssignedToEmail,
                OrderIndex = dto.OrderIndex,
                DueAtOffsetDays = dto.DueAtOffsetDays ?? 0,
                NodeType = dto.NodeType,
                ConditionConfig = dto.ConditionConfig,
                ParentTaskId = dto.ParentTaskId
            })
            .ToList();

        if (newTasks.Count > 0)
            await _taskRepo.AddRangeAsync(newTasks, ct);

        // Commit header update and task replacement atomically.
        await _uow.SaveChangesAsync(ct);

        var taskDtos = newTasks.Select(t => t.ToTaskDto()).ToList();
        return Result.Ok(workflow.ToDto(taskDtos));
    }
}
