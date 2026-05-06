// WorkflowSummaryDto — the lightweight response shape used in the workflow list endpoint.
//
// Returns aggregate fields (TaskCount, IsGlobal) rather than the full task list.
// Used by GET /api/workflows. The full detail (including tasks) is WorkflowDto,
// returned by GET /api/workflows/{id} and the create/update endpoints.

namespace StackFlow.Application.Features.Workflows.DTOs;

/// <summary>
/// Lightweight summary of a workflow template. Used in the list endpoint response.
/// IsGlobal is true for starter templates (WorkspaceId == GlobalWorkspaceId).
/// </summary>
public record WorkflowSummaryDto(
    string Id,
    string Name,
    string? Description,
    string? Category,
    bool IsActive,
    int TaskCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsGlobal
);
