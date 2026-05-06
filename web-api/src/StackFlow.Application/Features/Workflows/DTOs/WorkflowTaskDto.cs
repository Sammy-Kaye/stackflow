// WorkflowTaskDto — the response shape for one WorkflowTask template step.
// Returned as part of WorkflowDto in all endpoints that return the full workflow detail.
// All Guid fields are returned as strings. JSON serialisation to camelCase is handled
// by the global JsonOptions in Program.cs.

namespace StackFlow.Application.Features.Workflows.DTOs;

/// <summary>
/// The response representation of a single WorkflowTask template step.
/// Included in WorkflowDto.Tasks, ordered by OrderIndex ascending.
/// </summary>
public record WorkflowTaskDto(
    string Id,
    string WorkflowId,
    string Title,
    string? Description,
    string AssigneeType,
    string? DefaultAssignedToEmail,
    int OrderIndex,
    int DueAtOffsetDays,
    string NodeType,
    string? ConditionConfig,
    string? ParentTaskId
);
