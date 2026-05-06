// WorkflowDto — the full response shape for workflow detail endpoints.
// Matches the API contract in Feature Brief 08 exactly.
// All Guid fields are returned as strings. All DateTime fields are returned as ISO 8601 strings.
// JSON serialisation to camelCase is handled by the global JsonOptions in Program.cs.

namespace StackFlow.Application.Features.Workflows.DTOs;

/// <summary>
/// The full response representation of a Workflow template record, including its task list.
/// Returned by GET /api/workflows/{id}, POST /api/workflows, and PUT /api/workflows/{id}.
/// </summary>
public record WorkflowDto(
    string Id,
    string Name,
    string? Description,
    string? Category,
    string WorkspaceId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<WorkflowTaskDto> Tasks
);
