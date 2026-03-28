# StackFlow — Phase 1 Prompt Sequence

> **How to use this file:** Copy the exact prompt shown, paste it into Claude Code,
> follow the test steps before moving on. Do not skip the test gate — a broken
> foundation breaks everything built on top of it.
>
> **The pattern for every feature:**
> 1. `Brief: [Feature Name]` → feature-provider produces brief + API contract
> 2. Read the brief. If good → `Build this`
> 3. `Review this` → pr-reviewer runs checklist
> 4. Fix anything flagged, then manually test
> 5. `Update tracker: [Feature Name] is done` → feature-provider marks it complete

---

## Current status

| # | Feature | Done |
|---|---|---|
| 1 | Project Scaffold | ✅ |
| 2 | Dev Auth Stub | ⬜ |
| 3 | Domain Entities + DB | ⬜ |
| 4 | Repository Layer | ⬜ |
| 5 | Custom Mediator + Pipeline | ⬜ |
| 6 | App Shell + Routing | ⬜ |
| 7 | Landing Page | ⬜ |
| 8 | Workflow CRUD (Templates) | ⬜ |
| 9 | Workflow Builder UI | ⬜ |
| 10 | Template Library | ⬜ |
| 11 | WorkflowState Spawn | ⬜ |
| 12 | WorkflowTask Execution + Audit Trail | ⬜ |
| 13 | My Tasks Dashboard | ⬜ |
| 14 | Task Detail Page | ⬜ |
| 15 | Active Workflows Board | ⬜ |
| 16 | Workflow Instance Detail | ⬜ |

---

## Feature 2 — Dev Auth Stub

**What it builds:** Hardcoded single-user login so the frontend can function. Not real auth —
that's Phase 2. Just enough to get past the gate and into the app.

```
Brief: Dev Auth Stub
```

Review the brief. Then:

```
Build this
```

Backend-agent runs first (JWT endpoint, CurrentUserService), then frontend-agent
(authSlice, useDevLogin hook, ProtectedRoute, AdminRoute).

```
Review this
```

**Manual test checklist:**
- [ ] `POST http://localhost:5000/api/auth/dev-login` with body `{"email":"sam@stackflow.dev"}` returns a JWT
- [ ] "Enter app" button on the landing page calls dev-login and routes to `/dashboard`
- [ ] Refreshing the browser does not log you out (token in localStorage)
- [ ] Navigating to `/admin` works (demo user is Admin role)
- [ ] `POST /api/auth/dev-login` returns 404 when `ASPNETCORE_ENVIRONMENT=Production`

```
Update tracker: Dev Auth Stub is done
```

---

## Feature 3 — Domain Entities + DB

**What it builds:** All domain entities in C#, EF Core Fluent API config, AppDbContext,
first migration, seeded demo workspace and 3 starter templates.

```
Brief: Domain Entities + DB
```

> **Note in the brief review:** Confirm `Category (string, nullable)` is on the
> `Workflow` entity — it's needed for Template Library filtering in Feature 10.
> If missing, flag it before approving.

Backend-only feature — no frontend-agent.

```
Build this
```

```
Review this
```

**Manual test checklist:**
- [ ] `dotnet ef database update` applies without error
- [ ] Open DBeaver — all tables present with correct columns
- [ ] Demo workspace row exists in `Workspaces` table
- [ ] 3 starter templates exist in `Workflows` table with `WorkspaceId = null`
- [ ] `dotnet build` passes zero warnings

```
Update tracker: Domain Entities + DB is done
```

---

## Feature 4 — Repository Layer

**What it builds:** 7 repository interfaces (Application layer), 7 implementations
(Infrastructure layer), IUnitOfWork, DI registration. Pure data access — no business logic.

```
Brief: Repository Layer
```

Backend-only feature.

```
Build this
```

```
Review this
```

**Manual test checklist:**
- [ ] `dotnet build` passes zero warnings
- [ ] `GET http://localhost:5000/health` still returns 200 (DI resolves without error)
- [ ] No `SaveChangesAsync()` calls inside any repository method (only in handlers via IUnitOfWork)

```
Update tracker: Repository Layer is done
```

---

## Feature 5 — Custom Mediator + Pipeline

**What it builds:** Hand-rolled CQRS mediator — IRequest/ICommand/IQuery interfaces,
IRequestHandler, IPipelineBehavior, the Mediator class itself, ValidationBehavior,
LoggingBehavior. The backbone every command and query flows through.

```
Brief: Custom Mediator + Pipeline
```

Backend-only feature.

```
Build this
```

```
Review this
```

**Manual test checklist:**
- [ ] `dotnet build` passes zero warnings
- [ ] A `PingCommand` round-trip flows through both ValidationBehavior and LoggingBehavior
- [ ] An invalid request (fails validation) is blocked before reaching the handler
- [ ] Handler registration uses assembly scanning — no manual wiring in DI
- [ ] `GET /health` still returns 200

After review:

```
Write tests for this feature
```

test-agent writes unit tests for the mediator pipeline — this is worth testing properly
since everything depends on it.

```
Update tracker: Custom Mediator + Pipeline is done
```

---

## Feature 6 — App Shell + Routing

**What it builds:** The authenticated layout shell — React Router route tree, sidebar with
navigation and collapse toggle, ProtectedRoute and AdminRoute guards, empty page stubs for
all 9 inner routes.

```
Brief: App Shell + Routing
```

Frontend-only feature — no backend-agent.

```
Build this
```

```
Review this
```

**Manual test checklist:**
- [ ] All routes resolve without a console error (`/dashboard`, `/workflows`, `/templates`, etc.)
- [ ] Navigating to `/dashboard` without a token redirects to `/`
- [ ] Navigating to `/admin` as the demo user (Admin) works
- [ ] Navigating to `/admin` as a non-admin redirects to `/dashboard`
- [ ] Sidebar collapse toggle works — icon-only at 64px, full at 240px
- [ ] Active route highlighted in the sidebar
- [ ] "Sign out" in the user dropdown clears auth and routes to `/`

```
Update tracker: App Shell + Routing is done
```

---

## Feature 7 — Landing Page

**What it builds:** The public entry point — hero, features section, pricing, footer, navbar
with scroll transition. "Enter app" button calls useDevLogin and routes to the app.

```
Brief: Landing Page
```

> **Note in the brief:** Tell feature-provider the design reference is
> `web-frontend/src/design-reference/landing/index.html`

Frontend-only feature.

```
Build this
```

```
Review this
```

**Manual test checklist:**
- [ ] Page renders at `http://localhost:3000/` without console errors
- [ ] "Enter app" button calls `POST /api/auth/dev-login` and routes to `/dashboard`
- [ ] Navbar gains a dark fill on scroll past ~60px
- [ ] Three feature cards render correctly
- [ ] Pricing section renders with three tiers
- [ ] Footer renders with links
- [ ] Page is not broken on a narrow browser window (mobile not priority but must not explode)

```
Update tracker: Landing Page is done
```

---

## Feature 8 — Workflow CRUD (Templates)

**What it builds:** Full CRUD for workflow templates — 5 endpoints, 3 commands, 2 queries,
DTOs, validators on the backend. WorkflowsListPage with list, delete confirmation, and hooks
on the frontend.

```
Brief: Workflow CRUD
```

Full-stack feature — backend-agent first, then frontend-agent.

```
Build this
```

```
Review this
```

**Manual test checklist — backend (Swagger):**
- [ ] `POST /api/workflows` creates a workflow with tasks and returns full DTO
- [ ] `GET /api/workflows` returns both global templates (WorkspaceId null) and workspace workflows
- [ ] `GET /api/workflows/{id}` returns workflow with tasks
- [ ] `PUT /api/workflows/{id}` updates name and task list
- [ ] `DELETE /api/workflows/{id}` on a workspace workflow — succeeds
- [ ] `DELETE /api/workflows/{id}` on a global template — returns error (not allowed)

**Manual test checklist — frontend:**
- [ ] `/workflows` page shows global templates with a "Global" badge
- [ ] `New workflow` button routes to `/workflows/new` (stub — builder not built yet)
- [ ] Delete confirmation dialog appears, workflow disappears from list on confirm
- [ ] Skeleton loader shows during fetch

```
Write tests for this feature
```

```
Update tracker: Workflow CRUD is done
```

---

## Feature 9 — Workflow Builder UI

**What it builds:** The React Flow canvas — drag-and-drop node palette, node types with
custom styling, properties panel, auto-save, publish flow with confirmation dialog.

```
Brief: Workflow Builder UI
```

> **Note in the brief:** Design references are
> `web-frontend/src/design-reference/workflows/builder-1.html` and `builder-2.html`

Frontend-only feature (uses Feature 8 API).

```
Build this
```

```
Review this
```

**Manual test checklist:**
- [ ] Drag a Task node from the palette onto the canvas — node appears
- [ ] Drag an Approval node — renders with purple border
- [ ] Connect two nodes with an edge
- [ ] Click a node — properties panel opens with correct fields
- [ ] Edit the title in properties panel — node label updates in real time
- [ ] "Save draft" calls the API and shows "Saved" indicator
- [ ] "Publish" shows confirmation dialog — on confirm routes to `/workflows/active`
- [ ] Navigate to `/workflows/{id}/edit` — existing workflow renders with all nodes and edges
- [ ] Edit mode shows amber "This workflow is currently live" banner for published workflows
- [ ] Delete from `···` menu shows confirmation and removes workflow

```
Update tracker: Workflow Builder UI is done
```

---

## Feature 10 — Template Library

**What it builds:** Template library page with search, filter by category, sort options,
card grid, read-only preview modal, "Use" clones template into builder.

```
Brief: Template Library
```

> **Note in the brief:** Design reference is
> `web-frontend/src/design-reference/workflows/templates.html`

Frontend-only feature (uses Feature 8 API).

```
Build this
```

```
Review this
```

**Manual test checklist:**
- [ ] Three global starter templates appear on first load
- [ ] Search input filters cards in real time (client-side, no API call)
- [ ] Category filter works (Approvals, Onboarding, etc.)
- [ ] Preview modal opens with a read-only React Flow canvas
- [ ] "Use this template" in modal calls `POST /api/workflows` (clone) and opens builder pre-populated
- [ ] Global template "Delete" button is disabled with tooltip "Global templates cannot be deleted"
- [ ] Workspace template "Delete" works (delete confirmation → removed from list)

```
Update tracker: Template Library is done
```

---

## Feature 11 — WorkflowState Spawn

**What it builds:** The backend engine for launching a live workflow instance — SpawnWorkflowCommand,
WorkflowStatesController, 2 queries. Frontend spawn modal wired into the Active Workflows Board stub.

```
Brief: WorkflowState Spawn
```

Full-stack feature.

```
Build this
```

```
Review this
```

**Manual test checklist — backend (Swagger):**
- [ ] `POST /api/workflow-states` with a valid WorkflowId creates a WorkflowState
- [ ] All WorkflowTaskStates created — first task `InProgress`, rest `Pending`
- [ ] DueDate correctly calculated from `DueAtOffsetDays`
- [ ] Reference number generated in format `WF-YYYYMMDD-XXXX`
- [ ] A WorkflowAudit entry exists with `Action: "WorkflowStarted"`
- [ ] `GET /api/workflow-states` returns the new instance

**Manual test checklist — frontend:**
- [ ] "Start new workflow" button on Active Workflows Board opens the spawn modal
- [ ] Dropdown shows published workflows
- [ ] "Launch" calls the API and shows a Sonner toast with the reference number
- [ ] New instance card appears on the board (board will be a stub — just verify the modal works)

```
Write tests for this feature
```

```
Update tracker: WorkflowState Spawn is done
```

---

## Feature 12 — WorkflowTask Execution + Audit Trail

**What it builds:** The core domain actions — CompleteTask, DeclineTask, CancelWorkflow,
ReassignTask commands, GetMyTasks and GetAuditTrail queries. Frontend delivers service
and hooks only (no UI — that's Features 13–16).

```
Brief: WorkflowTask Execution + Audit Trail
```

Full-stack feature — but frontend scope is hooks/service only, not pages.

```
Build this
```

```
Review this
```

> **PR reviewer note:** Every command that mutates state must write an audit entry.
> Reviewer should check this explicitly for all four commands.

**Manual test checklist — all via Swagger:**
- [ ] `POST /api/workflow-task-states/{id}/complete` — task status → Completed, next task → InProgress
- [ ] Completing the last task — WorkflowState status → Completed
- [ ] `POST /api/workflow-task-states/{id}/decline` — task status → Declined, workflow halts at that step
- [ ] `POST /api/workflow-task-states/{id}/decline` without a reason — returns validation error
- [ ] `DELETE /api/workflow-states/{id}/cancel` — WorkflowState → Cancelled, all pending/in-progress tasks → Skipped
- [ ] `GET /api/workflow-task-states/my-tasks` — returns only tasks assigned to the authenticated user
- [ ] `GET /api/workflow-states/{id}/audit` — returns merged audit trail for the instance
- [ ] Every action above has a corresponding audit entry with actor, action, old value, new value

```
Write tests for this feature
```

```
Update tracker: WorkflowTask Execution + Audit Trail is done
```

---

## Feature 13 — My Tasks Dashboard

**What it builds:** The home screen — greeting header, stats bar with filter chips, task table
with filter bar, inline complete/decline popovers, skeleton loader, empty state.

```
Brief: My Tasks Dashboard
```

> **Note in the brief:** Design reference is
> `web-frontend/src/design-reference/dashboard/my-tasks.html`

Frontend-only feature (uses Feature 12 hooks).

```
Build this
```

```
Review this
```

**Manual test checklist:**
- [ ] `/dashboard` loads and shows tasks assigned to the demo user
- [ ] Greeting shows correct time-of-day ("Good morning/afternoon/evening, Sam")
- [ ] Stats bar shows correct counts — Due today, Overdue, In Progress, Completed this week
- [ ] Clicking a stat chip filters the task list to that category
- [ ] Overdue rows have a visible red left border
- [ ] Completed/declined rows are muted (not hidden)
- [ ] "Complete" popover opens, optional notes field works, row updates on confirm
- [ ] "Decline" popover requires a reason — cannot submit empty
- [ ] Pagination works at 25 rows per page
- [ ] Skeleton table shows during initial fetch
- [ ] Empty state shows when no tasks assigned

```
Update tracker: My Tasks Dashboard is done
```

---

## Feature 14 — Task Detail Page

**What it builds:** Full task detail view — breadcrumb, two-column layout (task content left,
workflow context + activity feed right), action buttons based on node type.

```
Brief: Task Detail Page
```

> **Note in the brief:** No Stitch design reference exists for this screen.
> Build to the spec in `docs/PHASE1-BUILD-PLAN.md` using design system tokens from
> `web-frontend/src/design-reference/DESIGN.md`.

Frontend-only feature (uses Feature 12 hooks).

```
Build this
```

```
Review this
```

**Manual test checklist:**
- [ ] Task opens from `/dashboard` row click
- [ ] Breadcrumb `← My Tasks` routes back to `/dashboard`
- [ ] Task node type: shows "Mark as complete" button + optional notes
- [ ] Approval node type: shows "Approve" (green) and "Decline" (red) buttons side by side
- [ ] Decline on approval node requires a reason
- [ ] Task assigned to someone else (not demo user, not Admin): no action buttons shown
- [ ] Completing from this page writes audit entry — verify in `/api/workflow-states/{id}/audit`
- [ ] Activity feed shows entries in reverse chronological order with relative timestamps
- [ ] Workflow context card shows correct "Step X of Y" and progress bar

```
Update tracker: Task Detail Page is done
```

---

## Feature 15 — Active Workflows Board

**What it builds:** The operations view — instance card grid, status pill tabs, spawn modal
(already built in Feature 11), cancel from overflow menu, empty state.

```
Brief: Active Workflows Board
```

> **Note in the brief:** Design reference is
> `web-frontend/src/design-reference/workflows/board.html`

Frontend-only feature (uses Feature 11 + 12 hooks).

```
Build this
```

```
Review this
```

**Manual test checklist:**
- [ ] Board shows all workflow instances with correct status badges
- [ ] Progress bar shows correct fraction ("3 of 6 tasks complete")
- [ ] Recent activity shows last 3 audit entries per card
- [ ] Status pill tab "In Progress" filters to only in-progress instances
- [ ] "Start new workflow" opens the spawn modal — new instance appears on board after launch
- [ ] `···` menu cancel → confirmation dialog → instance card updates to "Cancelled"
- [ ] Clicking a card routes to `/workflows/active/{id}` (stub page — just verify routing)
- [ ] Empty state shows when no instances exist

```
Update tracker: Active Workflows Board is done
```

---

## Feature 16 — Workflow Instance Detail

**What it builds:** Live read-only canvas with node states overlaid (pulsing teal for active,
green for completed, red for declined), workflow summary strip, full audit trail feed.

```
Brief: Workflow Instance Detail
```

Frontend-only feature (uses Feature 11 + 12 hooks).

```
Build this
```

```
Review this
```

**Manual test checklist:**
- [ ] Canvas renders the correct nodes for the workflow template
- [ ] Pending tasks: muted grey
- [ ] Active (InProgress) task: teal border with pulsing ring animation
- [ ] Completed tasks: green fill with checkmark
- [ ] Declined tasks: red fill with X
- [ ] Completing a task on My Tasks, then navigating here — node visual updates
- [ ] Workflow summary strip shows status, progress, started date
- [ ] Full audit trail shows all events with human-readable text and timestamps
- [ ] "Cancel workflow" (Admin) shows confirmation and navigates back to `/workflows/active`

```
Write tests for this feature
```

```
Update tracker: Workflow Instance Detail is done
```

---

## Phase 1 complete

When all 16 features are ticked in the tracker, Phase 1 is done. The demo shows:
landing page → enter app → workflow builder → publish → spawn → complete tasks →
watch the instance canvas update → audit trail.

Phase 2 begins with: `Brief: Email + Password Auth`
