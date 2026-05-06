// GetWorkflowByIdQuery — fetches a single workflow by its ID.
//
// The handler validates that the workflow belongs to the current user's workspace
// before returning it. If the workflow exists but belongs to a different workspace,
// the handler returns 404 (not 403) — this is intentional per the API contract:
// the existence of another workspace's workflow must not be revealed.

using StackFlow.Application.Common;
using StackFlow.Application.Common.Mediator;
using StackFlow.Application.Features.Workflows.DTOs;

namespace StackFlow.Application.Features.Workflows.Queries.GetWorkflowById;

/// <summary>
/// Returns a single workflow by ID. Returns 404 if not found or not in the current workspace.
/// </summary>
public record GetWorkflowByIdQuery(Guid Id) : IQuery<Result<WorkflowDto>>;
