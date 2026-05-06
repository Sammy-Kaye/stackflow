// WorkflowListDto — the response shape for GET /api/workflows.
// Wraps the list with a totalCount so the frontend can display "N workflows"
// and future pagination can be added without changing the contract.
//
// Items uses WorkflowSummaryDto — the lightweight shape that includes TaskCount
// and IsGlobal but not the full task list. The full task list is in WorkflowDto,
// returned by the detail endpoint (GET /api/workflows/{id}).

namespace StackFlow.Application.Features.Workflows.DTOs;

/// <summary>
/// Response shape for the workflow list endpoint.
/// </summary>
public record WorkflowListDto(
    IReadOnlyList<WorkflowSummaryDto> Items,
    int TotalCount
);
