# Domain Entities + DB

> Last updated: 2026-03-29
> Phase: 1
> Status: Complete — PR approved

---

## What it does

This feature establishes the complete domain model and database schema for StackFlow.
It defines all eight core entities as pure C# classes with no framework dependencies,
configures them for PostgreSQL via EF Core Fluent API, and seeds three workflow templates
(Employee Onboarding, Purchase Approval, Client Offboarding) so the application has
meaningful data on first run. There are no HTTP endpoints in this feature — it is the
data foundation that every subsequent feature builds on.

---

## How it works

The domain entities in `StackFlow.Domain/Models/` are plain C# classes. They carry no
EF Core attributes or package references — their only imports are other domain types.
EF Core configuration for each entity lives in a matching `IEntityTypeConfiguration<T>`
class in `StackFlow.Infrastructure/Persistence/Configurations/`. `AppDbContext` picks
these up automatically via `ApplyConfigurationsFromAssembly` — no manual registration
is needed when a new configuration file is added. Seed data is applied directly in
`AppDbContext.OnModelCreating` via `HasData`, using fixed Guids and a fixed timestamp
(`2026-01-01T00:00:00Z`) so the migration is deterministic and idempotent. The
infrastructure layer is wired into the DI container by calling
`builder.Services.AddInfrastructure(builder.Configuration)` in `Program.cs`, which
reads the connection string from `ConnectionStrings:DefaultConnection` and throws a
clear error at startup if it is absent.

---

## Key files

| File | Purpose |
|---|---|
| `web-api/src/StackFlow.Domain/Models/Workspace.cs` | Root container entity |
| `web-api/src/StackFlow.Domain/Models/User.cs` | Workspace member entity |
| `web-api/src/StackFlow.Domain/Models/Workflow.cs` | Workflow template entity |
| `web-api/src/StackFlow.Domain/Models/WorkflowTask.cs` | Template task step entity; self-referencing FK for condition branches |
| `web-api/src/StackFlow.Domain/Models/WorkflowState.cs` | Live workflow instance entity |
| `web-api/src/StackFlow.Domain/Models/WorkflowTaskState.cs` | Live task step instance entity; holds token fields for Phase 2 |
| `web-api/src/StackFlow.Domain/Models/WorkflowAudit.cs` | Immutable audit record for workflow instance changes |
| `web-api/src/StackFlow.Domain/Models/WorkflowTaskAudit.cs` | Immutable audit record for task state changes |
| `web-api/src/StackFlow.Domain/Enums/UserRole.cs` | Admin, Member |
| `web-api/src/StackFlow.Domain/Enums/AssigneeType.cs` | Internal, External |
| `web-api/src/StackFlow.Domain/Enums/NodeType.cs` | Task, Approval, Condition, Notification, ExternalStep, Deadline |
| `web-api/src/StackFlow.Domain/Enums/WorkflowStatus.cs` | InProgress, Completed, Cancelled |
| `web-api/src/StackFlow.Domain/Enums/ContextType.cs` | Standalone, Group |
| `web-api/src/StackFlow.Domain/Enums/WorkflowTaskStatus.cs` | Pending, InProgress, Completed, Declined, Expired, Skipped |
| `web-api/src/StackFlow.Domain/Enums/Priority.cs` | Low, Medium, High, Critical |
| `web-api/src/StackFlow.Domain/Constants/WellKnownIds.cs` | Fixed Guids for DemoWorkspaceId and GlobalWorkspaceId |
| `web-api/src/StackFlow.Infrastructure/Persistence/AppDbContext.cs` | EF DbContext; assembly-scanned configuration; seed data |
| `web-api/src/StackFlow.Infrastructure/Persistence/Configurations/WorkspaceConfiguration.cs` | Fluent API config for Workspaces table |
| `web-api/src/StackFlow.Infrastructure/Persistence/Configurations/UserConfiguration.cs` | Fluent API config for Users table |
| `web-api/src/StackFlow.Infrastructure/Persistence/Configurations/WorkflowConfiguration.cs` | Fluent API config for Workflows table |
| `web-api/src/StackFlow.Infrastructure/Persistence/Configurations/WorkflowTaskConfiguration.cs` | Fluent API config for WorkflowTasks table; cascade and self-ref FK |
| `web-api/src/StackFlow.Infrastructure/Persistence/Configurations/WorkflowStateConfiguration.cs` | Fluent API config for WorkflowStates table |
| `web-api/src/StackFlow.Infrastructure/Persistence/Configurations/WorkflowTaskStateConfiguration.cs` | Fluent API config for WorkflowTaskStates table; cascade FK |
| `web-api/src/StackFlow.Infrastructure/Persistence/Configurations/WorkflowAuditConfiguration.cs` | Fluent API config for WorkflowAudits table |
| `web-api/src/StackFlow.Infrastructure/Persistence/Configurations/WorkflowTaskAuditConfiguration.cs` | Fluent API config for WorkflowTaskAudits table |
| `web-api/src/StackFlow.Infrastructure/DependencyInjection.cs` | AddInfrastructure extension method; registers AppDbContext |
| `web-api/src/StackFlow.Api/Program.cs` | Calls AddInfrastructure |

---

## Database changes

Migration name: `20260329163132_202603_InitialCreate`
Location: `web-api/src/StackFlow.Infrastructure/Migrations/`

Tables created:

- `Workspaces`
- `Users`
- `Workflows`
- `WorkflowTasks`
- `WorkflowStates`
- `WorkflowTaskStates`
- `WorkflowAudits`
- `WorkflowTaskAudits`

All tables use `uuid` primary keys. All `DateTime` columns use `timestamp with time zone`.
All enum columns are stored as `integer`. FK indexes are created automatically by EF Core
for all foreign key columns.

The migration also inserts the seed rows described in the API reference document:
2 workspace rows, 3 workflow template rows, and 15 workflow task rows.

---

## Events

No RabbitMQ events in this feature.

---

## Real-time (SignalR)

No SignalR events in this feature.

---

## Known limitations or caveats

- `User` is a minimal Phase 1 shell. Password hash, OTP, Google OAuth token, and
  refresh token fields are intentionally absent and will be added as new columns in
  a Phase 2 migration. Do not add auth fields to this entity until that feature is built.

- `WorkflowTaskState.CompletionToken` is present but will be null for all rows until
  Phase 2 implements external token issuance. The column is `varchar(500)` to accommodate
  the hashed value that Phase 2 will store.

- All 15 seed tasks have `AssigneeType = Internal` and `DefaultAssignedToEmail = null`.
  The assignee email is intended to be resolved at workflow spawn time (Feature 11), not
  pre-filled in the template.

- The `ConditionConfig` column exists on `WorkflowTask` but no seed rows use it. It is
  present so the schema does not need a migration when Condition node functionality is built.

---

## Notes: Brief vs implementation

Implementation matches the Feature Brief.
