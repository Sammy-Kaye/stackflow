# Feature Brief: Repository Layer
Phase: 1
Feature: #4
Status: Ready for implementation

---

## What this feature does (plain English)

The Repository Layer introduces the data access contract between the Application layer and PostgreSQL. Application handlers will never reference EF Core directly — they call repository interfaces defined in the Application project. The Infrastructure project provides the concrete EF Core implementations. A Unit of Work interface ensures that no repository ever saves its own changes, keeping transaction control in the handler's hands. This feature produces no API endpoints and has no frontend component.

---

## Scope — what IS in this brief

- `IWorkflowRepository` interface with methods: `GetByIdAsync`, `GetByWorkspaceAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`
- `IWorkflowTaskRepository` interface with methods: `GetByWorkflowIdAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `AddRangeAsync`
- `IWorkflowStateRepository` interface with methods: `GetByIdAsync`, `GetByWorkspaceAsync`, `GetActiveByWorkspaceAsync`, `AddAsync`, `UpdateAsync`
- `IWorkflowTaskStateRepository` interface with methods: `GetByIdAsync`, `GetByWorkflowStateIdAsync`, `GetByAssignedUserAsync`, `AddAsync`, `UpdateAsync`, `AddRangeAsync`
- `IWorkflowAuditRepository` interface with methods: `AddAsync`, `GetByWorkflowStateIdAsync`
- `IWorkflowTaskAuditRepository` interface with methods: `AddAsync`, `GetByWorkflowTaskStateIdAsync`
- `IUnitOfWork` interface with a single method: `SaveChangesAsync(CancellationToken ct)`
- One concrete implementation class per interface in `StackFlow.Infrastructure/Persistence/Repositories/`
- `UnitOfWork` implementation that delegates `SaveChangesAsync` to `AppDbContext.SaveChangesAsync`
- All implementations registered as scoped services in `Infrastructure/DependencyInjection.cs`
- All async methods accept `CancellationToken ct` as the final parameter
- Read queries use `AsNoTracking()` — entities returned from read methods are not tracked by EF
- Write methods (`AddAsync`, `UpdateAsync`, `DeleteAsync`) operate on tracked entities
- No raw SQL — EF Core LINQ only

---

## Scope — what is NOT in this brief

- No CQRS handlers, commands, or queries — those arrive in Feature 5 (Custom Mediator) and Feature 8 (Workflow CRUD)
- No API endpoints — this feature exposes nothing over HTTP
- No frontend changes
- No pagination logic — query methods return full collections; pagination is added per-feature when needed
- No soft delete — hard delete only, consistent with Feature 3
- No caching layer
- No specification pattern — plain LINQ per method
- No `IUserRepository` or `IWorkspaceRepository` — those will be added when the Auth feature (Phase 2) requires them
- No `WorkflowTask` Phase 2 attachment fields in repository queries

---

## Domain entities involved

All entities defined in Feature 3 (Domain Entities + DB):

- `Workflow` — all CRUD operations
- `WorkflowTask` — read by workflow, full CRUD plus bulk insert
- `WorkflowState` — read by workspace, read active instances, CRUD
- `WorkflowTaskState` — read by workflow state, read by assigned user, CRUD plus bulk insert
- `WorkflowAudit` — append-only write, read by workflow state
- `WorkflowTaskAudit` — append-only write, read by workflow task state

No new fields required. No new entities introduced.

---

## API Contract

None for this feature. The Repository Layer is an internal infrastructure concern. It produces no HTTP endpoints. The first endpoints appear in Feature 8 (Workflow CRUD).

---

## Frontend routes and views

None. This feature is backend-only.

---

## RabbitMQ events (if any)

None for this feature.

---

## SignalR events (if any)

None for this feature.

---

## Audit requirements

None directly. The repository layer provides the mechanism by which audit entries are written (`IWorkflowAuditRepository.AddAsync`, `IWorkflowTaskAuditRepository.AddAsync`), but no handler exists yet to call them. Audit writes begin in Feature 12 (WorkflowTask Execution + Audit Trail).

---

## Acceptance criteria

1. Given the application starts, when DI resolves `IWorkflowRepository`, then a concrete `WorkflowRepository` instance is returned without error.
2. Given the application starts, when DI resolves `IUnitOfWork`, then a concrete `UnitOfWork` instance is returned without error.
3. Given a `Workflow` entity is passed to `IWorkflowRepository.AddAsync`, when `IUnitOfWork.SaveChangesAsync` is called, then the entity is persisted to the `Workflows` table in PostgreSQL.
4. Given a workflow exists in the database, when `IWorkflowRepository.GetByIdAsync` is called with its ID, then the entity is returned with `AsNoTracking` (the EF change tracker does not hold a reference to it).
5. Given a workspace ID, when `IWorkflowRepository.GetByWorkspaceAsync` is called, then only workflows belonging to that workspace are returned.
6. Given a `WorkflowTaskState` exists assigned to a user email, when `IWorkflowTaskStateRepository.GetByAssignedUserAsync` is called with that email, then that task state is included in the result.
7. Given a list of `WorkflowTask` entities, when `IWorkflowTaskRepository.AddRangeAsync` is called, then all entities in the list are added to the EF change tracker in a single operation.
8. Given `IWorkflowAuditRepository.AddAsync` is called with an audit entry, when `IUnitOfWork.SaveChangesAsync` is called, then the audit row is present in the `WorkflowAudits` table.
9. Given any repository implementation, when a method is called without a `CancellationToken`, then the code does not compile (all async methods require `CancellationToken ct` as the final parameter).
10. Given `dotnet build` is run, then all four projects compile with zero warnings.

---

## Agent instructions

**Backend Agent:** ordered build sequence

1. Write all 7 interfaces in `Application/Common/Interfaces/Repositories/` (place `IUnitOfWork` at `Application/Common/Interfaces/` — it is not a repository)
2. Write all 7 implementations in `Infrastructure/Persistence/Repositories/`
3. Implement `UnitOfWork` in `Infrastructure/Persistence/UnitOfWork.cs`
4. Update DI registration in `Infrastructure/DependencyInjection.cs` — all as scoped
5. Run `dotnet build` from `web-api/` and fix any errors until the build is clean

**Frontend Agent:** not required for this feature.

**Handoff point:** Not applicable — this feature produces no API surface. Feature 5 (Custom Mediator + Pipeline) may begin immediately after this feature is PR-approved.
