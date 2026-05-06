// GetWorkflowsQuery — fetches all workflows belonging to the authenticated user's workspace.
//
// WorkspaceId is not a parameter here — the handler reads it from ICurrentUserService.
// This prevents any client from requesting workflows from a workspace they don't own.
// Results are ordered by CreatedAt descending (newest first) by the repository.

using StackFlow.Application.Common;
using StackFlow.Application.Common.Mediator;
using StackFlow.Application.Features.Workflows.DTOs;

namespace StackFlow.Application.Features.Workflows.Queries.GetWorkflows;

/// <summary>
/// Returns all workflows for the current user's workspace, ordered by CreatedAt descending.
/// </summary>
public record GetWorkflowsQuery() : IQuery<Result<WorkflowListDto>>;
