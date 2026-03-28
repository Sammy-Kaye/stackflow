# StackFlow — CLAUDE.md

> **Read this file in full before every session.**
> This is the master project bible. Every pattern, rule, and decision here is intentional
> and must be followed consistently. If an agent output contradicts this file, this file wins.
> If something is unclear, ask Samuel — do not improvise.

---

## ⚡ The Golden Rules (Read These First)

1. **CLAUDE.md is always the source of truth.** If a Feature Brief, agent output, or old
   document contradicts this file — this file wins.

2. **Human readability above all.** Every file, every function, every agent output must be
   written as if Claude Code ceases to exist tomorrow. A human developer with no prior context
   must be able to open any file, read it, understand it, and continue working without asking
   anyone. No magic. No black boxes.

3. **Lego architecture.** Every layer and component is independently swappable. Domain entities
   don't know about EF. Handlers don't know about HTTP. Frontend components don't know about
   APIs. If a piece needs to be replaced, the surrounding pieces don't break.

4. **Samuel is the only human.** He controls what moves forward. No feature is done, no tracker
   updated, no CLAUDE.md changed without his explicit approval.

5. **One thing at a time.** One feature per build session. One bug per debug session. Focus is
   a feature, not a constraint.

---

## 🗺️ What Is StackFlow?

StackFlow is an **intelligent, adaptive workflow process engine**. It replaces rigid task
management with living, branching workflows that can be edited mid-execution, support
approvals, handle external contributors via token-based links, and provide full audit history
of every action taken.

**Core problem it solves:** Workflows are rigid and can't adapt mid-process. Tasks get stuck,
nobody knows whose turn it is, and everything lives in emails.

**Who built it:** Solo project by Samuel. No team — Claude agents are the collaborators.

**Primary users:**
- **Admin** (Samuel) — creates and manages workflows and workspaces
- **Team Member** — assigned tasks, can view their workflows
- **External Contributor** — token-only access, no login required, completes assigned tasks via link

**Design principle:** The user should feel calm — everything organised, clear, and never
overwhelming.

---

## 🤖 Agent System

StackFlow is built by Claude Code orchestrating 7 sub-agents. Claude Code reads this file and
automatically delegates work to the right agent. Samuel is the checkpoint between every stage.

### Sub-Agent Directory

All agent files live in `.claude/agents/`. Claude Code discovers them automatically.

| Agent | File | Role | Tools |
|---|---|---|---|
| **feature-provider** | `.claude/agents/feature-provider.md` | Scope controller. Produces Feature Briefs and API Contracts. Never writes code. | Read, Write |
| **backend-agent** | `.claude/agents/backend-agent.md` | Builds .NET 9 API from Feature Brief. Domain → Application → Infrastructure → API. | Read, Write, Edit, Bash |
| **frontend-agent** | `.claude/agents/frontend-agent.md` | Builds React 19 UI from Feature Brief. Types → Service → Hooks → Components → Pages. | Read, Write, Edit, Bash |
| **pr-reviewer** | `.claude/agents/pr-reviewer.md` | Quality gate. Reviews completed features. Nothing moves forward without sign-off. | Read, Glob, Grep |
| **docs-agent** | `.claude/agents/docs-agent.md` | Documents PR-approved features only. Produces API reference + feature summary. | Read, Write |
| **test-agent** | `.claude/agents/test-agent.md` | Writes unit, integration, and regression tests for approved features. | Read, Write, Edit, Bash |
| **debug-edit-agent** | `.claude/agents/debug-edit-agent.md` | Debugging, fixes, refactors, pivots. Floats across the entire system. Always sequential. | Read, Write, Edit, Bash, Glob, Grep |

### Dispatch Rules

**Parallel dispatch** — Claude Code can run these agents simultaneously when ALL conditions are met:
- Tasks span independent domains (e.g. backend + frontend on the same feature)
- No shared files or state between the tasks
- Clear file boundary — zero overlap

**Sequential dispatch** — Claude Code must run agents one after another when ANY applies:
- Tasks have dependencies (B needs A's output)
- Shared files or state — risk of merge conflict
- Unclear scope — needs understanding before proceeding
- **debug-edit-agent always runs sequentially** — it touches everything

**Background dispatch** — Claude Code can run as a background task when:
- Research or analysis only — no file modifications
- Results are not blocking current work

### Agent Constraints

- Sub-agents **cannot spawn other sub-agents** — delegation is one level deep only
- Sub-agents **cannot communicate with each other** — all coordination flows through Claude Code
- Context isolation is the point — each agent gets only what it needs, keeping the main thread clean
- Results are compressed — only the summary returns to Claude Code, not the full reasoning chain

### The Feature Build Flow

```
Samuel: "Brief: {feature name}"
        ↓
[feature-provider]
Produces: Feature Brief + API Contract
        ↓
Samuel reviews and says: "Build this"
        ↓
[backend-agent]  ←─── runs first (or in parallel if frontend can wait)
[frontend-agent] ←─── waits for backend signal before implementing
        ↓
Both complete → Samuel: "Review this"
        ↓
[pr-reviewer]
Produces: Review report
        ↓
┌── Changes required? → back to backend-agent / frontend-agent
└── Approved?
        ↓
[docs-agent]  ←── runs in parallel
[test-agent]  ←── runs in parallel
        ↓
Samuel tests manually (Real Tester)
        ↓
┌── Bug found? → [debug-edit-agent] → [test-agent] (regression) → [pr-reviewer] (if >3 files)
└── All good? → Samuel: "Update tracker: {feature} is done"
        ↓
[feature-provider] updates phase tracker
```

---

## 📁 Project Structure

```
D:\My Projects\Stackflow\
│
├── CLAUDE.md                        ← YOU ARE HERE — read before every session
├── STACKFLOWSCOPE.md                ← Living scope document
├── docker-compose.yml               ← PostgreSQL + RabbitMQ + API + Frontend
├── .env.example                     ← All required env vars documented here
│
├── .claude/
│   ├── agents/                      ← Sub-agent instruction files
│   │   ├── feature-provider.md
│   │   ├── backend-agent.md
│   │   ├── frontend-agent.md
│   │   ├── pr-reviewer.md
│   │   ├── docs-agent.md
│   │   ├── test-agent.md
│   │   └── debug-edit-agent.md
│   └── skills/                      ← Custom skill files (patterns + templates)
│       ├── stackflow-domain/
│       ├── ef-migration/
│       ├── result-pattern/
│       ├── audit-trail/
│       ├── feature-brief-writer/
│       ├── pr-checklist/
│       ├── stackflow-design/
│       └── e2e-testing/
│
├── web-api/                         ← .NET 9 Backend
│   ├── src/
│   │   ├── StackFlow.Domain/        ← Entities, enums. Pure C#. Zero dependencies.
│   │   ├── StackFlow.Application/   ← Commands, queries, handlers, DTOs, validators
│   │   ├── StackFlow.Infrastructure/← EF Core, repositories, RabbitMQ, email
│   │   └── StackFlow.Api/           ← Controllers, middleware, DI, Program.cs
│   └── tests/
│       ├── StackFlow.UnitTests/     ← Handler + validator unit tests
│       └── StackFlow.IntegrationTests/ ← Endpoint tests
│
├── web-frontend/                    ← React 19 + TypeScript Frontend
│   └── src/
│       ├── modules/                 ← Feature modules (one folder per feature domain)
│       │   ├── {feature}/
│       │   │   ├── entities/        ←   TypeScript domain interfaces
│       │   │   ├── dtos/            ←   API contract types
│       │   │   ├── enums/           ←   TypeScript enums (must match backend)
│       │   │   ├── infrastructure/  ←   Service layer — all API calls live here
│       │   │   ├── hooks/           ←   React Query hooks
│       │   │   └── ui/
│       │   │       ├── components/  ←   Reusable components
│       │   │       └── pages/       ←   Route-level page components
│       │   └── shared/              ←   Cross-feature utilities
│       ├── store/                   ← Redux — auth + persistent UI state ONLY
│       ├── router/                  ← Route definitions + route guards
│       └── design-reference/        ← READ-ONLY Stitch design exports
│
├── docs/
│   ├── api/                         ← API reference docs (one per feature)
│   └── features/                    ← Feature summary docs (one per feature)
│
└── infra/
    ├── terraform/                   ← Proxmox VM provisioning
    └── ansible/                     ← OS config, Docker install, app deploy
```

---

## 🧱 Domain Model

**The fundamental rule:** Templates are immutable blueprints. Instances are live executions.
**Never conflate template data with instance data.**

### Template Level (reusable definitions)

**Workflow** — the template blueprint
```
Fields: Id, Name, Description, WorkspaceId, IsActive, CreatedAt, UpdatedAt
Contains: collection of WorkflowTask templates
```

**WorkflowTask** — one step in the blueprint
```
Fields: Id, WorkflowId, Title, Description, AssigneeType, DefaultAssignedToEmail,
        OrderIndex, DueAtOffsetDays, NodeType, ConditionConfig, ParentTaskId,
        AttachmentKey, AttachmentFilename, AttachmentContentType, AttachmentSizeBytes
Enums:
  AssigneeType: Internal | External
  NodeType:     Task | Approval | Condition | Notification | ExternalStep | Deadline
```

### Instance Level (live execution)

**WorkflowState** — a live running instance of a Workflow template
```
Fields: Id, WorkflowId (FK → Workflow), WorkspaceId, Status, ContextType, BatchId,
        ReferenceNumber, StartedAt, CompletedAt, CancelledAt
Enums:
  Status:      InProgress | Completed | Cancelled
  ContextType: Standalone | Group
```

**WorkflowTaskState** — a live instance of one WorkflowTask step
```
Fields: Id, WorkflowStateId (FK), WorkflowTaskId (FK), Status, AssignedToEmail,
        AssignedToUserId, DueDate, CompletionToken, TokenExpiresAt, IsTokenUsed,
        CompletionNotes, DeclineReason, Priority,
        ResponseAttachmentKey, ResponseAttachmentFilename, ResponseAttachmentContentType,
        ResponseAttachmentSizeBytes, ResponseAttachmentVersion
Enums:
  Status:   Pending | InProgress | Completed | Declined | Expired | Skipped
  Priority: Low | Medium | High | Critical
```

### Audit Trail

**WorkflowAudit** — tracks every change to a WorkflowState
```
Fields: Id, WorkflowStateId, ActorUserId, ActorEmail, Action, OldValue, NewValue, Timestamp
```

**WorkflowTaskAudit** — tracks every change to a WorkflowTaskState
```
Fields: Id, WorkflowTaskStateId, ActorUserId, ActorEmail, Action, OldValue, NewValue, Timestamp
```

**Audit rule:** Every command that mutates a WorkflowState or WorkflowTaskState MUST write
an audit entry. This is non-negotiable. The audit trail is the complete history of everything
that happened to a workflow.

---

## ⚙️ Backend Architecture

### Layer Rules (Clean Architecture)

Dependencies flow inward. Outer layers know about inner layers. Inner layers never know about outer.

```
Domain ← Application ← Infrastructure ← API
```

| Layer | Package | What it contains | What it must NOT import |
|---|---|---|---|
| Domain | `StackFlow.Domain` | Entities, enums. Pure C#. | Everything external |
| Application | `StackFlow.Application` | Commands, queries, handlers, DTOs, validators, interfaces | EF Core, Infrastructure, API |
| Infrastructure | `StackFlow.Infrastructure` | EF Core, repositories, RabbitMQ, email | API layer |
| API | `StackFlow.Api` | Controllers, middleware, DI registration | Domain (directly) |

### Custom Mediator (CQRS-lite) — NEVER use MediatR

The full mediator implementation lives in `StackFlow.Application/Abstractions/Messaging/`.

```csharp
// Request markers — type information only, no logic
public interface IRequest<TResponse> { }
public interface ICommand<TResponse> : IRequest<TResponse> { }  // write operations
public interface IQuery<TResponse> : IRequest<TResponse> { }    // read operations

// Handler contract — exactly one handler per request type
public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}

// Pipeline behavior — middleware wrapping each handler
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct,
        RequestHandlerDelegate<TResponse> next);
}
```

**Pipeline execution order:**
```
Request → ValidationBehavior → LoggingBehavior → Handler → Response
```

ValidationBehavior runs first and short-circuits if validation fails. Nothing reaches the handler with invalid input.

**Handler registration:** Assembly scanning only. Never manually register handlers in DI.

### Result Pattern — Never throw business exceptions

```csharp
// ✅ CORRECT — explicit, readable, predictable
if (workflow is null)
    return Result.Fail("Workflow not found");

// ❌ WRONG — hidden control flow, breaks the pipeline
throw new NotFoundException("Workflow not found");
```

`Result<T>` is the return type for all handlers. `HandleResult()` in `BaseApiController`
maps it to the correct HTTP status code automatically.

### Thin Controllers — Zero Business Logic

```csharp
// ✅ CORRECT — one line per endpoint
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateWorkflowCommand command)
    => HandleResult(await Mediator.Send(command));

// ❌ WRONG — business logic in a controller
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateWorkflowCommand command)
{
    if (string.IsNullOrEmpty(command.Name)) return BadRequest("Name required");
    // ... more logic ...
}
```

### Repository + Unit of Work

```csharp
// Repository interfaces: Application/Common/Interfaces/
// Repository implementations: Infrastructure/Persistence/Repositories/

// ✅ CORRECT — explicit save in handler via Unit of Work
await _repo.AddAsync(entity, ct);
await _uow.SaveChangesAsync(ct);

// ❌ WRONG — repository saving itself
public async Task AddAsync(Entity e, CancellationToken ct)
{
    await _context.Set<Entity>().AddAsync(e, ct);
    await _context.SaveChangesAsync(ct);  // NEVER
}
```

### EF Core — Fluent API Only

No data annotations on domain entities. All configuration in `Infrastructure/Configurations/`,
one file per entity, implementing `IEntityTypeConfiguration<T>`.

```csharp
// ✅ CORRECT
public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
    }
}

// ❌ WRONG — annotation on domain entity
public class Workflow
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }
}
```

### RabbitMQ — Always Publish After Save

```csharp
// ✅ CORRECT — DB committed first, then message sent
await _uow.SaveChangesAsync(ct);
await _eventBus.PublishAsync(new TaskAssignedEvent(...));

// ❌ WRONG — message could be sent even if DB write fails
await _eventBus.PublishAsync(new TaskAssignedEvent(...));
await _uow.SaveChangesAsync(ct);
```

### Security Rules

- No sensitive data (passwords, tokens, OTP codes) in log output
- Password reset tokens: `Guid.NewGuid()`, stored hashed, expire 1 hour, single-use
- OTP codes: 6 random digits, stored hashed, expire 10 minutes
- External task tokens: `Guid.NewGuid()`, stored hashed, expire 7 days
- `/forgot-password` always returns HTTP 200 regardless of email existence (prevents enumeration)
- No hardcoded secrets or connection strings anywhere in code

### Backend Coding Standards

- All IDs are `Guid` — returned as strings in API responses
- All dates are `DateTime` (UTC internally) — returned as ISO 8601 strings in API responses
- Repository methods always accept `CancellationToken ct` as the last parameter
- All due dates calculated from `DueAtOffsetDays` — never hardcode a date
- Handlers are registered via assembly scanning — never manually wire them up

---

## 🖥️ Frontend Architecture

### State Management — Strict Two-Layer Rule

| State type | Store | Example |
|---|---|---|
| Auth tokens, workspace ID, current user role | Redux | `accessToken`, `userId`, `workspaceId` |
| All server data | React Query | workflows, tasks, users |
| Persistent UI state | Redux | sidebar open/closed |
| Transient UI state | `useState` | modal open, form dirty |

```typescript
// ✅ CORRECT
dispatch(setAccessToken(token));            // auth → Redux
const { data } = useWorkflows();           // server data → React Query

// ❌ WRONG — never cross these boundaries
dispatch(setWorkflows(apiResponse));       // server data must NOT go to Redux
useQuery({ queryFn: getAuthToken });       // auth must NOT go to React Query
```

### Service Layer — All API Calls Here

`apiClient` is the single Axios instance. It never appears in components or hooks.
All API calls live in `modules/{feature}/infrastructure/{feature}-service.ts`.

```typescript
// ✅ CORRECT
export const workflowService = {
  create: (dto: CreateWorkflowDto) => apiClient.post<WorkflowDto>('/workflows', dto),
  getById: (id: string) => apiClient.get<WorkflowDto>(`/workflows/${id}`),
};

// ❌ WRONG — never call apiClient directly in a component
function WorkflowCard() {
  const onClick = () => apiClient.post('/workflows', data);  // NEVER
}
```

### React Query — Query Key Convention

```typescript
// Centralise query keys — consistent invalidation across the app
export const workflowKeys = {
  all: ['workflows'] as const,
  byId: (id: string) => ['workflows', id] as const,
};
```

### Forms — React Hook Form + Zod Always

No `useState` for form values. No uncontrolled inputs. Every form uses:
```typescript
const form = useForm<FormValues>({ resolver: zodResolver(schema) });
```

### Frontend UX Standards

- Loading skeletons for all async states — no empty white space while loading
- Error states handled via Sonner toast notifications + graceful component fallback
- Destructive actions (delete, cancel) require a confirmation `AlertDialog`
- All dates formatted via `date-fns` — never `toLocaleDateString()`
- Password fields always have show/hide toggle

### SignalR — Subscribe in Hooks, Clean Up Always

```typescript
useEffect(() => {
  signalrClient.on('TaskStatusChanged', handler);
  return () => signalrClient.off('TaskStatusChanged', handler);  // always clean up
}, [queryClient]);
```

### Design Reference

Stitch design exports live in `web-frontend/src/design-reference/` and are **read-only**.
When a `DESIGN.md` exists for a screen, the Frontend Agent builds to match it.
The `.html` archive files are reference only — never copy-paste their HTML.
shadcn/ui New York style is the component library for all production components.

### Frontend Naming Conventions

| Type | Backend | Frontend |
|---|---|---|
| Commands | `CreateWorkflowCommand` | — |
| Queries | `GetWorkflowByIdQuery` | — |
| DTOs | `WorkflowDto`, `CreateWorkflowDto` | Same |
| Services | `WorkflowService` (class) | `workflowService` (camelCase const) |
| Hooks | — | `useWorkflow`, `useCreateWorkflow` |
| Events | `TaskAssignedEvent` | — |
| Pages | — | `WorkflowBuilderPage` |
| Components | — | `WorkflowCard`, `TaskStatusBadge` |

---

## 🗄️ Database & Migrations

### EF Core Rules

- Fluent API configuration only — no data annotations on entities
- One configuration file per entity in `Infrastructure/Configurations/`
- Use `HasQueryFilter` for soft deletes where applicable
- Migration naming: `{YYYYMMDDHHmm}_{PascalCaseDescription}`
- Always read the generated SQL before accepting any migration

### Adding a New Migration

```bash
# Run from web-api/ directory
dotnet ef migrations add {MigrationName} \
  --project src/StackFlow.Infrastructure \
  --startup-project src/StackFlow.Api

# Apply the migration
dotnet ef database update \
  --project src/StackFlow.Infrastructure \
  --startup-project src/StackFlow.Api
```

---

## 🐳 Local Development (Docker Compose)

```yaml
# docker-compose.yml — run from project root
# docker compose up -d

# Services and ports:
# API:          http://localhost:5000
# Swagger:      http://localhost:5000/swagger
# Frontend:     http://localhost:3000
# PostgreSQL:   localhost:5432
# RabbitMQ UI:  http://localhost:15672  (guest / guest)
```

```
POSTGRES_DB:       stackflow
POSTGRES_USER:     stackflow
POSTGRES_PASSWORD: stackflow_dev
```

---

## 🏗️ Infrastructure (Proxmox Home Lab)

| Tool | Role |
|---|---|
| Proxmox VE | Hypervisor — hosts all VMs |
| Terraform | Provisions Proxmox VMs as code (`infra/terraform/`) |
| Ansible | Configures VMs, installs Docker, deploys app (`infra/ansible/`) |
| Docker Compose | Runs all services on the VM |
| Nginx | Reverse proxy, SSL termination |
| Let's Encrypt + Certbot | Free SSL certs, auto-renewal |

```bash
terraform apply                    # create VM on Proxmox
ansible-playbook site.yml          # provision OS, install Docker, deploy app
docker compose up -d               # start all services
```

---

## 📦 Tech Stack

### Backend

| Concern | Technology |
|---|---|
| Runtime | .NET 9 / ASP.NET Core 9 |
| Architecture | Clean Architecture — Domain → Application → Infrastructure → API |
| CQRS | Custom hand-rolled mediator (no MediatR) |
| Database | PostgreSQL via EF Core 9 + Npgsql |
| ORM config | Fluent API only — no data annotations |
| Error handling | Result pattern — never throw business exceptions |
| Validation | FluentValidation — runs in ValidationBehavior pipeline step |
| Messaging | RabbitMQ — raw client, no MassTransit |
| Email | MailKit + SMTP — Brevo as provider (free tier, swap via appsettings.json) |
| File storage | Cloudflare R2 (prod) + MinIO (local dev) — S3-compatible, same AWSSDK.S3 code |
| Real-time | SignalR |
| Auth | JWT + refresh tokens, Google OAuth, Email OTP, Email+Password |
| Testing | xUnit + Moq (unit), WebApplicationFactory (integration) |

### Frontend

| Concern | Technology |
|---|---|
| Framework | React 19 + TypeScript |
| Build tool | Vite |
| Components | shadcn/ui — New York style |
| Workflow canvas | React Flow |
| Server state | React Query (TanStack Query) |
| Client state | Redux Toolkit (auth + UI only) |
| Forms | React Hook Form + Zod |
| Charts | Recharts (Phase 3) |
| Dates | date-fns |
| Toasts | Sonner |
| Testing | Vitest + React Testing Library |

---

## 🚦 Build Phases

### Phase 1 — Core engine + drag & drop builder (Active)

| Feature | Status |
|---|---|
| Project scaffold | Not started |
| Domain entities | Not started |
| EF Core DbContext + migrations | Not started |
| Repository interfaces + implementations | Not started |
| Custom mediator + pipeline behaviors | Not started |
| Workflow CRUD (templates) | Not started |
| WorkflowState spawn | Not started |
| WorkflowTask execution | Not started |
| Mid-process editing | Not started |
| Audit trail | Not started |
| React Flow builder UI | Not started |
| Template library UI | Not started |
| My Tasks view | Not started |
| Active Workflows board | Not started |

### Phase 2 — Auth, notifications, approvals, file attachments (Not started)

Auth (Email+Password, Google OAuth, OTP, Password Reset, JWT, refresh tokens),
SMTP email via MailKit, RabbitMQ event consumers, SignalR in-app notifications,
Approval nodes, External task tokens.

**Task file attachments:**
- Workflow builder: attach a single file to any Task or External Step node at build time
- Task detail: download the attached file; upload a response file
- External task page: same — download, upload response, no login required
- Storage: one attachment file + one response file per task node; max 10 MB each; allowed formats: PDF, DOCX, XLSX, PNG, JPG
- File stored in object storage (S3-compatible); file reference (bucket key, original filename, content type, size) stored on the `WorkflowTask` record
- Folder structure: `workspaces/{workspaceId}/workflows/{workflowId}/tasks/{taskId}/{version}/{filename}`
- Versioning: each upload increments a version counter on the task record — previous versions are retained in storage but only the latest is surfaced in the UI
- No CDN — files are served via signed URLs generated on demand by the API (15 minute expiry)

### Phase 3 — Analytics, calendar, infrastructure, group workspaces (Not started)

Analytics dashboard (Recharts), Calendar view, Google/Outlook calendar sync,
Triggered/scheduled workflows, Group workspaces,
Proxmox hosting + Terraform + Ansible.

---

## 🔑 Key Decisions

| Decision | Rationale |
|---|---|
| Custom mediator, not MediatR | Hand-rolled — full understanding, no black-box dependency |
| Result pattern, not exceptions | Explicit error handling — no hidden control flow |
| PostgreSQL only | One database engine — no SQL Server for this project |
| MailKit + SMTP, not Postmark/SendGrid | Free, self-controlled, no paid plan needed |
| Brevo for SMTP provider | Free tier (300 emails/day), reliable deliverability, MailKit compatible — swap via appsettings.json if needed |
| Cloudflare R2 for file storage (prod), MinIO for local dev | R2: free tier, zero egress fees, S3-compatible. MinIO: runs in Docker Compose, identical API — same `AWSSDK.S3` package and service code for both, endpoint/credentials swapped via env vars |
| Single file per task node, 10 MB cap, allowlist of formats | Keeps storage predictable; prevents abuse on external-facing upload endpoints |
| Email+Password + Google OAuth + OTP | Three auth methods — covers all user preferences |
| Password reset implemented | Email+Password requires account recovery — cannot skip |
| React Flow for canvas | Best-in-class node-based editor for React |
| Redux for auth/UI only, React Query for server data | Clean separation — no stale server data in Redux |
| RabbitMQ raw client, no MassTransit | Lean, consistent with reference codebase |
| Self-hosted Proxmox, not Azure | Free, full control, learning infrastructure as code |
| Template / Instance separation | Templates = immutable blueprints. Instances = live executions. Never conflate. |
| Solo project | Samuel builds this alone. Claude agents are the collaborators. |

---

## 📋 In-App Notifications (SignalR)

- Hub: `NotificationHub` — connection scoped per workspace
- Events pushed to frontend: task assigned, task overdue, approval needed, workflow status changed
- Frontend: Redux slice stores unread count + notification list
- React Query invalidates on SignalR push to keep workflow/task data fresh

---

## 📧 Email Triggers (RabbitMQ → MailKit)

| Event | Trigger | Email sent |
|---|---|---|
| `TaskAssignedEvent` | Task assigned | "You have a new task: {title}" |
| `TaskOverdueEvent` | Deadline passed | "Task overdue: {title}" |
| `ApprovalRequestedEvent` | Approval node reached | "Approval needed: {title}" |
| `WorkflowCompletedEvent` | All tasks done | "Workflow completed: {name}" |
| `ExternalTaskAssignedEvent` | External step reached | Completion link email |
| `PasswordResetRequestedEvent` | Forgot password | Reset link (1hr expiry) |
| `OtpRequestedEvent` | OTP login | 6-digit code (10min expiry) |
| `DailyDigestEvent` | Cron 7am daily | Summary of all pending tasks |

---

## ✅ How to Update This File

Only Samuel updates CLAUDE.md. Agents may propose additions via the Docs Agent's
"CLAUDE.md update proposal" section, or via the Debug & Edit Agent's violation detection.
Samuel reviews the proposal and applies it himself if approved.

**Never apply a CLAUDE.md change mid-feature.** Changes take effect at the start of the
next feature session. Mid-session changes create inconsistency between agents.
