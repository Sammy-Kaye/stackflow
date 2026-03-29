# Repository Layer

> Last updated: 2026-03-29
> Phase: 1
> Status: Complete — PR approved

---

## What it does

The Repository Layer establishes the data access contract between the Application layer (handlers, commands, queries) and the database. Handlers never reference EF Core directly. Instead, they call interfaces defined in the Application project (`IWorkflowRepository`, `IWorkflowStateRepository`, etc.), which are implemented in the Infrastructure project using EF Core. A Unit of Work interface ensures that transaction control stays in the handler's hands — repositories never commit changes themselves. This design keeps the codebase layered, testable, and swappable.

---

## How it works

When a handler needs to persist data, it follows this pattern:

1. Inject repository interfaces (from Application layer)
2. Call repository methods to add, update, or delete entities — they go into EF Core's change tracker but are not persisted
3. Call `IUnitOfWork.SaveChangesAsync()` once at the end of the handler
4. All pending changes are committed to PostgreSQL in a single database round-trip

For reads, repositories use `AsNoTracking()` so entities are not held in EF's change tracker. For writes, entities are tracked, allowing multiple repository operations in one handler to participate in the same implicit EF Core transaction. If the handler hits an error after writing to a repository but before calling `SaveChangesAsync`, nothing is persisted — the transaction is automatically rolled back.

Example flow:
- `CreateWorkflowHandler` receives a command
- Calls `_workflowRepo.AddAsync(workflow, ct)` — adds to tracker, not persisted
- Calls `_auditRepo.AddAsync(auditEntry, ct)` — adds to tracker, not persisted
- Calls `_uow.SaveChangesAsync(ct)` — both writes committed atomically to PostgreSQL

---

## Key files

| File | Purpose |
|---|---|
| `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowRepository.cs` | Interface: contract for Workflow CRUD |
| `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowTaskRepository.cs` | Interface: contract for WorkflowTask CRUD |
| `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowStateRepository.cs` | Interface: contract for WorkflowState CRUD |
| `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowTaskStateRepository.cs` | Interface: contract for WorkflowTaskState CRUD |
| `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowAuditRepository.cs` | Interface: contract for WorkflowAudit writes and reads |
| `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowTaskAuditRepository.cs` | Interface: contract for WorkflowTaskAudit writes and reads |
| `web-api/src/StackFlow.Application/Common/Interfaces/IUnitOfWork.cs` | Interface: transaction control point for all handlers |
| `web-api/src/StackFlow.Infrastructure/Persistence/Repositories/WorkflowRepository.cs` | EF Core implementation of IWorkflowRepository |
| `web-api/src/StackFlow.Infrastructure/Persistence/Repositories/WorkflowTaskRepository.cs` | EF Core implementation of IWorkflowTaskRepository |
| `web-api/src/StackFlow.Infrastructure/Persistence/Repositories/WorkflowStateRepository.cs` | EF Core implementation of IWorkflowStateRepository |
| `web-api/src/StackFlow.Infrastructure/Persistence/Repositories/WorkflowTaskStateRepository.cs` | EF Core implementation of IWorkflowTaskStateRepository |
| `web-api/src/StackFlow.Infrastructure/Persistence/Repositories/WorkflowAuditRepository.cs` | EF Core implementation of IWorkflowAuditRepository |
| `web-api/src/StackFlow.Infrastructure/Persistence/Repositories/WorkflowTaskAuditRepository.cs` | EF Core implementation of IWorkflowTaskAuditRepository |
| `web-api/src/StackFlow.Infrastructure/Persistence/UnitOfWork.cs` | Wraps AppDbContext.SaveChangesAsync behind IUnitOfWork |
| `web-api/src/StackFlow.Infrastructure/DependencyInjection.cs` | DI registration of all repositories and Unit of Work as scoped services |

---

## Database changes

No new tables or migrations. This feature adds no columns to existing tables. All six domain entities (Workflow, WorkflowTask, WorkflowState, WorkflowTaskState, WorkflowAudit, WorkflowTaskAudit) were defined in Feature 3 (Domain Entities + DB) and their tables already exist in the database. The Repository Layer is purely a code-layer abstraction over those tables.

---

## Events

No RabbitMQ events in this feature.

---

## Real-time (SignalR)

No SignalR events in this feature.

---

## Known limitations or caveats

1. **No pagination.** Repository query methods return full collections. Pagination is added per-feature when needed (e.g., when listing workflows on a dashboard). Handlers that need paged results will call a repository query, then paginate in the handler if needed, or a dedicated paginated query method will be added to the repository at that time.

2. **No caching.** Repositories hit PostgreSQL every time. Caching is added if profiling shows it's necessary.

3. **No soft delete.** `DeleteAsync` performs hard deletes. Soft deletes are not implemented in this phase.

4. **Audit repositories are append-only.** `IWorkflowAuditRepository` and `IWorkflowTaskAuditRepository` have no `UpdateAsync` or `DeleteAsync` methods — audit entries are immutable history. This is by design.

5. **No specification pattern.** Repositories use plain LINQ `Where` clauses. No generic `ISpecification<T>` abstraction. Queries are explicit and readable.

6. **IUserRepository does not exist yet.** When the Auth feature arrives (Phase 2), `IUserRepository` and `IWorkspaceRepository` will be added for managing users and workspaces. For now, the focus is on workflow and task data access.

---

## Architectural decisions

**Why repositories at all?** They decouple handlers from EF Core, making handlers testable (mock the repository), and making it easy to swap the database implementation without rewriting handlers. They are also the contract — other layers know exactly what data access is available without reading EF configuration.

**Why a separate Unit of Work interface instead of a SaveChanges method on each repository?** Unit of Work consolidates transaction control in one place. If a handler needs to call three repositories and then commit once, all three repositories are working with the same `AppDbContext` and the same transaction. If each repository had its own `SaveChanges()`, there would be three separate database round-trips and three separate transactions — breaking atomicity. By forcing handlers to call `SaveChangesAsync()` explicitly via a single `IUnitOfWork`, we guarantee that all pending changes within a handler are committed together or rolled back together.

**Why AsNoTracking on reads?** Entities returned from queries are not being mutated in the same request, so there's no point in EF tracking them. `AsNoTracking()` reduces memory overhead and GC pressure. It also prevents accidental mutations of query results from being persisted.

**Why is email matching case-insensitive?** Email addresses are case-insensitive by spec (RFC 5321). `GetByAssignedUserAsync` uses PostgreSQL's `ILIKE` function to push the case-insensitive comparison to the database instead of pulling all records in-process and filtering with `.ToLower()`. This is more efficient at scale.

**Why order results in the repository instead of the handler?** Consistent ordering prevents handlers from accidentally displaying task lists out of sequence. `GetByWorkflowIdAsync` orders by `OrderIndex` (the workflow step position), and audit methods order by `Timestamp` (chronological). Handlers can override this ordering if needed, but the default is correct.

**Why scoped lifetime for repositories?** Scoped means one instance per HTTP request. All repositories within the same request share the same `AppDbContext` instance, so all their changes are part of the same implicit transaction. This is the only way to guarantee atomicity across multiple repository calls.

---

## Notes: Brief vs implementation

Implementation matches the Feature Brief exactly.

All method signatures, parameter names, return types, async patterns, cancellation tokens, and ordering logic are as specified. No deviations from the brief.
