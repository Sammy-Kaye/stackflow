# Feature Brief: Workflow CRUD (Templates)
Phase: 1
Feature: #8
Status: Ready for implementation
Brief date: 2026-03-31

---

## What this feature does (plain English)

Workflow CRUD gives the admin the ability to create, view, update, and delete workflow
templates — the reusable blueprints that describe what steps a workflow runs through. Each
workflow template is made up of an ordered list of task nodes (WorkflowTask records). This
feature covers template management only: the user can build a workflow definition, give it
a name and description, add or remove task nodes, and delete workflows they no longer need.
No live execution happens here — spawning a running instance from a template is Feature 11.

---

## Scope — what IS in this brief

**Backend:**

- `GET /api/workflows` — returns all workflows visible to the current workspace: the
  workspace's own workflows plus global starter templates. Each item is a
  `WorkflowSummaryDto` including task count and `isGlobal` flag.
- `GET /api/workflows/{id}` — returns one workflow with its full task list as
  `WorkflowDto`. Global templates are accessible to all workspaces (read-only).
- `POST /api/workflows` — creates a workflow and its initial task list in one request.
  `WorkspaceId` comes from the JWT, not the request body. Returns the full `WorkflowDto`.
- `PUT /api/workflows/{id}` — updates a workflow's header fields (Name, Description,
  Category, IsActive) and replaces its task list (delete all existing tasks, insert new
  ones). Returns the updated `WorkflowDto`.
- `DELETE /api/workflows/{id}` — hard-deletes a workflow and all its WorkflowTask records.
  Returns 204 No Content on success. Global starter templates cannot be deleted (400).
- `WorkflowSummaryDto` — Id, Name, Description, Category, IsActive, TaskCount, CreatedAt,
  UpdatedAt, IsGlobal
- `WorkflowDto` — Id, Name, Description, Category, WorkspaceId, IsActive, CreatedAt,
  UpdatedAt, Tasks (list of WorkflowTaskDto)
- `WorkflowTaskDto` — all WorkflowTask fields (Id, WorkflowId, Title, Description,
  AssigneeType, DefaultAssignedToEmail, OrderIndex, DueAtOffsetDays, NodeType,
  ConditionConfig, ParentTaskId)
- `CreateWorkflowTaskDto` — input shape for tasks in create/update requests (Title,
  Description, AssigneeType, DefaultAssignedToEmail, OrderIndex, DueAtOffsetDays, NodeType,
  ConditionConfig, ParentTaskId)
- FluentValidation on both create and update: Name required, max 200 chars; Description max
  2000 chars; Category max 100 chars; each task Title required, max 200 chars; NodeType must
  be a valid enum value; OrderIndex must be >= 0
- `IWorkflowTaskRepository` used in create, update, and delete handlers
- `WorkflowTaskConfiguration` EF Fluent API configuration file (if not already present)
- `CurrentUserService` already wired — handlers read WorkspaceId from it (do not add
  WorkspaceId to the request body)

**Frontend:**

- `WorkflowsListPage.tsx` at route `/workflows`
  - Fetches all workflows via `useWorkflows()` hook
  - Card grid: each card shows name, description (2-line truncation), task count, last
    edited date, IsActive badge (Active / Draft), and `IsGlobal` badge for starter templates
  - `Edit` button → `/workflows/{id}/edit` (routes to builder, Feature 9)
  - `Delete` button (Admin only, disabled if `isGlobal: true`)
  - Delete shows an `AlertDialog`: "Delete this workflow? This cannot be undone." with
    Cancel and Delete confirm buttons
  - Top bar: page title "Workflows" + `New workflow` button → `/workflows/new`
  - Empty state (when no workspace-owned workflows exist): "No workflows yet. Build your
    first one." + `Create workflow` button → `/workflows/new`
  - Skeleton loader while fetching
- `workflowService.ts` — `getAll()`, `getById(id)`, `create(dto)`, `update(id, dto)`,
  `remove(id)`
- `useWorkflows()` hook — React Query, query key `['workflows']`
- `useWorkflow(id)` hook — React Query, query key `['workflows', id]`
- `useCreateWorkflow()` mutation — invalidates `['workflows']` on success
- `useUpdateWorkflow()` mutation — invalidates `['workflows']` and `['workflows', id]`
- `useDeleteWorkflow()` mutation — invalidates `['workflows']` on success; success toast:
  "Workflow deleted"; error toast if backend returns 400 (e.g. global template)
- TypeScript entity interfaces: `Workflow`, `WorkflowTask`
- TypeScript DTO types: `WorkflowSummaryDto`, `WorkflowDto`, `WorkflowTaskDto`,
  `CreateWorkflowDto`, `UpdateWorkflowDto`, `CreateWorkflowTaskDto`
- TypeScript enums: `AssigneeType`, `NodeType` (must match backend enum values exactly)

---

## Scope — what is NOT in this brief

- WorkflowState and WorkflowTaskState — instance-level records. Those are Feature 11.
- The workflow builder canvas (React Flow drag-and-drop UI) — that is Feature 9. This
  brief covers list management only; the Edit button routes to Feature 9's builder page.
- `ActiveInstanceCount` in WorkflowSummaryDto — querying WorkflowState records is deferred
  to a later feature. For now this field is omitted from the summary DTO.
- Pagination — all workflows are returned in a single response. Added later if needed.
- Soft delete — hard delete only, consistent with the rest of Phase 1.
- Role-based access beyond what the existing JWT stub provides. The Delete endpoint is
  restricted to Admin role via `[Authorize(Roles = "Admin")]` on that action only.
- Template Library page (Feature 10) — that is a separate feature with its own page and
  browse/preview UI.
- File attachments on WorkflowTask records — Phase 2.
- WorkflowTask attachment fields (`AttachmentKey`, `AttachmentFilename`,
  `AttachmentContentType`, `AttachmentSizeBytes`) — present in the domain model (CLAUDE.md)
  but not surfaced in DTOs or API responses for this feature. Left as reserved fields.
- Any changes to the seeded global starter template WorkflowTask records. They are read by
  this feature's GET endpoints but not modified here.

---

## Domain entities involved

- `Workflow` (existing) — no new fields. All fields already in the database.
- `WorkflowTask` (existing) — no new fields. This feature introduces the first handlers
  that write WorkflowTask records via `IWorkflowTaskRepository`.
- No new entities introduced.

---

## API Contract

#### GET /api/workflows
Auth: JWT required

Request body: none

Response 200:
```json
{
  "items": [
    {
      "id": "string UUID",
      "name": "string",
      "description": "string | null",
      "category": "string | null",
      "isActive": "boolean",
      "taskCount": "number",
      "createdAt": "string ISO 8601",
      "updatedAt": "string ISO 8601",
      "isGlobal": "boolean"
    }
  ],
  "totalCount": "number"
}
```

Business rule: the items list includes both the current workspace's own workflows and
global starter templates (those whose WorkspaceId equals WellKnownIds.GlobalWorkspaceId).
The GlobalWorkspace row itself is never surfaced. Results ordered by CreatedAt descending,
with global templates listed after workspace-owned workflows.

Error responses:
  401 { "error": "Unauthorised" }

---

#### GET /api/workflows/{id}
Auth: JWT required

Request body: none

Response 200:
```json
{
  "id": "string UUID",
  "name": "string",
  "description": "string | null",
  "category": "string | null",
  "workspaceId": "string UUID",
  "isActive": "boolean",
  "createdAt": "string ISO 8601",
  "updatedAt": "string ISO 8601",
  "tasks": [
    {
      "id": "string UUID",
      "workflowId": "string UUID",
      "title": "string",
      "description": "string | null",
      "assigneeType": "string (Internal | External)",
      "defaultAssignedToEmail": "string | null",
      "orderIndex": "number",
      "dueAtOffsetDays": "number | null",
      "nodeType": "string (Task | Approval | Condition | Notification | ExternalStep | Deadline)",
      "conditionConfig": "string | null",
      "parentTaskId": "string UUID | null"
    }
  ]
}
```

Business rule: A workflow is accessible if its WorkspaceId matches the caller's workspace,
OR if it is a global template (WorkspaceId == WellKnownIds.GlobalWorkspaceId). The tasks
array is ordered by OrderIndex ascending.

Error responses:
  401 { "error": "Unauthorised" }
  404 { "error": "Workflow not found" }

---

#### POST /api/workflows
Auth: JWT required

Request body:
```json
{
  "name": "string (required, max 200)",
  "description": "string | null (optional, max 2000)",
  "category": "string | null (optional, max 100)",
  "tasks": [
    {
      "title": "string (required, max 200)",
      "description": "string | null (optional, max 2000)",
      "assigneeType": "string (required: Internal | External)",
      "defaultAssignedToEmail": "string | null (optional, max 256)",
      "orderIndex": "number (required, >= 0)",
      "dueAtOffsetDays": "number | null (optional)",
      "nodeType": "string (required: Task | Approval | Condition | Notification | ExternalStep | Deadline)",
      "conditionConfig": "string | null (optional)",
      "parentTaskId": "string UUID | null (optional)"
    }
  ]
}
```

Note: `tasks` may be an empty array. A workflow can be created with no tasks (the builder
adds tasks later in Feature 9). `workspaceId` is NOT accepted in the request body — it is
always taken from the JWT claim.

Response 201:
```json
{
  "id": "string UUID",
  "name": "string",
  "description": "string | null",
  "category": "string | null",
  "workspaceId": "string UUID",
  "isActive": "boolean",
  "createdAt": "string ISO 8601",
  "updatedAt": "string ISO 8601",
  "tasks": [
    {
      "id": "string UUID",
      "workflowId": "string UUID",
      "title": "string",
      "description": "string | null",
      "assigneeType": "string",
      "defaultAssignedToEmail": "string | null",
      "orderIndex": "number",
      "dueAtOffsetDays": "number | null",
      "nodeType": "string",
      "conditionConfig": "string | null",
      "parentTaskId": "string UUID | null"
    }
  ]
}
```

Business rules:
- IsActive is set to true on creation.
- CreatedAt and UpdatedAt are set to DateTime.UtcNow by the handler.
- Each task is created with a new Guid. OrderIndex is taken from the request — no
  server-side reordering.

Error responses:
  400 { "error": "string" }   ← validation failure
  401 { "error": "Unauthorised" }

---

#### PUT /api/workflows/{id}
Auth: JWT required

Request body:
```json
{
  "name": "string (required, max 200)",
  "description": "string | null (optional, max 2000)",
  "category": "string | null (optional, max 100)",
  "isActive": "boolean (required)",
  "tasks": [
    {
      "title": "string (required, max 200)",
      "description": "string | null (optional, max 2000)",
      "assigneeType": "string (required: Internal | External)",
      "defaultAssignedToEmail": "string | null (optional, max 256)",
      "orderIndex": "number (required, >= 0)",
      "dueAtOffsetDays": "number | null (optional)",
      "nodeType": "string (required: Task | Approval | Condition | Notification | ExternalStep | Deadline)",
      "conditionConfig": "string | null (optional)",
      "parentTaskId": "string UUID | null (optional)"
    }
  ]
}
```

Business rules:
- The workflow must exist and belong to the caller's workspace, otherwise 404.
- The task list is fully replaced: all existing WorkflowTask records for this workflow are
  deleted, then the new list is inserted. No diffing. Simple and correct for Phase 1.
- UpdatedAt is set to DateTime.UtcNow by the handler. The client does not supply it.
- The `{id}` in the route is the authoritative workflow ID. No Id field in the body.

Response 200: same shape as GET /api/workflows/{id} (full WorkflowDto with updated tasks)

Error responses:
  400 { "error": "string" }
  401 { "error": "Unauthorised" }
  404 { "error": "Workflow not found" }

---

#### DELETE /api/workflows/{id}
Auth: JWT required, Admin role required

Request body: none

Response 204: empty body

Business rules:
- The workflow must exist and belong to the caller's workspace, otherwise 404.
- If the workflow's WorkspaceId == WellKnownIds.GlobalWorkspaceId, return
  400 { "error": "Global starter templates cannot be deleted" }.
- Hard delete: removes the Workflow record. WorkflowTask records are removed by cascade
  delete (FK: WorkflowTask → Workflow, cascade on delete).

Error responses:
  400 { "error": "Global starter templates cannot be deleted" }
  401 { "error": "Unauthorised" }
  403 (no body) ← non-Admin JWT
  404 { "error": "Workflow not found" }

---

## Frontend routes and views

```
/workflows          → WorkflowsListPage (new)
/workflows/new      → WorkflowBuilderPage (stub from Feature 6 — not built in this brief)
/workflows/:id/edit → WorkflowBuilderPage (stub from Feature 6 — not built in this brief)
```

Only `WorkflowsListPage` is built in this brief. The `/new` and `/edit` routes already
exist as stubs from Feature 6 and are not modified here — the Edit and New buttons simply
navigate to them.

Module structure to create:

```
src/modules/workflows/
  entities/
    workflow.ts          ← Workflow, WorkflowTask interfaces
  dtos/
    workflow-dtos.ts     ← WorkflowSummaryDto, WorkflowDto, WorkflowTaskDto,
                            CreateWorkflowDto, UpdateWorkflowDto, CreateWorkflowTaskDto
  enums/
    assignee-type.ts     ← AssigneeType enum
    node-type.ts         ← NodeType enum
  infrastructure/
    workflow-service.ts  ← workflowService (getAll, getById, create, update, remove)
  hooks/
    use-workflows.ts     ← useWorkflows, useWorkflow, useCreateWorkflow,
                            useUpdateWorkflow, useDeleteWorkflow
  ui/
    pages/
      WorkflowsListPage.tsx
    components/
      WorkflowCard.tsx
      DeleteWorkflowDialog.tsx
```

---

## RabbitMQ events (if any)

None for this feature.

---

## SignalR events (if any)

None for this feature.

---

## Audit requirements

No audit entries required for this feature. Template CRUD (creating, editing, deleting
workflow blueprints) is not audited. Audit trail begins in Feature 12 (WorkflowTask
Execution + Audit Trail), which covers mutations to WorkflowState and WorkflowTaskState
instance records only.

---

## Acceptance criteria

**Backend:**

1. Given a valid JWT, when `GET /api/workflows` is called, then the response is 200 with
   both the workspace's own workflows and the three global starter templates in the items
   array, and `isGlobal` is `true` for global templates and `false` for workspace workflows.

2. Given a valid JWT and an existing workflow ID belonging to the caller's workspace, when
   `GET /api/workflows/{id}` is called, then the response is 200 and the `tasks` array
   contains all WorkflowTask records for that workflow ordered by `orderIndex` ascending.

3. Given a valid JWT and a global template ID, when `GET /api/workflows/{id}` is called,
   then the response is 200 (global templates are readable by all workspaces).

4. Given a valid JWT and a workflow ID from a different workspace, when
   `GET /api/workflows/{id}` is called, then the response is 404.

5. Given a valid JWT with name "Onboarding" and two tasks in the body, when
   `POST /api/workflows` is called, then the response is 201 with the new workflow ID,
   `isActive: true`, and a `tasks` array containing both tasks with server-generated IDs.

6. Given a valid JWT and an empty name, when `POST /api/workflows` is called, then the
   response is 400 with `{ "error": "Name is required." }`.

7. Given a valid JWT and an existing workspace workflow, when `PUT /api/workflows/{id}` is
   called with a new name and a different task list, then the response is 200, the workflow
   name is updated, the old task records no longer exist in the database, and the new task
   records are present with the IDs generated by the server.

8. Given a valid JWT and a workflow ID belonging to a different workspace, when
   `PUT /api/workflows/{id}` is called, then the response is 404.

9. Given a valid JWT with Admin role and an existing workspace workflow, when
   `DELETE /api/workflows/{id}` is called, then the response is 204 and the workflow and
   all its WorkflowTask records are removed from the database.

10. Given a valid JWT with Admin role and a global template ID, when
    `DELETE /api/workflows/{id}` is called, then the response is 400 with
    `{ "error": "Global starter templates cannot be deleted" }`.

11. Given a valid JWT with Member role, when `DELETE /api/workflows/{id}` is called, then
    the response is 403.

12. Given a valid JWT and a task with an empty title, when `POST /api/workflows` is called,
    then the response is 400 with a validation error referencing the task title.

**Frontend:**

13. Given the user navigates to `/workflows`, when the API returns three global templates
    and two workspace workflows, then five cards are displayed, with the global templates
    showing a "Global template" badge and their Delete buttons disabled.

14. Given the user is on `/workflows` and clicks Delete on a non-global workflow, when the
    confirmation dialog appears and the user clicks "Delete", then the workflow is removed
    from the list without a page reload and a success toast appears.

15. Given the user is on `/workflows` and no workspace-owned workflows exist (only global
    templates), when the page renders, the empty state message is shown alongside the global
    template cards.

16. Given the API is loading, when `/workflows` renders, then skeleton cards are displayed
    instead of empty white space.

17. Given the user clicks "New workflow", then they are navigated to `/workflows/new`.

18. Given the user clicks "Edit" on a workflow card, then they are navigated to
    `/workflows/{id}/edit`.

---

## Agent instructions

**Backend Agent — ordered build sequence:**

1. **Review existing partial implementation.** The following files already exist and must be
   read before writing anything:
   - `WorkflowsController.cs` — already wired with five endpoints, but current handlers do
     not include WorkflowTask support. The controller itself is correct and should not be
     modified unless the contract requires it (it does not).
   - `CreateWorkflowCommand`, `UpdateWorkflowCommand`, `DeleteWorkflowCommand` — exist but
     lack task list fields. These must be updated.
   - `GetWorkflowByIdQuery`, `GetWorkflowsQuery` — exist but their handlers do not include
     tasks in the response and do not include global templates in the list. Update handlers.
   - `WorkflowDto` — currently has no `tasks` field. Must be updated.
   - `WorkflowListDto` — uses `WorkflowDto` items. Replace items type with `WorkflowSummaryDto`.
   - `WorkflowMappingExtensions` — will need new mapping methods for WorkflowTask and the
     summary DTO.
   - `CreateWorkflowCommandValidator`, `UpdateWorkflowCommandValidator` — exist, add task
     validation rules.
   - `ICurrentUserService`, `CurrentUserService` — already present and registered. No changes.

2. **DTOs** (`Application/Features/Workflows/DTOs/`):
   - Add `WorkflowTaskDto` record.
   - Add `CreateWorkflowTaskDto` record.
   - Update `WorkflowDto` to include `Tasks (IReadOnlyList<WorkflowTaskDto>)`.
   - Add `WorkflowSummaryDto` record (Id, Name, Description, Category, IsActive, TaskCount,
     CreatedAt, UpdatedAt, IsGlobal).
   - Update `WorkflowListDto` items type from `WorkflowDto` to `WorkflowSummaryDto`.
   - Update `WorkflowMappingExtensions`: add `ToTaskDto()`, `ToSummaryDto(int taskCount, bool isGlobal)`,
     update `ToDto()` to accept and include the tasks list.

3. **Commands** (`Application/Features/Workflows/Commands/`):
   - Update `CreateWorkflowCommand` to add `Tasks (IReadOnlyList<CreateWorkflowTaskDto>)`.
   - Update `CreateWorkflowCommandHandler` to create WorkflowTask records via
     `IWorkflowTaskRepository.AddRangeAsync` and include them in the returned `WorkflowDto`.
   - Update `CreateWorkflowCommandValidator` to validate each task's Title, NodeType,
     AssigneeType, and OrderIndex.
   - Update `UpdateWorkflowCommand` to add `Tasks (IReadOnlyList<CreateWorkflowTaskDto>)`.
   - Update `UpdateWorkflowCommandHandler` to delete existing tasks then insert the new ones.
     Use `IWorkflowTaskRepository.GetByWorkflowIdAsync` to fetch existing, then
     `DeleteAsync` per task or a bulk delete method, then `AddRangeAsync` for new ones.
   - Update `UpdateWorkflowCommandValidator` to validate tasks the same way as create.
   - `DeleteWorkflowCommandHandler` — no changes needed. WorkflowTask records are removed
     by EF cascade delete configured on the FK relationship.

4. **Queries** (`Application/Features/Workflows/Queries/`):
   - Update `GetWorkflowByIdQueryHandler`: use `IWorkflowRepository.GetByIdAsync` then
     `IWorkflowTaskRepository.GetByWorkflowIdAsync` to load tasks. Access rule: allow if
     `workflow.WorkspaceId == _currentUser.WorkspaceId` OR
     `workflow.WorkspaceId == WellKnownIds.GlobalWorkspaceId`.
   - Update `GetWorkflowsQueryHandler`: fetch workspace workflows via
     `IWorkflowRepository.GetByWorkspaceAsync(_currentUser.WorkspaceId)` AND global
     templates via `IWorkflowRepository.GetByWorkspaceAsync(WellKnownIds.GlobalWorkspaceId)`.
     For each workflow, call `IWorkflowTaskRepository.GetByWorkflowIdAsync` to get
     `taskCount`. Map to `WorkflowSummaryDto`. Workspace workflows first, global templates
     after, each group ordered by CreatedAt descending.

5. **EF Configuration** (`Infrastructure/Persistence/Configurations/`):
   - Confirm `WorkflowTaskConfiguration` exists with FK to Workflow (cascade delete) and
     all required column constraints. If missing, create it following the same pattern as
     `WorkflowConfiguration`. Read `WorkflowConfiguration.cs` first for the pattern.

6. **Controller** (`Api/Controllers/WorkflowsController.cs`):
   - Add `[Authorize(Roles = "Admin")]` to the `Delete` action only. All other actions keep
     the class-level `[Authorize]` (any authenticated user).
   - No other changes to the controller.

7. **Run `dotnet build`** from `web-api/` and fix any compile errors before declaring done.

8. **No new migration required** if the WorkflowTask table already exists from Feature 3.
   If `WorkflowTaskConfiguration` was missing and a schema column is absent, add a migration
   following the ef-migration skill.

**Frontend Agent — ordered build sequence:**

Wait for Backend Agent to confirm all five endpoints are live and returning the contract
shapes above before implementing any API calls.

1. Create the module folder structure under `src/modules/workflows/`.
2. Write TypeScript enums: `AssigneeType`, `NodeType` (string enums matching backend).
3. Write TypeScript entity interfaces: `Workflow`, `WorkflowTask`.
4. Write DTO types: `WorkflowSummaryDto`, `WorkflowDto`, `WorkflowTaskDto`,
   `CreateWorkflowDto`, `UpdateWorkflowDto`, `CreateWorkflowTaskDto`.
5. Write `workflow-service.ts` — five methods wrapping `apiClient` calls.
6. Write React Query hooks in `use-workflows.ts`.
7. Write `WorkflowCard.tsx` — displays one summary card.
8. Write `DeleteWorkflowDialog.tsx` — AlertDialog with cancel/confirm.
9. Write `WorkflowsListPage.tsx` — fetches, renders grid, handles empty state and loading.
10. Wire the page into the existing router at `/workflows` (replace the Feature 6 stub).

**Handoff point:**

Backend Agent must confirm that all five endpoints (`GET /api/workflows`,
`GET /api/workflows/{id}`, `POST /api/workflows`, `PUT /api/workflows/{id}`,
`DELETE /api/workflows/{id}`) are reachable and returning the exact DTO shapes defined in
this brief's API Contract section before Frontend Agent makes any API calls. Backend Agent
should verify this manually using Swagger at `http://localhost:5000/swagger` or curl before
signalling ready.
