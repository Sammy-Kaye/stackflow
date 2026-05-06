// CreateWorkflowCommandHandler — creates a new Workflow template and its task list.
//
// Business rules:
//   - WorkspaceId is sourced from ICurrentUserService — never from the command.
//   - IsActive is set to true on creation.
//   - CreatedAt and UpdatedAt are set to DateTime.UtcNow by the handler.
//   - Each task receives a new server-generated Guid. OrderIndex is taken from the request.
//   - The workflow and all tasks are persisted in one SaveChangesAsync call (atomic).
//   - The full WorkflowDto (including tasks) is returned on success (HTTP 201 from controller).
//
// No audit entry required — template operations are not audited in Phase 1.

using StackFlow.Application.Common;
using StackFlow.Application.Common.Interfaces;
using StackFlow.Application.Common.Interfaces.Repositories;
using StackFlow.Application.Common.Mediator;
using StackFlow.Application.Features.Workflows.DTOs;
using StackFlow.Domain.Models;

namespace StackFlow.Application.Features.Workflows.Commands.CreateWorkflow;

public sealed class CreateWorkflowCommandHandler
    : IRequestHandler<CreateWorkflowCommand, Result<WorkflowDto>>
{
    private readonly IWorkflowRepository _repo;
    private readonly IWorkflowTaskRepository _taskRepo;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateWorkflowCommandHandler(
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
        CreateWorkflowCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var workflowId = Guid.NewGuid();

        var workflow = new Workflow
        {
            Id = workflowId,
            WorkspaceId = _currentUser.WorkspaceId,
            Name = command.Name,
            Description = command.Description ?? string.Empty,
            Category = command.Category,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Map each task DTO to a WorkflowTask entity with a new server-generated Guid.
        // OrderIndex is taken from the request — no server-side reordering in Phase 1.
        var tasks = command.Tasks
            .Select(dto => new WorkflowTask
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
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

        await _repo.AddAsync(workflow, ct);

        if (tasks.Count > 0)
            await _taskRepo.AddRangeAsync(tasks, ct);

        await _uow.SaveChangesAsync(ct);

        var taskDtos = tasks.Select(t => t.ToTaskDto()).ToList();
        return Result.Ok(workflow.ToDto(taskDtos));
    }
}
