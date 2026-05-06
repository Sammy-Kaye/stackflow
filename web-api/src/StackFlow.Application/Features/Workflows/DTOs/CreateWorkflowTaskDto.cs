// CreateWorkflowTaskDto — the input shape for a single task in the tasks array of
// CreateWorkflowCommand and UpdateWorkflowCommand request bodies.
//
// No Id field — the server generates a new Guid for each task.
// OrderIndex is provided by the client — no server-side reordering in Phase 1.

using StackFlow.Domain.Enums;

namespace StackFlow.Application.Features.Workflows.DTOs;

/// <summary>
/// Input shape for one WorkflowTask step inside a create or update workflow request.
/// </summary>
public record CreateWorkflowTaskDto(
    string Title,
    string? Description,
    AssigneeType AssigneeType,
    string? DefaultAssignedToEmail,
    int OrderIndex,
    int? DueAtOffsetDays,
    NodeType NodeType,
    string? ConditionConfig,
    Guid? ParentTaskId
);
