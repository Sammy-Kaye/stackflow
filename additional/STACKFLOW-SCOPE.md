# StackFlow — Comprehensive Product Scope

> Living document. Updated as phases are completed and decisions are made.
> Owner: Samuel. Source of truth for all agents.

---

## What is StackFlow?

StackFlow is an intelligent, adaptive workflow process engine for developers and small teams. It replaces rigid task management with living, branching workflows that adapt mid-execution, support approvals, handle external contributors via token-based links, and provide full audit history of every action taken.

**Core problem:** Workflows are rigid and can't adapt mid-process. Tasks get stuck, nobody knows whose turn it is, and everything lives in emails.

**Who:** Solo project by Samuel. Claude agents are the collaborators.

**Users:** Admin (Samuel), Team Member (invited users), External contributor (token-only, no login required)

**Design principle:** The user should feel calm — everything organised, clear, never overwhelming.

---

## The three pillars

| Pillar | What it means |
|---|---|
| Adaptability | Re-assign, reorder, branch — any workflow can change mid-execution |
| Team clarity | One place to see who does what, what's stuck, and what's next |
| Full audit trail | Every action logged — who did what, when, and why |

---

## Delivery Phases

### Phase 1 — Core engine + drag & drop builder
**Target: Week 1–2**
**Status: Active**

| Feature | Status | Notes |
|---|---|---|
| Project scaffold | Not started | Solution, layers, Docker Compose |
| Domain entities | Not started | Workflow, WorkflowTask, WorkflowState, WorkflowTaskState, Audits |
| EF Core DbContext + migrations | Not started | Fluent API config, one file per entity |
| Repository interfaces + implementations | Not started | Application interfaces, Infrastructure implementations |
| Custom mediator + pipeline behaviors | Not started | ValidationBehavior → LoggingBehavior → Handler |
| Workflow CRUD (templates) | Not started | Create, read, update, delete workflow templates |
| WorkflowState spawn | Not started | Instantiate a template into a live execution instance |
| WorkflowTask execution | Not started | Assign, complete, decline tasks |
| Mid-process editing | Not started | Re-assign, reorder, add/remove tasks on live workflows |
| Audit trail | Not started | Write WorkflowAudit + WorkflowTaskAudit on every state mutation |
| React Flow drag & drop builder | Not started | Visual canvas — linear, parallel, branching flows |
| Template library UI | Not started | Browse, clone, and manage workflow templates |
| My Tasks view | Not started | Personal dashboard of all tasks assigned to current user |
| Active Workflows board | Not started | Live overview of all running workflow instances |

### Phase 2 — Auth, notifications, approvals
**Target: Week 3–4**
**Status: Not started**

| Feature | Status | Notes |
|---|---|---|
| Email + Password auth | Not started | ASP.NET Identity, hashed passwords |
| Google OAuth | Not started | @react-oauth/google + Google.Apis.Auth |
| Email OTP | Not started | 6-digit code via SMTP, 10-minute expiry |
| Password reset flow | Not started | Token-based, hashed, 1-hour expiry, anti-enumeration |
| JWT + refresh tokens | Not started | 15-min access, 7-day refresh, single-use rotation |
| Role-based route guards | Not started | ProtectedRoute, AdminRoute, GuestRoute, Public |
| SMTP email via MailKit | Not started | Free, self-hosted, no third-party service |
| RabbitMQ event consumers | Not started | All email trigger consumers |
| SignalR in-app notifications | Not started | Real-time hub per workspace |
| Approval nodes | Not started | Block progress until approved or declined with reason |
| External task tokens | Not started | Token-based completion, no login required, 7-day expiry |

### Phase 3 — Analytics, calendar, infrastructure
**Target: Week 5–6**
**Status: Not started**

| Feature | Status | Notes |
|---|---|---|
| Analytics dashboard | Not started | Bottlenecks, completion rates, team performance — Recharts |
| Calendar view | Not started | Deadlines and tasks on a timeline |
| Google Calendar sync | Not started | Push due dates to Google Calendar |
| Microsoft Outlook sync | Not started | Push due dates to Outlook via Microsoft Graph |
| Triggered/scheduled workflows | Not started | Auto-start on event or cron schedule |
| Group workspaces | Not started | Multi-team support with scoped permissions |
| Proxmox + Docker hosting | Not started | Self-hosted home lab deployment |
| Terraform infra-as-code | Not started | Proxmox VM provisioning |

---

## Domain Model

### Template level (reusable definitions)

**Workflow** — reusable template defining a process
- `Id`, `Name`, `Description`, `WorkspaceId`, `IsActive`, `CreatedAt`, `UpdatedAt`
- Contains: collection of `WorkflowTask` templates

**WorkflowTask** — template task within a workflow
- `Id`, `WorkflowId`, `Title`, `Description`
- `AssigneeType` (Internal / External)
- `DefaultAssignedToEmail`
- `OrderIndex` — task sequencing
- `DueAtOffsetDays` — relative due date from workflow start date
- `NodeType` (Task / Approval / Condition / Notification / ExternalStep / Deadline)
- `ConditionConfig` — JSON config for if/else branching logic
- `ParentTaskId` — for branching (null = top-level task)

### Instance level (live execution)

**WorkflowState** — live execution instance of a workflow
- `Id`, `WorkflowId` (FK to template), `WorkspaceId`
- `Status`: InProgress / Completed / Cancelled
- `ContextType`: Standalone / Group
- `BatchId`, `ReferenceNumber` — grouping and tracking
- `StartedAt`, `CompletedAt`, `CancelledAt`
- Contains: collection of `WorkflowTaskState` instances

**WorkflowTaskState** — live task instance
- `Id`, `WorkflowStateId`, `WorkflowTaskId` (FK to template task)
- `Status`: Pending / InProgress / Completed / Declined / Expired / Skipped
- `AssignedToEmail`, `AssignedToUserId`
- `DueDate` — calculated from template `DueAtOffsetDays`
- `CompletionToken`, `TokenExpiresAt`, `IsTokenUsed` — external completion
- `CompletionNotes`, `DeclineReason`
- `Priority`: Low / Medium / High / Critical

### Audit trail

**WorkflowAudit** — every change to a WorkflowState
**WorkflowTaskAudit** — every change to a WorkflowTaskState
- Both include: `ActorUserId`, `ActorEmail`, `Action`, `OldValue`, `NewValue`, `Timestamp`

### Core rule: Template / Instance separation
Templates are immutable blueprints. Instances are live executions. A template change never mutates a running instance — it only affects new instances spawned after the change. Never conflate the two.

---

## Workflow Node Types

| Node | Colour | Behaviour |
|---|---|---|
| Task | Default | Assigned to a user — completed or declined |
| Approval | Purple | Blocks progress until approved or declined with reason |
| Condition | Teal | If/else — branches the flow based on a configured rule |
| Deadline | Amber | Task must be completed by a calculated due date |
| Notification | Blue | Alerts someone without blocking progress |
| External step | Coral | Token-based link sent to an outside user |

---

## User Roles

| Role | Access |
|---|---|
| Admin | Full access — build templates, manage users, view audit log, workspace settings |
| Team Member | Execute assigned tasks, view own workflows, daily digest, in-app notifications |
| External | Token-only — no login, single task completion page, expiring secure links |

---

## Views & Screens

| View | Role | Description |
|---|---|---|
| Workflow builder | Admin | Drag and drop canvas — build and edit workflow templates |
| Template library | Admin + Member | Browse, clone, and manage saved templates |
| Active workflows | Admin + Member | Live board of all running workflow instances |
| My tasks | Member | Personal queue of all tasks assigned to the current user |
| Calendar view | Admin + Member | Deadlines and tasks displayed on a timeline |
| Analytics | Admin | Bottlenecks, completion rates, team performance charts |
| Audit log | Admin | Full history of every action taken on every workflow |
| Notification centre | Admin + Member | Read/unread in-app notifications |
| Admin panel | Admin | User management, role assignment, workspace settings |
| External completion | External | Token-based task completion — no login required |

---

## Authentication

| Method | Flow |
|---|---|
| Email + Password | Credentials → validate → JWT + refresh token |
| Google OAuth | Google token → Google.Apis.Auth → JWT + refresh token |
| Email OTP | Enter email → 6-digit code via SMTP → enter code → JWT + refresh token |

**Password reset:** Full token-based flow. Always returns HTTP 200 on /forgot-password to prevent user enumeration. Token hashed in DB, single-use, 1-hour expiry. Invalidates all refresh tokens on success.

**Token storage:** Redux Persist, encrypted via crypto-js.
**401 handling:** Auto-refresh → retry → logout on failure.

---

## Notifications

### Email (SMTP via MailKit — free, no third-party)

| Event | Trigger | Email |
|---|---|---|
| TaskAssignedEvent | Task assigned | "You have a new task: {title}" |
| TaskOverdueEvent | Deadline passed | "Task overdue: {title}" |
| ApprovalRequestedEvent | Approval node reached | "Approval needed: {title}" |
| WorkflowCompletedEvent | All tasks done | "Workflow completed: {name}" |
| ExternalTaskAssignedEvent | External step reached | Completion link email |
| PasswordResetRequestedEvent | Forgot password | Reset link (1hr expiry) |
| OtpRequestedEvent | OTP login | 6-digit code (10min expiry) |
| DailyDigestEvent | Cron 7am daily | Summary of all pending tasks |

### In-app (SignalR real-time)
- Hub: NotificationHub — scoped per workspace
- Events: task assigned, task overdue, approval needed, workflow status changed
- Frontend: Redux stores unread count + list. React Query invalidates on push.

---

## Tech Stack

### Backend

| Concern | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| Architecture | Clean Architecture — Domain → Application → Infrastructure → Api |
| CQRS | Custom hand-rolled mediator (no MediatR) |
| Database | PostgreSQL via EF Core 10 + Npgsql |
| Messaging | RabbitMQ — raw RabbitMQ.Client (no MassTransit) |
| Real-time | SignalR |
| Auth | ASP.NET Identity + JWT Bearer + Google.Apis.Auth |
| Email | MailKit v4 + MimeKit (SMTP — free, self-hosted) |
| Resilience | Polly |
| Docs | Swagger / Swashbuckle |
| Testing | xUnit + Moq + coverlet |

### Frontend

| Concern | Technology |
|---|---|
| Core | React 19 + TypeScript + Vite |
| Styling | TailwindCSS v4 |
| UI components | shadcn/ui (New York style) + Radix UI + Lucide React |
| Canvas | React Flow (drag & drop workflow builder) |
| Server state | TanStack React Query v5 |
| Global state | Redux Toolkit + Redux Persist |
| Forms | React Hook Form + Zod v4 |
| HTTP | Axios (single instance) |
| Real-time | @microsoft/signalr |
| Charts | Recharts |
| Notifications | Sonner (toasts) |
| Dates | date-fns |
| Auth | @react-oauth/google + input-otp |
| Testing | Vitest + React Testing Library |
| E2E | Playwright |

### Infrastructure (Proxmox home lab)

| Tool | Role |
|---|---|
| Proxmox VE | Hypervisor — hosts all VMs |
| Terraform | Provisions Proxmox VMs as code |
| Ansible | Configures VMs, installs Docker, deploys app |
| Docker Compose | Runs all services (API, frontend, PostgreSQL, RabbitMQ, Nginx) |
| Nginx | Reverse proxy + SSL termination |
| Let's Encrypt + Certbot | Free SSL certs, auto-renewal |

---

## Backend Architecture Patterns

| Pattern | Implementation |
|---|---|
| CQRS mediator | Hand-rolled IMediator, ICommand\<T\>, IQuery\<T\>, IRequestHandler |
| Pipeline | ValidationBehavior → LoggingBehavior → Handler (auto-registered open generics) |
| Error handling | Result\<T\> pattern — never throw business exceptions from handlers |
| Repository | Interfaces in Application, implementations in Infrastructure |
| Unit of Work | Explicit SaveChangesAsync(ct) in handler — never in repository |
| Controllers | Thin — zero business logic, only Mediator.Send() calls |
| Multi-tenancy | X-Workspace-Id header → WorkspaceContextMiddleware → IWorkspaceContextService |
| Global exceptions | GlobalExceptionHandlerMiddleware — NotFoundException→404, BadRequestException→400, etc. |
| Events | IEventBus → RabbitMqEventBus. IEventHandler\<T\> consumers for all async work |
| Audit | Every state mutation writes WorkflowAudit or WorkflowTaskAudit |
| EF config | Fluent API only — no data annotations on domain entities |
| Migrations | Named {YYYYMMDDHHmm}_{PascalCaseDescription} |

## Frontend Architecture Patterns

| Pattern | Implementation |
|---|---|
| Module structure | src/modules/{feature}/ — entities, dtos, enums, infrastructure, hooks, store, ui |
| Service layer | All API calls in infrastructure/{feature}-service.ts — never inline in components |
| API client | Single Axios instance in src/lib/api-client.ts — JWT + workspace header injection |
| State split | Redux for auth/UI state. React Query for all server/workflow data. Never mix. |
| Forms | React Hook Form + Zod on every form — no uncontrolled inputs |
| Loading states | Skeleton components for every async state — never empty white space |
| Errors | Sonner toast notifications for all error states |
| Dates | date-fns only — never .toLocaleDateString() |
| Destructive actions | AlertDialog confirmation required before any delete or cancel |
| Real-time | SignalR useEffect subscriptions with cleanup — no memory leaks |
| Route guards | ProtectedRoute / AdminRoute / GuestRoute / Public |

---

## Agent System

| Agent | File | Role |
|---|---|---|
| Feature Provider | AGENT-FEATURE-PROVIDER.md | Scope tracking, Feature Briefs, API contracts — never writes code |
| Backend Agent | AGENT-BACKEND.md | .NET 10 API implementation |
| Frontend Agent | AGENT-FRONTEND.md | React 19 + TypeScript UI implementation |
| PR Reviewer | AGENT-PR-REVIEWER.md | Code quality gate — nothing ships without sign-off |
| Docs Agent | AGENT-DOCS.md | Documentation from reviewed code only |
| Test Agent | AGENT-TEST.md | Unit, integration, and regression tests |
| Debug & Edit Agent | AGENT-DEBUG-EDIT.md | Debugging, fixing, refactoring, pivot coordination |

### Feature pipeline
```
Feature Provider → Brief + API Contract
       │
       ├──→ Backend Agent → builds API → completion summary
       └──→ Frontend Agent → waits for backend → builds UI
                    │
              PR Reviewer → required changes or sign-off
                    │
         ┌──────────┴──────────┐
     Docs Agent           Test Agent
     (documents)          (writes tests)
                               │
                        Real Tester (Samuel)
                        (exploratory testing)
                               │
                 bugs → Debug & Edit Agent → fixes → back through PR
                               │
                   Feature Provider (mark done)
```

---

## Key Decisions

| Decision | Rationale |
|---|---|
| Custom mediator, not MediatR | Hand-rolled — full understanding, no black-box dependency, documented in CLAUDE.md |
| Result pattern, not exceptions | Explicit error handling — no hidden control flow, predictable return types |
| PostgreSQL only | One database engine — no SQL Server for this project |
| MailKit + SMTP, not Postmark/SendGrid | Free, self-controlled, no paid plan needed |
| Email + Password + Google OAuth + OTP | Three auth methods — password reset required for email path |
| Password reset implemented | Email + Password auth requires account recovery |
| React Flow for canvas | Best-in-class node-based editor for React |
| Redux for auth/UI only, React Query for data | Clean separation — no stale server data in Redux |
| RabbitMQ raw client, no MassTransit | Lean and consistent with reference codebase |
| Self-hosted Proxmox, not Azure | Free, full control, learning infrastructure as code |
| Template / Instance separation | Templates = blueprints. Instances = live executions. Never conflate. |
| Solo project | Samuel builds this alone. Claude agents are the only collaborators. |

---

*Last updated: Project setup — pre-build*
