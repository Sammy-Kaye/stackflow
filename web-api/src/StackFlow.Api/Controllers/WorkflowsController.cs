// WorkflowsController — Feature 8: Workflow CRUD (Templates)
//
// Five endpoints for managing Workflow template records.
// This controller is intentionally thin — every action is one line.
// All business logic lives in the Application-layer handlers.
//
// Auth: all endpoints require a valid JWT bearer token ([Authorize] on the class).
// Unauthorised requests receive { "error": "Unauthorised" } from JwtBearerEvents.OnChallenge.
// The DELETE endpoint additionally requires the Admin role — non-Admin JWT receives 403.
//
// HTTP method → handler → HTTP response:
//   GET    /api/workflows          → GetWorkflowsQuery          → 200 WorkflowListDto
//   GET    /api/workflows/{id}     → GetWorkflowByIdQuery        → 200 WorkflowDto | 404
//   POST   /api/workflows          → CreateWorkflowCommand       → 201 WorkflowDto | 400
//   PUT    /api/workflows/{id}     → UpdateWorkflowCommand       → 200 WorkflowDto | 400 | 404
//   DELETE /api/workflows/{id}     → DeleteWorkflowCommand       → 204 | 400 | 403 | 404

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackFlow.Application.Common.Mediator;
using StackFlow.Application.Features.Workflows.Commands.CreateWorkflow;
using StackFlow.Application.Features.Workflows.Commands.DeleteWorkflow;
using StackFlow.Application.Features.Workflows.Commands.UpdateWorkflow;
using StackFlow.Application.Features.Workflows.DTOs;
using StackFlow.Application.Features.Workflows.Queries.GetWorkflowById;
using StackFlow.Application.Features.Workflows.Queries.GetWorkflows;

namespace StackFlow.Api.Controllers;

[Authorize]
[Route("api/workflows")]
public class WorkflowsController : BaseApiController
{
    public WorkflowsController(Mediator mediator) : base(mediator) { }

    // GET /api/workflows
    // Returns all workflows visible to the authenticated user's workspace,
    // including global starter templates. Workspace workflows listed first.
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => HandleResult(await Mediator.Send(new GetWorkflowsQuery(), ct));

    // GET /api/workflows/{id}
    // Returns a single workflow with its full task list.
    // Returns 404 if not found, outside the current workspace, or not a global template.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => HandleResult(await Mediator.Send(new GetWorkflowByIdQuery(id), ct));

    // POST /api/workflows
    // Creates a new workflow with its initial task list.
    // WorkspaceId is sourced from the JWT — not the request body.
    // Returns 201 Created with the new workflow in the response body.
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowCommand command, CancellationToken ct)
        => HandleCreatedResult(await Mediator.Send(command, ct));

    // PUT /api/workflows/{id}
    // Updates header fields (Name, Description, Category, IsActive) and replaces the task list.
    // Id comes from the route; the rest comes from the body.
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkflowBody body, CancellationToken ct)
        => HandleResult(await Mediator.Send(
            new UpdateWorkflowCommand(id, body.Name, body.Description, body.Category, body.IsActive, body.Tasks), ct));

    // DELETE /api/workflows/{id}
    // Hard-deletes a workflow. Requires Admin role — non-Admin returns 403.
    // Global starter templates return 400. Returns 204 No Content on success.
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => HandleNoContentResult(await Mediator.Send(new DeleteWorkflowCommand(id), ct));
}

// UpdateWorkflowBody — the request body shape for PUT /api/workflows/{id}.
// Id is excluded from the body intentionally: it comes from the route parameter.
// This prevents mismatches between the route ID and a body ID.
public record UpdateWorkflowBody(
    string Name,
    string? Description,
    string? Category,
    bool IsActive,
    IReadOnlyList<CreateWorkflowTaskDto> Tasks
);
