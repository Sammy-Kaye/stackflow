# Repository Layer API Reference

> Last updated: 2026-03-29
> Feature status: Approved — PR reviewed
> Related files: `web-api/src/StackFlow.Infrastructure/Persistence/Repositories/`

---

## IWorkflowRepository

Data access contract for Workflow template entities.

**File:** `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowRepository.cs`

### GetByIdAsync

Retrieves a single Workflow by ID.

**Signature:**
```csharp
Task<Workflow?> GetByIdAsync(Guid id, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `id` | `Guid` | The workflow ID to retrieve |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task<Workflow?>` — The workflow if found, null otherwise.

**Behavior:** Uses `AsNoTracking()` — returned entity is not tracked by EF's change tracker.

---

### GetByWorkspaceAsync

Retrieves all Workflows in a specific workspace.

**Signature:**
```csharp
Task<IReadOnlyList<Workflow>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `workspaceId` | `Guid` | The workspace ID |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task<IReadOnlyList<Workflow>>` — All workflows in the workspace, ordered by Name ascending.

**Behavior:** Uses `AsNoTracking()`. Empty list if no workflows exist in the workspace.

---

### AddAsync

Adds a new Workflow to the change tracker.

**Signature:**
```csharp
Task AddAsync(Workflow workflow, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `workflow` | `Workflow` | The workflow entity to add |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task` — Completes when the entity is added to the tracker.

**Behavior:** Does not call `SaveChangesAsync`. The handler must call `IUnitOfWork.SaveChangesAsync` to persist.

---

### UpdateAsync

Marks an existing Workflow as modified in the change tracker.

**Signature:**
```csharp
Task UpdateAsync(Workflow workflow, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `workflow` | `Workflow` | The workflow entity with updated values |
| `ct` | `CancellationToken` | Cancellation token (not used) |

**Returns:** `Task` — Completes synchronously after marking the entity as modified.

**Behavior:** Does not call `SaveChangesAsync`. Transaction control belongs to the handler.

---

### DeleteAsync

Removes a Workflow from the change tracker.

**Signature:**
```csharp
Task DeleteAsync(Workflow workflow, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `workflow` | `Workflow` | The workflow entity to delete |
| `ct` | `CancellationToken` | Cancellation token (not used) |

**Returns:** `Task` — Completes synchronously after marking the entity for deletion.

**Behavior:** Does not call `SaveChangesAsync`. Transaction control belongs to the handler.

---

## IWorkflowTaskRepository

Data access contract for WorkflowTask template step entities.

**File:** `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowTaskRepository.cs`

### GetByWorkflowIdAsync

Retrieves all tasks belonging to a workflow.

**Signature:**
```csharp
Task<IReadOnlyList<WorkflowTask>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `workflowId` | `Guid` | The workflow ID |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task<IReadOnlyList<WorkflowTask>>` — All tasks in the workflow, ordered by OrderIndex ascending.

**Behavior:** Uses `AsNoTracking()`. Results are in execution sequence. Empty list if no tasks exist.

---

### AddAsync

Adds a single WorkflowTask to the change tracker.

**Signature:**
```csharp
Task AddAsync(WorkflowTask task, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `task` | `WorkflowTask` | The task entity to add |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task` — Completes when the entity is added to the tracker.

**Behavior:** Does not call `SaveChangesAsync`. Use `IUnitOfWork.SaveChangesAsync` to persist.

---

### UpdateAsync

Marks an existing WorkflowTask as modified in the change tracker.

**Signature:**
```csharp
Task UpdateAsync(WorkflowTask task, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `task` | `WorkflowTask` | The task entity with updated values |
| `ct` | `CancellationToken` | Cancellation token (not used) |

**Returns:** `Task` — Completes synchronously after marking the entity as modified.

**Behavior:** Does not call `SaveChangesAsync`. Transaction control belongs to the handler.

---

### DeleteAsync

Removes a WorkflowTask from the change tracker.

**Signature:**
```csharp
Task DeleteAsync(WorkflowTask task, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `task` | `WorkflowTask` | The task entity to delete |
| `ct` | `CancellationToken` | Cancellation token (not used) |

**Returns:** `Task` — Completes synchronously after marking the entity for deletion.

**Behavior:** Does not call `SaveChangesAsync`. Transaction control belongs to the handler.

---

### AddRangeAsync

Adds multiple WorkflowTasks to the change tracker in a single operation.

**Signature:**
```csharp
Task AddRangeAsync(IEnumerable<WorkflowTask> tasks, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `tasks` | `IEnumerable<WorkflowTask>` | Collection of task entities to add |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task` — Completes when all entities are added to the tracker.

**Behavior:** More efficient than calling `AddAsync` in a loop. Used when creating a workflow with multiple tasks. Does not call `SaveChangesAsync`.

---

## IWorkflowStateRepository

Data access contract for WorkflowState instance entities.

**File:** `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowStateRepository.cs`

### GetByIdAsync

Retrieves a single WorkflowState by ID.

**Signature:**
```csharp
Task<WorkflowState?> GetByIdAsync(Guid id, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `id` | `Guid` | The workflow state ID |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task<WorkflowState?>` — The workflow state if found, null otherwise.

**Behavior:** Uses `AsNoTracking()`.

---

### GetByWorkspaceAsync

Retrieves all WorkflowStates in a workspace (all statuses).

**Signature:**
```csharp
Task<IReadOnlyList<WorkflowState>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `workspaceId` | `Guid` | The workspace ID |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task<IReadOnlyList<WorkflowState>>` — All workflow states in the workspace, ordered by StartedAt descending.

**Behavior:** Uses `AsNoTracking()`. Includes completed, cancelled, and in-progress instances. Empty list if none exist.

---

### GetActiveByWorkspaceAsync

Retrieves only the in-progress WorkflowStates in a workspace.

**Signature:**
```csharp
Task<IReadOnlyList<WorkflowState>> GetActiveByWorkspaceAsync(Guid workspaceId, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `workspaceId` | `Guid` | The workspace ID |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task<IReadOnlyList<WorkflowState>>` — Only workflow states with `Status == WorkflowStatus.InProgress`, ordered by StartedAt descending.

**Behavior:** Uses `AsNoTracking()`. Filter is applied in the database via WHERE clause. Empty list if no active instances exist.

---

### AddAsync

Adds a new WorkflowState to the change tracker.

**Signature:**
```csharp
Task AddAsync(WorkflowState state, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `state` | `WorkflowState` | The workflow state entity to add |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task` — Completes when the entity is added to the tracker.

**Behavior:** Does not call `SaveChangesAsync`.

---

### UpdateAsync

Marks an existing WorkflowState as modified in the change tracker.

**Signature:**
```csharp
Task UpdateAsync(WorkflowState state, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `state` | `WorkflowState` | The workflow state entity with updated values |
| `ct` | `CancellationToken` | Cancellation token (not used) |

**Returns:** `Task` — Completes synchronously after marking the entity as modified.

**Behavior:** Does not call `SaveChangesAsync`.

---

## IWorkflowTaskStateRepository

Data access contract for WorkflowTaskState instance entities.

**File:** `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowTaskStateRepository.cs`

### GetByIdAsync

Retrieves a single WorkflowTaskState by ID.

**Signature:**
```csharp
Task<WorkflowTaskState?> GetByIdAsync(Guid id, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `id` | `Guid` | The task state ID |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task<WorkflowTaskState?>` — The task state if found, null otherwise.

**Behavior:** Uses `AsNoTracking()`.

---

### GetByWorkflowStateIdAsync

Retrieves all task states belonging to a workflow instance.

**Signature:**
```csharp
Task<IReadOnlyList<WorkflowTaskState>> GetByWorkflowStateIdAsync(Guid workflowStateId, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `workflowStateId` | `Guid` | The workflow state (instance) ID |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task<IReadOnlyList<WorkflowTaskState>>` — All task states in the workflow instance.

**Behavior:** Uses `AsNoTracking()`. Empty list if no tasks exist in the instance.

---

### GetByAssignedUserAsync

Retrieves all task states assigned to a specific email address across all workflows.

**Signature:**
```csharp
Task<IReadOnlyList<WorkflowTaskState>> GetByAssignedUserAsync(string email, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `email` | `string` | The assignee email address |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task<IReadOnlyList<WorkflowTaskState>>` — All task states assigned to the given email across all workflow instances.

**Behavior:** Uses `AsNoTracking()`. Email comparison is case-insensitive via PostgreSQL's `ILIKE` function. Empty list if no tasks are assigned.

---

### AddAsync

Adds a single WorkflowTaskState to the change tracker.

**Signature:**
```csharp
Task AddAsync(WorkflowTaskState taskState, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `taskState` | `WorkflowTaskState` | The task state entity to add |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task` — Completes when the entity is added to the tracker.

**Behavior:** Does not call `SaveChangesAsync`.

---

### UpdateAsync

Marks an existing WorkflowTaskState as modified in the change tracker.

**Signature:**
```csharp
Task UpdateAsync(WorkflowTaskState taskState, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `taskState` | `WorkflowTaskState` | The task state entity with updated values |
| `ct` | `CancellationToken` | Cancellation token (not used) |

**Returns:** `Task` — Completes synchronously after marking the entity as modified.

**Behavior:** Does not call `SaveChangesAsync`.

---

### AddRangeAsync

Adds multiple WorkflowTaskStates to the change tracker in a single operation.

**Signature:**
```csharp
Task AddRangeAsync(IEnumerable<WorkflowTaskState> taskStates, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `taskStates` | `IEnumerable<WorkflowTaskState>` | Collection of task state entities to add |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task` — Completes when all entities are added to the tracker.

**Behavior:** More efficient than calling `AddAsync` in a loop. Used when spawning a workflow instance with multiple task states. Does not call `SaveChangesAsync`.

---

## IWorkflowAuditRepository

Data access contract for WorkflowAudit entries. Audit records are append-only.

**File:** `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowAuditRepository.cs`

### AddAsync

Adds a new WorkflowAudit entry to the change tracker.

**Signature:**
```csharp
Task AddAsync(WorkflowAudit audit, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `audit` | `WorkflowAudit` | The audit entry to record |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task` — Completes when the entry is added to the tracker.

**Behavior:** Does not call `SaveChangesAsync`. Write the audit entry in the same `SaveChangesAsync` call as the mutation it records.

---

### GetByWorkflowStateIdAsync

Retrieves all audit entries for a workflow instance.

**Signature:**
```csharp
Task<IReadOnlyList<WorkflowAudit>> GetByWorkflowStateIdAsync(Guid workflowStateId, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `workflowStateId` | `Guid` | The workflow state (instance) ID |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task<IReadOnlyList<WorkflowAudit>>` — All audit entries for the workflow instance, ordered by Timestamp ascending.

**Behavior:** Uses `AsNoTracking()`. Results are in chronological order. Empty list if no audits exist.

---

## IWorkflowTaskAuditRepository

Data access contract for WorkflowTaskAudit entries. Audit records are append-only.

**File:** `web-api/src/StackFlow.Application/Common/Interfaces/Repositories/IWorkflowTaskAuditRepository.cs`

### AddAsync

Adds a new WorkflowTaskAudit entry to the change tracker.

**Signature:**
```csharp
Task AddAsync(WorkflowTaskAudit audit, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `audit` | `WorkflowTaskAudit` | The audit entry to record |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task` — Completes when the entry is added to the tracker.

**Behavior:** Does not call `SaveChangesAsync`. Write the audit entry in the same `SaveChangesAsync` call as the mutation it records.

---

### GetByWorkflowTaskStateIdAsync

Retrieves all audit entries for a task state.

**Signature:**
```csharp
Task<IReadOnlyList<WorkflowTaskAudit>> GetByWorkflowTaskStateIdAsync(Guid workflowTaskStateId, CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `workflowTaskStateId` | `Guid` | The task state ID |
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task<IReadOnlyList<WorkflowTaskAudit>>` — All audit entries for the task state, ordered by Timestamp ascending.

**Behavior:** Uses `AsNoTracking()`. Results are in chronological order. Empty list if no audits exist.

---

## IUnitOfWork

Transaction control interface. The single point where handlers commit database changes.

**File:** `web-api/src/StackFlow.Application/Common/Interfaces/IUnitOfWork.cs`

### SaveChangesAsync

Commits all pending changes in the EF Core change tracker to the database in a single round-trip.

**Signature:**
```csharp
Task SaveChangesAsync(CancellationToken ct)
```

**Parameters:**
| Parameter | Type | Description |
|---|---|---|
| `ct` | `CancellationToken` | Cancellation token |

**Returns:** `Task` — Completes when all changes are persisted.

**Behavior:** All repository operations within a single handler share the same scoped `AppDbContext` and participate in an implicit EF Core transaction. Multiple repository calls (e.g., adding a workflow and adding an audit entry) are committed atomically.

**Usage pattern:**
```csharp
// Handler pseudocode
await _workflowRepo.AddAsync(workflow, ct);
await _auditRepo.AddAsync(auditEntry, ct);
await _uow.SaveChangesAsync(ct);  // both writes committed together, or both rolled back on error
```

---

## Dependency Injection Registration

All repositories and the Unit of Work are registered as **scoped** services in `DependencyInjection.cs`:

```csharp
services.AddScoped<IWorkflowRepository, WorkflowRepository>();
services.AddScoped<IWorkflowTaskRepository, WorkflowTaskRepository>();
services.AddScoped<IWorkflowStateRepository, WorkflowStateRepository>();
services.AddScoped<IWorkflowTaskStateRepository, WorkflowTaskStateRepository>();
services.AddScoped<IWorkflowAuditRepository, WorkflowAuditRepository>();
services.AddScoped<IWorkflowTaskAuditRepository, WorkflowTaskAuditRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

**Scoped lifetime:** One instance per HTTP request. All repositories within the same request share the same `AppDbContext` instance, ensuring they participate in the same implicit transaction.

---

## Implementation Notes

- **No SaveChangesAsync in repositories.** Write methods (`AddAsync`, `UpdateAsync`, `DeleteAsync`, `AddRangeAsync`) do not commit changes. This keeps the handler in control of the transaction boundary.

- **AsNoTracking on reads.** All query methods use `AsNoTracking()` to reduce memory overhead for data that will not be mutated in the same request.

- **Database filters in queries.** Filters like "active workflows only" and "case-insensitive email matching" are applied in the database via LINQ, not in-process.

- **No raw SQL.** All queries use EF Core LINQ. Raw SQL is never used in repository methods.

- **CancellationToken required.** All async methods accept `CancellationToken ct` as the final parameter, enabling request cancellation and graceful shutdown.
