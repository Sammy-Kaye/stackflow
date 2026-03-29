# StackFlow — Phase 1 Granular Build Plan

> **Purpose:** This is the authoritative build plan for Phase 1. It replaces the tracker-level
> feature list in CLAUDE.md with concrete deliverables. Every feature here has a defined backend
> scope, frontend scope, API contract summary, and explicit out-of-scope boundary. Nothing is vague.
>
> **Rule:** A feature is not done until both backend and frontend are complete, PR-reviewed,
> and manually tested end-to-end. The tracker in CLAUDE.md updates only after that gate.

---

## Phase 1 — What "Done" Looks Like

When Phase 1 is complete, a user can:

1. Land on the StackFlow landing page and click into the app as a demo user (no real auth yet)
2. See the app shell — sidebar, navigation, greeting
3. Open the workflow builder, drag nodes onto the canvas, connect them, name the workflow, and save it
4. Browse the template library, preview a template, and use one to pre-populate the builder
5. Publish a workflow and spawn it into a live instance
6. See their assigned tasks on the My Tasks dashboard
7. Open a task, complete it or decline it with a note
8. See the active workflow board update as tasks are completed
9. Navigate to the active workflow instance and see the live canvas with step states overlaid
10. Open the audit trail and see a complete chronological record of every action

Auth (real login, registration, OTP, Google OAuth) is **Phase 2**. The demo user in Phase 1 is
a hardcoded stub — enough to run the app, replaced entirely in Phase 2.

---

## Build Order

Features must be built in this sequence. Later features depend on earlier ones.

```
1.  Project Scaffold
2.  Dev Auth Stub
3.  Domain Entities + DB
4.  Repository Layer
5.  Custom Mediator + Pipeline
6.  App Shell + Routing
7.  Landing Page
8.  Workflow CRUD (Templates)
9.  Workflow Builder UI
10. Template Library
11. WorkflowState Spawn
12. WorkflowTask Execution + Audit Trail
13. My Tasks Dashboard
14. Task Detail Page
15. Active Workflows Board
16. Workflow Instance Detail
```

Features 1–5 are backend-only. Feature 6–7 are frontend-only. Features 8–16 are full-stack,
built backend-first, then frontend against the live API.

---

## Feature 1 — Project Scaffold

**What this is:** The structural foundation every other feature sits on. No business logic.
No screens. Just the wiring that makes the project run.

### Backend deliverables

- Solution file: `StackFlow.sln` with four projects:
  - `StackFlow.Domain` — entities, enums, no dependencies
  - `StackFlow.Application` — commands, queries, handlers, DTOs, validators, interfaces
  - `StackFlow.Infrastructure` — EF Core, repositories, RabbitMQ client, email
  - `StackFlow.Api` — controllers, middleware, DI registration, Program.cs
- `Program.cs` wired: CORS, Swagger, JSON options (ISO 8601 dates, camelCase), global
  exception middleware, health endpoint at `GET /health`
- `appsettings.json` + `appsettings.Development.json` — placeholders for all config sections
  (ConnectionStrings, Jwt, Email, RabbitMq)
- `BaseApiController` — abstract class with `Mediator` property and `HandleResult<T>()` method
  that maps `Result<T>` to correct HTTP status codes

### Frontend deliverables

- Vite + React 19 + TypeScript project initialised at `web-frontend/`
- shadcn/ui initialised with New York style and correct CSS variables
- Folder structure in place: `modules/`, `store/`, `router/`, `design-reference/`
- `apiClient.ts` — single Axios instance with base URL from `VITE_API_URL` env var,
  request interceptor that attaches Bearer token from Redux auth state,
  response interceptor that handles 401 (clears auth, redirects to `/`)
- `main.tsx` — renders `<App />` wrapped in `<Provider store={store}>` and `<QueryClientProvider>`
- `.env.example` with `VITE_API_URL=http://localhost:5000`

### Docker deliverables

- `docker-compose.yml` at project root with services:
  - `postgres` — image postgres:16, port 5432, env vars from `.env`
  - `api` — builds from `web-api/`, port 5000, depends on postgres
  - `frontend` — builds from `web-frontend/`, port 3000, depends on api
  - `rabbitmq` — image rabbitmq:3-management, ports 5672 + 15672 (for Phase 2 — included now
    so the compose file doesn't change later)
- `.env.example` at project root — all required env vars documented with placeholder values

### Definition of done

- `docker compose up -d` starts all services without error
- `GET http://localhost:5000/health` returns `200 OK`
- `GET http://localhost:5000/swagger` renders Swagger UI
- `http://localhost:3000` renders a blank React app without console errors
- `dotnet build` passes with zero warnings in all four projects

---

## Feature 2 — Dev Auth Stub

**What this is:** A hardcoded single-user auth system that makes the frontend functional
without building the real auth system. This is deliberately simple — it is a throwaway
replaced entirely in Phase 2. It exists only so Phase 1 can be tested end-to-end.

**This is NOT the real auth system.** No ASP.NET Identity. No password hashing. No OTP.
No Google OAuth. Those are Phase 2.

### Backend deliverables

- `POST /api/auth/dev-login` endpoint — accepts `{ "email": "sam@stackflow.dev" }`,
  returns a JWT access token signed with the dev secret from config.
  Hardcoded: returns a fixed user identity (id, email, name, role: Admin).
  This endpoint is disabled in Production environment.
- JWT middleware registered in `Program.cs` — validates tokens on all protected endpoints
- `CurrentUserService` — reads `HttpContext.User` claims, exposes `UserId`, `Email`, `Role`,
  `WorkspaceId`. Registered as scoped. Used by all handlers that need to know who is acting.
- A hardcoded workspace seeded in the database on first run (id, name: "Demo Workspace")
  so the frontend always has a workspace to work within

### Frontend deliverables

- Redux `authSlice` — stores `{ accessToken, userId, email, name, role, workspaceId }`.
  `setAuth()` action and `clearAuth()` action.
- `useDevLogin` hook — calls `POST /api/auth/dev-login`, dispatches `setAuth()` on success,
  persists token to `localStorage` so it survives page refresh
- `ProtectedRoute` component — wraps authenticated routes. If no token in Redux store,
  checks `localStorage`, restores if present, otherwise redirects to `/`
- `AdminRoute` component — wraps admin-only routes. If role is not Admin, redirects to
  `/dashboard`
- Landing page "Enter app" button (see Feature 7) calls `useDevLogin` and routes to
  `/dashboard` on success

### Definition of done

- Clicking "Enter app" on the landing page calls `POST /api/auth/dev-login` and redirects
  to `/dashboard`
- Refreshing the browser does not log the user out (token persists via `localStorage`)
- Navigating to `/admin` as the demo user (Admin role) works
- `POST /api/auth/dev-login` returns `404` or is absent when `ASPNETCORE_ENVIRONMENT=Production`

---

## Feature 3 — Domain Entities + DB

**What this is:** All domain entities defined in C#, mapped to PostgreSQL tables via EF Core,
with the first migration applied. This is the data foundation every feature after this reads
from and writes to.

### Reserved workspace GUIDs

Two workspace rows are seeded as system records. These GUIDs are constants — they must never
change, must never be generated at runtime, and must be defined as static readonly fields in
a `WellKnownIds` or `SeedData` static class so every feature that needs them can reference
the same value without magic strings.

| GUID | Name | Purpose |
|---|---|---|
| `00000000-0000-0000-0000-000000000001` | Demo Workspace | The default workspace for the Phase 1 demo user. Shown in the UI. |
| `00000000-0000-0000-0000-000000000002` | Global | System workspace that owns the starter workflow templates. Never shown in the UI. Never used as a real workspace. |

**The Global workspace is a system record only.** It exists so that `Workflow.WorkspaceId`
can be non-nullable (`Guid`, not `Guid?`) throughout the entire codebase, while still
distinguishing starter templates from user-created workflows. Nothing in the application
creates new workflows under this workspace, presents it in dropdowns, or allows it to be
selected or deleted.

### Backend deliverables

**Entities in `StackFlow.Domain/Models/`:**

- `Workspace` — `Id (Guid)`, `Name`, `CreatedAt`
- `User` — `Id (Guid)`, `WorkspaceId`, `Email`, `FullName`, `Role (enum)`, `CreatedAt`
  - Role enum: `Admin | Member`
- `Workflow` — `Id (Guid)`, `WorkspaceId (Guid, non-nullable)`, `Name`, `Description`,
  `IsActive`, `CreatedAt`, `UpdatedAt`
  - Note: `WorkspaceId` is non-nullable. Starter templates use the Global workspace GUID
    (`00000000-0000-0000-0000-000000000002`) as their WorkspaceId. There is no null
    WorkspaceId anywhere in the schema.
- `WorkflowTask` — `Id (Guid)`, `WorkflowId`, `Title`, `Description`, `AssigneeType (enum)`,
  `DefaultAssignedToEmail`, `OrderIndex`, `DueAtOffsetDays`, `NodeType (enum)`,
  `ConditionConfig (string, nullable)`, `ParentTaskId (Guid, nullable)`
  - AssigneeType enum: `Internal | External`
  - NodeType enum: `Task | Approval | Condition | Notification | ExternalStep | Deadline`
- `WorkflowState` — `Id (Guid)`, `WorkflowId`, `WorkspaceId`, `Status (enum)`,
  `ContextType (enum)`, `BatchId (Guid, nullable)`, `ReferenceNumber (string)`,
  `StartedAt`, `CompletedAt (nullable)`, `CancelledAt (nullable)`
  - Status enum: `InProgress | Completed | Cancelled`
  - ContextType enum: `Standalone | Group`
- `WorkflowTaskState` — `Id (Guid)`, `WorkflowStateId`, `WorkflowTaskId`, `Status (enum)`,
  `AssignedToEmail`, `AssignedToUserId (Guid, nullable)`, `DueDate (nullable)`,
  `CompletionToken (string, nullable)`, `TokenExpiresAt (nullable)`, `IsTokenUsed (bool)`,
  `CompletionNotes (string, nullable)`, `DeclineReason (string, nullable)`, `Priority (enum)`
  - Status enum: `Pending | InProgress | Completed | Declined | Expired | Skipped`
  - Priority enum: `Low | Medium | High | Critical`
- `WorkflowAudit` — `Id (Guid)`, `WorkflowStateId`, `ActorUserId (Guid, nullable)`,
  `ActorEmail`, `Action (string)`, `OldValue (string, nullable)`, `NewValue (string, nullable)`,
  `Timestamp`
- `WorkflowTaskAudit` — `Id (Guid)`, `WorkflowTaskStateId`, `ActorUserId (Guid, nullable)`,
  `ActorEmail`, `Action (string)`, `OldValue (string, nullable)`, `NewValue (string, nullable)`,
  `Timestamp`

**EF Core setup in `StackFlow.Infrastructure`:**

- `AppDbContext` — inherits `DbContext`, one `DbSet<T>` per entity
- One `IEntityTypeConfiguration<T>` file per entity in `Infrastructure/Configurations/` —
  Fluent API only, no data annotations on entities
- All primary keys: `Guid`, configured as `ValueGeneratedOnAdd`
- All string lengths defined (Name: 200, Email: 256, Description: 2000, etc.)
- FK relationships with correct cascade delete rules (WorkflowTask → Workflow: cascade,
  WorkflowTaskState → WorkflowState: cascade, Audit tables: restrict on delete)
- Soft delete not used in Phase 1 — hard delete only
- First migration: `202603_InitialCreate` — creates all tables

**Seeding in `AppDbContext.OnModelCreating`:**

All seed data is defined using the reserved GUIDs from the `WellKnownIds` static class.

- **Global workspace seeded:**
  `id: "00000000-0000-0000-0000-000000000002"`, name: "Global", createdAt: fixed UTC date.
  This row exists only to satisfy the non-nullable `WorkspaceId` foreign key on global
  workflow templates. It is never surfaced in the application UI.

- **Demo workspace seeded:**
  `id: "00000000-0000-0000-0000-000000000001"`, name: "Demo Workspace", createdAt: fixed UTC date.

- **Three starter workflow templates seeded** with `WorkspaceId: "00000000-0000-0000-0000-000000000002"` (the Global workspace):
  1. Employee Onboarding — 6 task nodes
  2. Purchase Approval — 4 nodes (includes one Approval node)
  3. Client Offboarding — 5 task nodes
  - These seeded templates have `IsActive: true`.
  - They are identified as global by their `WorkspaceId` matching the Global workspace GUID.
  - They cannot be deleted — enforced in the delete handler by checking
    `workflow.WorkspaceId == WellKnownIds.GlobalWorkspaceId`.

### Definition of done

- `dotnet ef database update` applies the migration without error
- All tables exist in PostgreSQL with correct columns, types, and constraints
- `Workflow.WorkspaceId` column is non-nullable in the database
- Both the Global workspace row and the Demo workspace row are present in the `Workspaces` table
- Three starter workflow templates are present in the `Workflows` table with `WorkspaceId`
  set to `00000000-0000-0000-0000-000000000002`
- `dotnet build` passes with zero warnings

---

## Feature 4 — Repository Layer

**What this is:** The data access layer. Application layer talks to interfaces; Infrastructure
layer provides concrete implementations backed by EF Core. No business logic here — just
queries and writes.

### Backend deliverables

**Interfaces in `StackFlow.Application/Common/Interfaces/`:**

- `IWorkflowRepository` — `GetByIdAsync`, `GetByWorkspaceAsync`, `AddAsync`, `UpdateAsync`,
  `DeleteAsync`
- `IWorkflowTaskRepository` — `GetByWorkflowIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`,
  `AddRangeAsync`
- `IWorkflowStateRepository` — `GetByIdAsync`, `GetByWorkspaceAsync`, `GetActiveByWorkspaceAsync`,
  `AddAsync`, `UpdateAsync`
- `IWorkflowTaskStateRepository` — `GetByIdAsync`, `GetByWorkflowStateIdAsync`,
  `GetByAssignedUserAsync`, `AddAsync`, `UpdateAsync`, `AddRangeAsync`
- `IWorkflowAuditRepository` — `AddAsync`, `GetByWorkflowStateIdAsync`
- `IWorkflowTaskAuditRepository` — `AddAsync`, `GetByWorkflowTaskStateIdAsync`
- `IUnitOfWork` — `SaveChangesAsync(CancellationToken)`

**Implementations in `StackFlow.Infrastructure/Persistence/Repositories/`:**

- One implementation class per interface
- All queries use `AsNoTracking()` for reads, tracked entities for writes
- All methods accept `CancellationToken ct` as last parameter
- No raw SQL — EF Core LINQ only in Phase 1

**DI registration:**

- All repositories and `IUnitOfWork` registered in `Infrastructure/DependencyInjection.cs`
  as scoped services
- `AppDbContext` registered as scoped

### Definition of done

- All repository interfaces have a corresponding implementation
- `IUnitOfWork.SaveChangesAsync()` delegates to `AppDbContext.SaveChangesAsync()`
- DI registration compiles and resolves without error at startup

---

## Feature 5 — Custom Mediator + Pipeline Behaviors

**What this is:** The CQRS backbone. Every command and query in the system flows through this.
No MediatR — hand-rolled per CLAUDE.md.

### Backend deliverables

**Abstractions in `StackFlow.Application/Common/Mediator/`:**

- `IRequest<TResponse>` — marker interface
- `ICommand<TResponse>` — extends `IRequest<TResponse>`
- `IQuery<TResponse>` — extends `IRequest<TResponse>`
- `IRequestHandler<TRequest, TResponse>` — `Handle(TRequest, CancellationToken)` method
- `IPipelineBehavior<TRequest, TResponse>` — `Handle(TRequest, CancellationToken, next)` method
- `Mediator` — concrete class registered as scoped. Resolves handler from DI via
  `IServiceProvider`, wraps in pipeline behaviors in registration order, executes chain.
  Handler registration is via assembly scanning — `Mediator` uses `GetRequiredService<>`
  with the handler's concrete type resolved from the DI container.

**Pipeline behaviors (in execution order):**

1. `ValidationBehavior<TRequest, TResponse>` — resolves all `IValidator<TRequest>` from DI,
   runs them, returns `Result.Fail(validationErrors)` if any fail. Nothing reaches the handler
   with invalid input.
2. `LoggingBehavior<TRequest, TResponse>` — logs request type and elapsed time via
   `ILogger`. Logs at `Information` level on success, `Warning` on failure.

**Result pattern in `StackFlow.Application/Common/`:**

- `Result` — non-generic. `IsSuccess`, `IsFailure`, `Error (string)`. Static factory:
  `Result.Ok()`, `Result.Fail(string error)`
- `Result<T>` — generic. Same as above plus `Value (T)`. Static factory: `Result.Ok<T>(T value)`,
  `Result.Fail<T>(string error)`
- `BaseApiController.HandleResult<T>(Result<T>)` — maps to:
  - `IsSuccess` + value → `Ok(value)`
  - `IsFailure` + error containing "not found" → `NotFound(error)`
  - `IsFailure` + error containing "forbidden" → `Forbid()`
  - All other failures → `BadRequest(error)`

**DI registration:**

- `Mediator` registered as scoped
- Assembly scanning registers all `IRequestHandler<,>` implementations from
  `StackFlow.Application` assembly
- All `IPipelineBehavior<,>` implementations registered as scoped in pipeline order

### Definition of done

- A simple smoke-test command (`PingCommand` returning `Result<string>`) can be dispatched
  via `Mediator.Send()` and flows through both pipeline behaviors and the handler
- ValidationBehavior correctly blocks an invalid request before it reaches the handler
- All handlers discovered via assembly scanning — no manual registration

---

## Feature 6 — App Shell + Routing

**What this is:** The authenticated layout that every inner page lives inside. The sidebar,
top bar, route definitions, and route guards. No data, no business logic — pure structure.

### Frontend deliverables

**Router (`src/router/`):**

- `AppRouter.tsx` — React Router v6 with the following route tree:

```
/                          → LandingPage (public)
/dashboard                 → ProtectedRoute → AppShell → MyTasksPage
/tasks/:taskId             → ProtectedRoute → AppShell → TaskDetailPage
/workflows                 → ProtectedRoute → AppShell → WorkflowsListPage
/workflows/new             → ProtectedRoute → AdminRoute → AppShell → WorkflowBuilderPage
/workflows/:id/edit        → ProtectedRoute → AdminRoute → AppShell → WorkflowBuilderPage
/workflows/active          → ProtectedRoute → AppShell → ActiveWorkflowsBoardPage
/workflows/active/:id      → ProtectedRoute → AppShell → WorkflowInstanceDetailPage
/templates                 → ProtectedRoute → AppShell → TemplateLibraryPage
/admin                     → ProtectedRoute → AdminRoute → AppShell → AdminPage (Phase 2 placeholder)
*                          → NotFoundPage
```

**AppShell component (`src/modules/shared/ui/components/AppShell.tsx`):**

- Left sidebar — fixed, 240px wide, collapsible to 64px (icon-only) on toggle
- Sidebar content:
  - Top: StackFlow wordmark + logo mark (links to `/dashboard`)
  - Nav links (each has icon + label):
    - My Tasks → `/dashboard`
    - Workflows → `/workflows`
    - Active Workflows → `/workflows/active`
    - Templates → `/templates`
    - Admin → `/admin` — only rendered if `role === 'Admin'`
  - Bottom:
    - User avatar circle (initials) + name + email — clicking opens a dropdown with
      "Sign out" (clears Redux auth + localStorage, routes to `/`)
    - Notification bell icon with unread count badge (badge is `0` and hidden in Phase 1
      — wired for Phase 2 SignalR)
    - Collapse toggle button
- Active route highlighted with teal accent on the sidebar link
- Main content area: takes remaining width, scrollable, with a consistent `px-8 py-6` padding
- A subtle top bar within the content area: shows the current page title (passed as prop)
  and a slot for page-level action buttons

**Empty page stubs for all routes** — each page component returns a `<div>` with its page
title so routing can be verified before each page is built

### Definition of done

- All routes resolve without a console error
- Navigating to `/dashboard` without a token redirects to `/`
- Navigating to `/admin` as a non-admin redirects to `/dashboard`
- Sidebar collapse toggle works — icon-only mode at 64px, full mode at 240px
- Active route is highlighted correctly in the sidebar
- User avatar dropdown shows "Sign out" and clears auth on click

---

## Feature 7 — Landing Page

**What this is:** The public entry point. Static content with one goal: get the user into
the app. Design reference: `design-reference/landing/code.html`.

### Frontend deliverables

**`LandingPage.tsx` — sections in render order:**

1. **Navbar** — fixed top, transparent background that gains a dark fill on scroll past 60px
   - Left: StackFlow wordmark
   - Right: "Enter app" button (teal filled) — calls `useDevLogin`, on success routes
     to `/dashboard`. This replaces the real "Log in" / "Get started" buttons that Phase 2
     will introduce.

2. **Hero section** — full viewport height, centred
   - Headline: "Workflows that run your team, not the other way around."
   - Subheadline: "StackFlow gives small teams a clear process for every repeating task."
   - Primary CTA: "Enter app" (teal filled) — same as navbar button
   - Hero visual: a static representation of the My Tasks dashboard or Workflow Builder canvas,
     built as a styled `div` mockup (not a screenshot). Matches design reference.

3. **Features section** — three cards in a row
   - "Build once, run forever" — drag-and-drop builder description
   - "Everyone knows their next step" — task assignment description
   - "See the full picture" — audit trail and active workflows description

4. **Pricing section** — three tiers (Starter / Team / Pro) matching design reference.
   Buttons on this page route to `/dashboard` (demo mode — no real registration in Phase 1).

5. **Footer** — wordmark + Privacy / Terms / Contact links + copyright

**Loading state:** The "Enter app" button shows a spinner while `useDevLogin` is in flight.
If `useDevLogin` fails (API not responding), a Sonner toast: "Could not connect to the server.
Is the API running?"

### Definition of done

- Landing page renders at `http://localhost:3000/`
- "Enter app" button calls the dev login endpoint and routes to `/dashboard` on success
- Navbar background transitions on scroll
- Responsive: navbar collapses to a hamburger on mobile (hamburger opens a full-screen menu)
- Page matches design reference colour tokens and typography scale

---

## Feature 8 — Workflow CRUD (Templates)

**What this is:** The ability to create, read, update, and delete workflow templates. These are
the blueprints — no live execution yet. Full-stack feature: API first, then the Workflows List page.

### Backend deliverables

**Commands:**

- `CreateWorkflowCommand` — `Name (string)`, `Description (string)`, `WorkspaceId (Guid)`,
  `Tasks (List<CreateWorkflowTaskDto>)`. Returns `Result<WorkflowDto>`.
  - Validates: Name required, max 200 chars. At least one task required.
  - Sets `IsActive: true`, `CreatedAt` and `UpdatedAt` to UTC now
  - Creates all `WorkflowTask` records with correct `OrderIndex`
- `UpdateWorkflowCommand` — `Id`, `Name`, `Description`, `Tasks`. Returns `Result<WorkflowDto>`.
  - Validates: same as create. Workflow must exist and belong to the caller's workspace.
  - Replaces task list (delete old tasks, insert new ones) — simpler than diffing in Phase 1
  - Updates `UpdatedAt`
- `DeleteWorkflowCommand` — `Id`. Returns `Result`.
  - Validates: Workflow must exist, must belong to caller's workspace, must not be a global
    template. A workflow is a global template when its `WorkspaceId` equals the reserved
    Global workspace GUID (`WellKnownIds.GlobalWorkspaceId` =
    `00000000-0000-0000-0000-000000000002`). Attempting to delete a global template returns
    `Result.Fail("Global templates cannot be deleted")`.
  - Hard delete — removes workflow and all its `WorkflowTask` records

**Queries:**

- `GetWorkflowByIdQuery` — `Id`. Returns `Result<WorkflowDto>` (includes tasks).
- `GetWorkflowsQuery` — `WorkspaceId`. Returns `Result<List<WorkflowSummaryDto>>`.
  Includes: global templates (those whose `WorkspaceId == WellKnownIds.GlobalWorkspaceId`)
  plus the workspace's own workflows. The Global workspace row itself is never returned as
  a workspace option anywhere in the application.

**DTOs:**

- `WorkflowDto` — `Id`, `Name`, `Description`, `IsActive`, `CreatedAt`, `UpdatedAt`,
  `Tasks (List<WorkflowTaskDto>)`
- `WorkflowSummaryDto` — `Id`, `Name`, `Description`, `IsActive`, `TaskCount`,
  `ActiveInstanceCount` (count of WorkflowStates with Status=InProgress), `CreatedAt`,
  `IsGlobal (bool)` — derived by comparing `WorkspaceId == WellKnownIds.GlobalWorkspaceId`
- `WorkflowTaskDto` — all WorkflowTask fields
- `CreateWorkflowTaskDto` — `Title`, `Description`, `AssigneeType`, `DefaultAssignedToEmail`,
  `OrderIndex`, `DueAtOffsetDays`, `NodeType`, `ConditionConfig`, `ParentTaskId`

**Controller:** `WorkflowsController` — thin, one line per endpoint:

```
GET    /api/workflows              → GetWorkflowsQuery
GET    /api/workflows/{id}         → GetWorkflowByIdQuery
POST   /api/workflows              → CreateWorkflowCommand
PUT    /api/workflows/{id}         → UpdateWorkflowCommand
DELETE /api/workflows/{id}         → DeleteWorkflowCommand
```

All endpoints require authentication. Delete requires Admin role.

### Frontend deliverables

**`WorkflowsListPage.tsx`** (`/workflows`):

- Fetches `GET /api/workflows` via `useWorkflows()` hook
- Displays a list/grid. Each item shows:
  - Workflow name (bold)
  - Description (truncated to 2 lines)
  - "Global template" badge if `isGlobal: true` (these cannot be deleted)
  - Status: Draft / Published badge
  - Number of active instances
  - Last edited date
  - Action buttons: `Edit` → `/workflows/{id}/edit`, `Delete` (Admin only, disabled for global)
- Top bar: "Workflows" title + `New workflow` button → `/workflows/new`
- Empty state: "No workflows yet. Build your first one." + `Create workflow` button
- Skeleton loader while fetching
- Delete shows an `AlertDialog`: "Delete this workflow?" with confirm/cancel

**Service + hooks in `src/modules/workflows/`:**

- `workflowService.ts` — `getAll()`, `getById(id)`, `create(dto)`, `update(id, dto)`,
  `delete(id)`
- `useWorkflows()` — React Query, query key `['workflows']`
- `useWorkflow(id)` — React Query, query key `['workflows', id]`
- `useCreateWorkflow()` — mutation, invalidates `['workflows']` on success
- `useUpdateWorkflow()` — mutation, invalidates `['workflows']` and `['workflows', id]`
- `useDeleteWorkflow()` — mutation, invalidates `['workflows']` on success

### Definition of done

- `POST /api/workflows` creates a workflow with tasks and returns the full DTO
- `GET /api/workflows` returns both global templates and workspace workflows
- `DELETE /api/workflows/{id}` on a global template returns an error (not allowed)
- Workflows list page shows all workflows with correct badges and counts
- Delete confirmation works — workflow disappears from list without page refresh
- Skeleton loader shows during fetch

---

## Feature 9 — Workflow Builder UI

**What this is:** The drag-and-drop canvas where workflows are built. React Flow powers the
canvas. This is frontend-only — it uses the Feature 8 API for saving. Design reference:
`design-reference/workflows/builder-1.html` and `design-reference/workflows/builder-2.html`.

### Frontend deliverables

**`WorkflowBuilderPage.tsx`** (`/workflows/new` and `/workflows/:id/edit`):

**Canvas area (React Flow):**

- Custom node types (each is a styled React component):
  - `TaskNode` — white card, title, assignee, due offset badge. Blue border.
  - `ApprovalNode` — white card, title, "Requires approval" label. Purple border.
  - `ConditionNode` — diamond shape, label "If / Else". Amber border.
  - `NotificationNode` — white card, title, "Notification" label. Teal border.
  - `StartNode` — fixed, cannot be deleted, cannot be moved (anchored top-centre). Green.
  - `EndNode` — fixed, anchored bottom. Always the last node. Grey.
- Edges: animated connection lines, deletable (click to select, Delete key to remove)
- Nodes are connectable — drag a handle from one node to another to create an edge
- Nodes can be dragged freely within the canvas
- `ConditionNode` has two output handles: "Yes" (right) and "No" (bottom) — enabling branches

**Left palette panel (120px):**

Vertical list of draggable node types. User drags a type from the palette onto the canvas;
a node appears at the drop position. Node types available in Phase 1:

- Task (blue)
- Approval (purple)
- Condition (amber)
- Notification (teal)

ExternalStep and Deadline nodes appear in the palette but show a "Phase 2" tooltip and cannot
be dropped onto the canvas.

**Right properties panel (280px, opens on node select):**

- Closes when canvas is clicked with no node selected
- Shows different fields based on node type:

  **For Task nodes:** Title (text input, required), Description (textarea, optional),
  Assigned to (dropdown populated from workspace users + "Unassigned" option),
  Due offset (number input, label: "+N days from workflow start")

  **For Approval nodes:** Same as Task but "Assigned to" label changes to "Approver"

  **For Condition nodes:** Branch labels (Yes label, No label — text inputs, defaults to "Yes" / "No")

  **For Notification nodes:** Title (text input), Message (textarea)

- All field changes update the node's data in React Flow state immediately (controlled)

**Top bar:**

- Back button: `← Workflows` → navigates to `/workflows` (with unsaved-changes guard)
- Workflow name: editable inline text input (click to edit, Enter or blur to confirm).
  Default: "Untitled workflow"
- Auto-save indicator: "Saving..." / "Saved" / "Unsaved changes" — appears in top bar
- `Save draft` button — calls `POST /api/workflows` (create) or `PUT /api/workflows/{id}` (update)
- `Publish` button — calls save first, then opens confirmation dialog:
  "Publish this workflow? Once published, team members can be assigned tasks from it."
  On confirm: `PUT /api/workflows/{id}` with `IsActive: true`, routes to `/workflows/active`

**Canvas controls (bottom-right):**

- Zoom in / Zoom out / Fit to screen buttons (React Flow built-ins)
- Mini-map toggle (React Flow MiniMap component)

**Auto-save:** Debounced 30s after any canvas change. Shows "Saving..." then "Saved".

**Unsaved changes guard:** Browser `beforeunload` event fires if there are unsaved changes.

**First-use hint banner** (shown only on first visit, stored in localStorage):
"New to the builder? Start with a Task node — drag it onto the canvas and connect it to Start."
Dismissed with "Got it" button.

**Edit mode differences** (when URL is `/workflows/:id/edit`):

- Loads existing workflow via `useWorkflow(id)` and populates canvas from the task list
- Top bar shows `Save changes` instead of `Save draft`
- If workflow is published (`IsActive: true`): amber banner "This workflow is currently live.
  Changes will apply to new instances only — running instances are not affected."
- `···` overflow menu in top bar (Admin only): `Delete workflow` → confirmation dialog →
  `useDeleteWorkflow()` → routes to `/workflows`

### Definition of done

- Dragging a Task node from the palette onto the canvas creates a node
- Connecting two nodes creates an edge
- Clicking a node opens the properties panel with the correct fields
- Editing the title in the properties panel updates the node label in real time
- Save draft calls the correct API and shows "Saved" on success
- Publish shows the confirmation dialog and routes to `/workflows/active` after confirm
- Loading an existing workflow renders all its nodes and edges in the correct positions
- Edit mode shows the "live workflow" amber banner when workflow is published
- Delete from the `···` menu shows confirmation and removes the workflow

---

## Feature 10 — Template Library

**What this is:** Browse, preview, and use workflow templates. The three global starter
templates from Feature 3 appear here. Design reference: `design-reference/workflows/templates.html`.

### Frontend deliverables

**`TemplateLibraryPage.tsx`** (`/templates`):

- Fetches `GET /api/workflows` — filters to show global templates + workspace templates
- Filter and search:
  - Search input filters by name (client-side, no API call)
  - Category filter: All / Approvals / Onboarding / HR / Operations / Custom
    (Phase 1: all global templates are categorised at seed time)
  - Sort: Most used (ActiveInstanceCount desc) / Newest / Alphabetical
- Template cards grid (3 columns desktop, 2 tablet, 1 mobile):
  - Template name (bold)
  - Category badge (colour coded)
  - Description (2 lines, truncated)
  - "N steps" count
  - "Created {date}"
  - Action buttons: `Use` (teal), `Preview` (ghost), `Delete` (danger — Admin only,
    disabled and tooltip "Global templates cannot be deleted" for global templates)
- Top bar: "Templates" title + `Create new template` button → `/workflows/new` (Admin only)
- Empty state for when workspace has no templates and global ones are loading

**Template preview modal:**

- Opens when `Preview` is clicked on any card
- Full-screen modal (not a drawer, not a new page)
- Renders the workflow canvas in read-only mode (React Flow, `nodesDraggable: false`,
  `nodesConnectable: false`, `elementsSelectable: false`)
- Nodes rendered from the template's `WorkflowTask` list — node positions are calculated
  automatically in a linear vertical layout (no saved positions in Phase 1)
- Two buttons at modal bottom: `Use this template` (teal) and `Close`

**`Use` button behaviour:**

- Calls `POST /api/workflows` with the template's tasks pre-populated (clone the template
  into a new workspace workflow)
- Routes to `/workflows/{newId}/edit` with the builder pre-populated

### Definition of done

- Template library shows three global starter templates on first load
- Search filters the card list in real time
- Preview modal opens and shows a read-only React Flow canvas with the template's nodes
- "Use" creates a new workflow and opens the builder pre-populated with the template's tasks
- Global template delete button is disabled with a tooltip

---

## Feature 11 — WorkflowState Spawn

**What this is:** The action of launching a live workflow instance from a template. This is
the moment a workflow blueprint becomes a real running process.

### Backend deliverables

**Command: `SpawnWorkflowCommand`**

- Inputs: `WorkflowId (Guid)`, `WorkspaceId (Guid)`, `ActorEmail (string)`
- Validates: Workflow must exist, must be active (`IsActive: true`), and must either belong
  to the caller's workspace or be a global template (i.e.
  `workflow.WorkspaceId == WellKnownIds.GlobalWorkspaceId`)
- Creates a `WorkflowState` with `Status: InProgress`, `StartedAt: UtcNow`,
  `ReferenceNumber` generated as `WF-{YYYYMMDD}-{4-char random hex}`
- For each `WorkflowTask` in the workflow, creates a `WorkflowTaskState` with:
  - `Status: Pending` (except the first task, which gets `Status: InProgress`)
  - `AssignedToEmail` from `WorkflowTask.DefaultAssignedToEmail`
  - `DueDate` calculated as `UtcNow + DueAtOffsetDays`
  - `Priority: Medium` (default — user can change after spawn, Phase 2)
- Writes a `WorkflowAudit` entry: `Action: "WorkflowStarted"`, `ActorEmail`, `Timestamp`
- Returns `Result<WorkflowStateDto>`

**`WorkflowStateDto`** — `Id`, `WorkflowId`, `WorkflowName`, `Status`, `ReferenceNumber`,
`StartedAt`, `TaskStates (List<WorkflowTaskStateDto>)`

**`WorkflowTaskStateDto`** — all WorkflowTaskState fields plus `TaskTitle`, `NodeType`

**Query: `GetWorkflowStatesQuery`** — `WorkspaceId`. Returns active instances.
Includes workflow name and progress (completed task count / total task count).

**Query: `GetWorkflowStateByIdQuery`** — `Id`. Returns full instance detail with all task states.

**Controller:** `WorkflowStatesController`:

```
GET    /api/workflow-states              → GetWorkflowStatesQuery
GET    /api/workflow-states/{id}         → GetWorkflowStateByIdQuery
POST   /api/workflow-states              → SpawnWorkflowCommand
DELETE /api/workflow-states/{id}/cancel  → CancelWorkflowCommand (see Feature 12)
```

### Frontend deliverables

**"Start new workflow" modal** (on Active Workflows Board — see Feature 15):

- A `Dialog` component triggered by the `Start new workflow` button
- Step 1: Dropdown to select a published workflow from `GET /api/workflows`
- Step 2: Confirmation — shows workflow name and task count: "This will create a new
  instance of 'Employee Onboarding' with 6 tasks."
- `Launch` button → calls `POST /api/workflow-states`
- On success: closes modal, invalidates `['workflow-states']` React Query cache,
  Sonner toast: "Workflow launched — instance #WF-20260328-A4F2 started"

### Definition of done

- `POST /api/workflow-states` creates a WorkflowState and all WorkflowTaskStates
- First task has `Status: InProgress`, all others `Status: Pending`
- DueDate is correctly calculated from `DueAtOffsetDays`
- A WorkflowAudit entry is written
- ReferenceNumber is generated in the correct format
- The spawn modal launches the workflow and the board updates

---

## Feature 12 — WorkflowTask Execution + Audit Trail

**What this is:** The core domain action — completing and declining tasks, with a full audit
trail written on every mutation. This is what makes StackFlow actually work.

### Backend deliverables

**Commands:**

- `CompleteTaskCommand` — `TaskStateId (Guid)`, `ActorEmail (string)`,
  `Notes (string, optional)`
  - Validates: task exists, status is `InProgress`, actor is the assigned user or Admin
  - Sets status to `Completed`, `CompletionNotes`, timestamp
  - Calls `AdvanceWorkflowState()` — private method that looks at the workflow structure
    and sets the next task to `InProgress`. If no next task: sets WorkflowState to `Completed`
  - Writes `WorkflowTaskAudit`: `Action: "TaskCompleted"`, old status, new status, actor, timestamp
  - If WorkflowState completed: writes `WorkflowAudit`: `Action: "WorkflowCompleted"`
  - Returns `Result<WorkflowTaskStateDto>`

- `DeclineTaskCommand` — `TaskStateId (Guid)`, `ActorEmail (string)`,
  `Reason (string, required)`
  - Validates: same as complete. Reason is required (min 5 chars).
  - Sets status to `Declined`, `DeclineReason`
  - Does NOT advance the workflow — a declined task halts the workflow at that step
  - Writes `WorkflowTaskAudit`: `Action: "TaskDeclined"`, reason, actor, timestamp
  - Returns `Result<WorkflowTaskStateDto>`

- `CancelWorkflowCommand` — `WorkflowStateId (Guid)`, `ActorEmail (string)`
  - Admin only (enforced in handler via `CurrentUserService`)
  - Sets WorkflowState status to `Cancelled`, `CancelledAt`
  - Sets all `InProgress` and `Pending` task states to `Skipped`
  - Writes `WorkflowAudit`: `Action: "WorkflowCancelled"`, actor, timestamp
  - Returns `Result`

- `ReassignTaskCommand` — `TaskStateId`, `NewAssigneeEmail`, `ActorEmail`
  - Admin only
  - Updates `AssignedToEmail` on the task state
  - Writes audit entry: `Action: "TaskReassigned"`, old assignee, new assignee
  - Returns `Result<WorkflowTaskStateDto>`

**Query: `GetMyTasksQuery`** — `AssignedToEmail`. Returns all task states assigned to the
user, ordered by: overdue first, then due date ascending, then priority descending.
Includes workflow name and reference number for each task.

**Query: `GetAuditTrailQuery`** — `WorkflowStateId`. Returns all `WorkflowAudit` and
`WorkflowTaskAudit` entries for the instance, merged and sorted by timestamp descending.

**Controller additions:**

```
POST   /api/workflow-task-states/{id}/complete   → CompleteTaskCommand
POST   /api/workflow-task-states/{id}/decline    → DeclineTaskCommand
POST   /api/workflow-task-states/{id}/reassign   → ReassignTaskCommand
GET    /api/workflow-task-states/my-tasks        → GetMyTasksQuery
GET    /api/workflow-states/{id}/audit           → GetAuditTrailQuery
DELETE /api/workflow-states/{id}/cancel          → CancelWorkflowCommand
```

**Audit trail rule (enforced in PR review):** Every command that mutates a WorkflowState
or WorkflowTaskState writes an audit entry before returning. The PR reviewer checks this
explicitly.

### Frontend deliverables

- `workflowTaskStateService.ts` — `complete(id, notes?)`, `decline(id, reason)`,
  `reassign(id, newEmail)`, `getMyTasks()`, `getAuditTrail(workflowStateId)`
- `useMyTasks()` hook — React Query
- `useCompleteTask()` mutation — on success: invalidates `['my-tasks']`, `['workflow-states']`
- `useDeclineTask()` mutation — same invalidation
- `useCancelWorkflow()` mutation — invalidates `['workflow-states']`
- `useAuditTrail(workflowStateId)` hook — React Query

*(These hooks are used in Features 13, 14, 15, 16 below)*

### Definition of done

- Completing a task sets its status to `Completed` and advances the next task to `InProgress`
- When the last task is completed, the WorkflowState status becomes `Completed`
- Declining a task sets its status to `Declined` and leaves the workflow halted at that step
- Cancelling a workflow sets all pending/in-progress tasks to `Skipped`
- Every mutation writes the correct audit entry with actor, action, old value, new value
- `GET /api/workflow-task-states/my-tasks` returns tasks filtered to the authenticated user

---

## Feature 13 — My Tasks Dashboard

**What this is:** The home screen for every authenticated user. Their assigned tasks, filterable
and sortable, with inline complete/decline actions. Design reference:
`design-reference/dashboard/my-tasks.html`.

### Frontend deliverables

**`MyTasksPage.tsx`** (`/dashboard`):

**Greeting header:**
"Good morning, Sam." — uses the name from Redux auth state. Time-sensitive:
Good morning (before 12) / Good afternoon (12–17) / Good evening (17+).
Disappears on scroll past 40px.

**Stats bar** — four stat chips, each a subtle card:

| Chip | Query logic | Colour cue |
|---|---|---|
| Due today | tasks where `dueDate` is today | Amber if count > 0 |
| Overdue | tasks where `dueDate` < today and status !== Completed/Declined | Red if count > 0 |
| In progress | tasks where status === InProgress | Blue |
| Completed this week | tasks where status === Completed and completedAt in last 7 days | Green |

Clicking a stat chip applies that filter to the task list below.

**Filter bar:**

- Dropdown: Workflow (all workflow names the user has tasks in)
- Dropdown: Priority (All / Low / Medium / High / Critical)
- Dropdown: Status (All / Pending / InProgress / Completed / Declined)
- `Clear filters` text link — only visible when a filter is active

**Task list table:**

Columns: Workflow | Task | Due date | Priority | Status | Actions

- Due date displayed as: "Today", "Tomorrow", "Mar 30", "3 days ago" (overdue formatted
  in red via date-fns)
- Priority badge: colour-coded pill (Low=grey, Medium=blue, High=amber, Critical=red)
- Status badge: colour-coded pill (Pending=grey, InProgress=blue, Completed=green, Declined=red)
- Overdue rows: subtle `border-l-2 border-red-400` left border
- Completed/declined rows: muted opacity (`opacity-60`) — not hidden
- Default sort: overdue first, then due date asc, then priority desc
- Pagination: 25 rows per page, Previous/Next buttons at bottom

**Inline actions:**

- `Complete` button — opens a `Popover` (not a full modal):
  "Mark as complete?" with optional notes textarea + `Confirm` (teal) and `Cancel`
  On confirm: calls `useCompleteTask()`, row status badge updates optimistically
- `Decline` button — same popover pattern with a required reason text field.
  On confirm: calls `useDeclineTask()`, row updates optimistically

**Loading state:** Full skeleton table — correct number of rows and columns, pulsing.

**Empty state** (no tasks): Centred illustration placeholder + "No tasks yet" heading +
body text + two CTA buttons: `Create a workflow` → `/workflows/new`,
`Browse templates` → `/templates`

### Definition of done

- Dashboard loads and shows all tasks assigned to the demo user
- Stats bar counts update correctly (overdue count, due today count, etc.)
- Clicking a stat chip filters the table to that category
- Completing a task via the popover updates the row status in place without page reload
- Declining a task requires a reason and updates the row status
- Overdue rows have a visible red left border
- Pagination works with 25 rows per page

---

## Feature 14 — Task Detail Page

**What this is:** The full detail view of a single task — context, instructions, action buttons,
and the audit trail made human-readable. **No design reference exists for this screen** — build to the spec below using the design system tokens in `design-reference/DESIGN.md`.

### Frontend deliverables

**`TaskDetailPage.tsx`** (`/tasks/:taskId`):

**Breadcrumb:** `← My Tasks` → `/dashboard`
Below breadcrumb: `My Tasks › [Workflow name] › [Task title]`

**Two-column layout:**

**Left column (65%):**

- Task title (h1, large)
- Status badge + Priority badge (inline, small)
- Due date — "Due today" / "Due tomorrow" / "Due Mar 30" / "3 days overdue" (red if past)
- "Assigned to" — avatar circle (initials) + name + "Assigned to you" if the current user
- Task instructions — read-only rendered text (description from WorkflowTask)
- Divider

**Action section (conditional on node type and user):**

*For Task nodes — assigned user or Admin:*
- `Mark as complete` (teal filled, full width)
- Optional notes textarea: "Add a note before completing (optional)"
- `Decline task` (ghost/text, small) — only if user is assigned or Admin
- On "Mark as complete": confirmation `AlertDialog` → calls `useCompleteTask()`
- On "Decline": opens an inline form requiring a reason → calls `useDeclineTask()`

*For Approval nodes — assigned approver or Admin:*
- Side-by-side buttons: `Approve` (green filled) and `Decline` (red outlined)
- Required comment/reason textarea appears below Decline button when clicked
- "Your decision" label above buttons

*For a task the current user is not assigned to (and is not Admin):*
- Read-only: "This task is assigned to [name]" — no action buttons

**Right column (35%):**

*Workflow context card:*
- "Part of" label
- Workflow name (links to `/workflows/active/{instanceId}`)
- "Step 3 of 6" — derived from task position in workflow
- Simple linear progress bar showing current step proportion

*Activity feed:*
- Chronological list of audit entries from `useAuditTrail(workflowStateId)`
- Each entry: user initials avatar, action text, relative timestamp (date-fns `formatDistanceToNow`)
- Action text examples: "Sam completed this task", "Alice approved Step 2",
  "Workflow started by Sam"
- Read-only, scrolls within the column

### Definition of done

- Task detail page loads correctly for the demo user's tasks
- Task type determines which action buttons appear
- Completing a task from this page writes the audit entry and navigates back to `/dashboard`
- Declining an approval task requires a reason and is enforced (cannot submit empty)
- Activity feed shows the correct audit history in reverse chronological order
- Workflow context card shows the correct step number and progress bar

---

## Feature 15 — Active Workflows Board

**What this is:** The operations view — all running, completed, and cancelled workflow instances.
Design reference: `design-reference/workflows/board.html`.

### Frontend deliverables

**`ActiveWorkflowsBoardPage.tsx`** (`/workflows/active`):

**Top bar:** "Active Workflows" title + `Start new workflow` button → opens spawn modal

**Filter + search bar:**
- Search input — filters by workflow name (client-side)
- Status pill tabs: All · In Progress · Completed · Cancelled
- Sort dropdown: Started date (newest first, default) / Name / Progress

**Workflow instance cards** (2-column grid):

Each card:
- Workflow name (bold)
- Reference number badge: "WF-20260328-A4F2"
- Status badge: In Progress (blue) / Completed (green) / Cancelled (grey)
- Progress bar: "3 of 6 tasks complete" — filled teal bar
- Started date: "Started Mar 22, 2026"
- Recent activity (last 3 audit entries, small text, from the instance's audit trail):
  "Alice completed Step 2 · 1h ago"
- `···` overflow menu (Admin only): "View details" → `/workflows/active/{id}`,
  "Cancel workflow" → confirmation `AlertDialog` → `useCancelWorkflow()`

**Clicking anywhere on the card** (outside the `···` menu) routes to `/workflows/active/{id}`

**Spawn modal** (wired here, logic from Feature 11):
- Step 1: Dropdown of published workflows
- Step 2: Confirmation with workflow name + task count
- `Launch` button

**Empty state:**
"No workflows running" + "Publish a workflow to start tracking your team's progress here."
+ `Build a workflow` button → `/workflows/new`

**Loading:** Skeleton cards — correct card shape and proportions, pulsing.

### Definition of done

- Board shows all workflow instances with correct status badges and progress bars
- Status pill tab filtering works (shows only In Progress instances, etc.)
- Spawn modal creates a new instance and it appears on the board
- Cancel workflow from the `···` menu shows confirmation and updates the card status
- Clicking a card routes to the instance detail page

---

## Feature 16 — Workflow Instance Detail

**What this is:** The live read-only canvas for a running workflow instance — step states
overlaid on the nodes, full audit trail below. The "mission control" view for a single instance.

### Frontend deliverables

**`WorkflowInstanceDetailPage.tsx`** (`/workflows/active/:id`):

**Top bar:**
- `← Active Workflows` breadcrumb → `/workflows/active`
- Page title: workflow name + reference number: "Employee Onboarding · WF-20260328-A4F2"
- Admin only action bar (right side): `Cancel workflow` (red outlined button)
  → confirmation `AlertDialog` → `useCancelWorkflow()` → navigates to `/workflows/active`

**Read-only React Flow canvas (top section, ~55% of page height):**

Renders the workflow's nodes and edges in read-only mode. Node visual state driven by
`WorkflowTaskState.status`:

- `Pending` tasks: muted grey fill, no border emphasis
- `InProgress` task: teal border, subtle teal background tint, a pulsing ring animation
  (CSS `animate-pulse` on the border — one animation worth including)
- `Completed` tasks: green fill, checkmark icon overlay in the node
- `Declined` tasks: red fill, X icon overlay
- `Skipped` tasks: grey, strikethrough text on the title

React Flow is read-only: `nodesDraggable: false`, `nodesConnectable: false`,
`elementsSelectable: false`, `panOnDrag: true`, `zoomOnScroll: true`.

Node positions in Phase 1 are calculated automatically in a linear layout from the
`OrderIndex` on each `WorkflowTask` — no stored positions. Layout algorithm: simple
vertical stack with 120px between nodes, centred horizontally.

**Workflow summary strip (between canvas and audit trail):**

A horizontal strip with four values:
- Status badge
- Progress: "3 of 6 tasks complete"
- Started: "Mar 22, 2026"
- Completed / Cancelled at (shown only if applicable)

**Full audit trail (bottom section):**

Chronological feed of all `WorkflowAudit` and `WorkflowTaskAudit` entries from
`useAuditTrail(id)`:

Each entry:
- User initials avatar (circle)
- Action text (human-readable — frontend formats the `Action` string:
  "TaskCompleted" → "Sam completed [task title]")
- Relative + absolute timestamp: "2 hours ago · Mar 28, 2026 at 14:32"
- Subtle divider between entries

Feed is loaded all at once (no pagination in Phase 1 — audit trails are short in Phase 1).

### Definition of done

- Canvas renders the correct nodes for the workflow template
- Node visual states reflect the actual `WorkflowTaskState.status` values
- The active (InProgress) task has a visible pulsing teal ring
- Completing a task on the My Tasks page and then navigating here shows the updated states
- Full audit trail shows all events in chronological order with correct human-readable text
- Cancel workflow from the admin action bar works and navigates back to the board

---

## What Phase 1 Does NOT Include

These are explicitly out of scope and must not be built, even if an agent suggests it:

- Real authentication (ASP.NET Identity, password hashing, JWT rotation) → Phase 2
- User registration or login pages → Phase 2
- Google OAuth → Phase 2
- Email OTP → Phase 2
- RabbitMQ consumers → Phase 2
- Email sending (any kind) → Phase 2
- SignalR real-time push → Phase 2 (Phase 1 uses page-load/refresh data only)
- In-app notification preferences → Phase 2
- File attachments → Phase 2
- Task reassignment UI (the command exists, but no UI surface for it) → Phase 2
- Admin panel pages → Phase 2
- User management → Phase 2
- Undo/redo on the canvas → Phase 2
- Parallel branches (Condition nodes support if/else only) → Phase 2
- External task token flow → Phase 2
- Analytics or charts → Phase 3
- Calendar integration → Phase 3
- Proxmox deployment → Phase 3
- Onboarding wizard → Phase 2 (not needed when there's no real user registration)

---

## Updated Phase Tracker

This replaces the tracker in `CLAUDE.md`. Update statuses here as features complete.

| # | Feature | Backend | Frontend | PR Reviewed | Manually Tested | Done |
|---|---|---|---|---|---|---|
| 1 | Project Scaffold | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| 2 | Dev Auth Stub | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| 3 | Domain Entities + DB | ⬜ | — | ⬜ | ⬜ | ⬜ |
| 4 | Repository Layer | ⬜ | — | ⬜ | ⬜ | ⬜ |
| 5 | Custom Mediator + Pipeline | ⬜ | — | ⬜ | ⬜ | ⬜ |
| 6 | App Shell + Routing | — | ⬜ | ⬜ | ⬜ | ⬜ |
| 7 | Landing Page | — | ⬜ | ⬜ | ⬜ | ⬜ |
| 8 | Workflow CRUD (Templates) | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| 9 | Workflow Builder UI | — | ⬜ | ⬜ | ⬜ | ⬜ |
| 10 | Template Library | — | ⬜ | ⬜ | ⬜ | ⬜ |
| 11 | WorkflowState Spawn | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| 12 | WorkflowTask Execution + Audit Trail | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |
| 13 | My Tasks Dashboard | — | ⬜ | ⬜ | ⬜ | ⬜ |
| 14 | Task Detail Page | — | ⬜ | ⬜ | ⬜ | ⬜ |
| 15 | Active Workflows Board | — | ⬜ | ⬜ | ⬜ | ⬜ |
| 16 | Workflow Instance Detail | — | ⬜ | ⬜ | ⬜ | ⬜ |

Legend: ⬜ Not started · ✅ Complete · — Not applicable for this feature
