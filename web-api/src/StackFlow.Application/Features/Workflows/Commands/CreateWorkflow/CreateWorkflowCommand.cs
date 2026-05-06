// CreateWorkflowCommand — creates a new Workflow template record with its initial task list.
//
// WorkspaceId is intentionally absent from this record. The handler sources it
// from ICurrentUserService so clients cannot create workflows in arbitrary workspaces.
// New workflows are always created with IsActive = true.
//
// Tasks may be an empty list. A workflow can be created with no tasks — the builder
// (Feature 9) adds tasks later. The list is never null; use an empty array in the request.

using StackFlow.Application.Common;
using StackFlow.Application.Common.Mediator;
using StackFlow.Application.Features.Workflows.DTOs;

namespace StackFlow.Application.Features.Workflows.Commands.CreateWorkflow;

/// <summary>
/// Creates a new workflow template with an initial task list.
/// WorkspaceId is sourced from the authenticated user's claims — not this record.
/// </summary>
public record CreateWorkflowCommand(
    string Name,
    string? Description,
    string? Category,
    IReadOnlyList<CreateWorkflowTaskDto> Tasks
) : ICommand<Result<WorkflowDto>>;
