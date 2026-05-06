// DeleteWorkflowCommand — hard-deletes a Workflow template.
//
// Global starter templates (WorkspaceId == WellKnownIds.GlobalWorkspaceId) cannot be deleted.
// The handler enforces this check before proceeding with the delete.

using StackFlow.Application.Common;
using StackFlow.Application.Common.Mediator;

namespace StackFlow.Application.Features.Workflows.Commands.DeleteWorkflow;

/// <summary>
/// Hard-deletes a workflow by ID. Global starter templates are protected from deletion.
/// Returns Result (no value) — the controller maps success to 204 No Content.
/// </summary>
public record DeleteWorkflowCommand(Guid Id) : ICommand<Result>;
