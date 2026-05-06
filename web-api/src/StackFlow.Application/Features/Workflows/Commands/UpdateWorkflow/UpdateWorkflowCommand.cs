// UpdateWorkflowCommand — updates a Workflow template's header fields and replaces its task list.
//
// The full current state is sent by the client (not a partial patch).
// The task list is fully replaced: all existing WorkflowTask records for this workflow
// are deleted, then the new list is inserted. No diffing — simple and correct for Phase 1.
// The handler verifies the workflow belongs to the current user's workspace before applying changes.

using StackFlow.Application.Common;
using StackFlow.Application.Common.Mediator;
using StackFlow.Application.Features.Workflows.DTOs;

namespace StackFlow.Application.Features.Workflows.Commands.UpdateWorkflow;

/// <summary>
/// Updates Name, Description, Category, IsActive, and replaces the full task list
/// on an existing workflow. All fields must be provided (full-replace, not patch).
/// </summary>
public record UpdateWorkflowCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Category,
    bool IsActive,
    IReadOnlyList<CreateWorkflowTaskDto> Tasks
) : ICommand<Result<WorkflowDto>>;
