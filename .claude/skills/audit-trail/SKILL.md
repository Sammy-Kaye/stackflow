---
name: audit-trail
description: >
  Enforce StackFlow audit trail requirements. Loaded once by backend-agent before
  building any state-mutating handler. Do not auto-load — load explicitly when
  the feature brief includes WorkflowState or WorkflowTaskState mutations.
allowed-tools: Read, Write, Edit
---

<!--
  WHY THIS SKILL EXISTS
  ──────────────────────
  The audit trail is the permanent, tamper-evident history of everything
  that happened to every workflow. It cannot be reconstructed after the fact.
  A missed audit entry today means a permanent gap in history.

  This is not just a compliance feature — it is the mechanism that lets
  Samuel (and any future team member) answer the question:
  "What happened to this workflow, who did it, and when?"

  The audit trail also supports the Lego principle: if the workflow engine
  itself is ever replaced or refactored, the audit history survives because
  it is written to durable, append-only tables that belong to the data, not
  the implementation.

  IF CLAUDE CODE CEASES TO EXIST:
  A developer following this skill can implement every audit entry correctly
  by reading the patterns and rules below. The audit tables and entry format
  are fully specified — no guesswork required.
-->

# StackFlow — Audit Trail

---

## The Core Rule

```
Every command that mutates WorkflowState MUST write a WorkflowAudit entry.
Every command that mutates WorkflowTaskState MUST write a WorkflowTaskAudit entry.
Both the mutation AND the audit entry are saved in ONE SaveChangesAsync call.
```

There are no exceptions to this rule. If a mutation happens without an audit entry,
the history is broken and cannot be repaired.

---

## When to Write Which Audit Entry

| You are mutating... | You must write... |
|---|---|
| `WorkflowState.Status` | `WorkflowAudit` |
| `WorkflowState.CompletedAt` | `WorkflowAudit` |
| `WorkflowState.CancelledAt` | `WorkflowAudit` |
| Any field on `WorkflowState` | `WorkflowAudit` |
| `WorkflowTaskState.Status` | `WorkflowTaskAudit` |
| `WorkflowTaskState.AssignedToEmail` | `WorkflowTaskAudit` |
| `WorkflowTaskState.AssignedToUserId` | `WorkflowTaskAudit` |
| `WorkflowTaskState.Priority` | `WorkflowTaskAudit` |
| `WorkflowTaskState.DueDate` | `WorkflowTaskAudit` |
| Any field on `WorkflowTaskState` | `WorkflowTaskAudit` |

---

## WorkflowAudit Entry — Full Pattern

```csharp
// STEP 1: Capture the old value BEFORE making the mutation
var oldStatus = workflowState.Status.ToString();

// STEP 2: Apply the mutation
workflowState.Status = WorkflowStatus.Cancelled;
workflowState.CancelledAt = DateTime.UtcNow;

// STEP 3: Build the audit entry
var audit = new WorkflowAudit
{
    Id = Guid.NewGuid(),
    WorkflowStateId = workflowState.Id,
    ActorUserId = _currentUser.UserId,       // Guid? — null for system-triggered actions
    ActorEmail = _currentUser.Email,          // string — always required
    Action = "WorkflowCancelled",             // Human-readable past-tense verb
    OldValue = oldStatus,                     // What it was before
    NewValue = workflowState.Status.ToString(), // What it is now
    Timestamp = DateTime.UtcNow              // Always UTC
};

// STEP 4: Persist BOTH the mutation and the audit in ONE call
await _workflowAuditRepo.AddAsync(audit, ct);
await _uow.SaveChangesAsync(ct);  // One transaction — both or neither
```

---

## WorkflowTaskAudit Entry — Full Pattern

```csharp
// STEP 1: Capture old values BEFORE mutation
var oldStatus = taskState.Status.ToString();

// STEP 2: Apply the mutation
taskState.Status = TaskStatus.Completed;
taskState.CompletionNotes = command.Notes;

// STEP 3: Build the audit entry
var audit = new WorkflowTaskAudit
{
    Id = Guid.NewGuid(),
    WorkflowTaskStateId = taskState.Id,
    ActorUserId = _currentUser.UserId,
    ActorEmail = _currentUser.Email,
    Action = "TaskCompleted",
    OldValue = oldStatus,
    NewValue = taskState.Status.ToString(),
    Timestamp = DateTime.UtcNow
};

// STEP 4: One save — mutation + audit together
await _taskAuditRepo.AddAsync(audit, ct);
await _uow.SaveChangesAsync(ct);
```

---

## Action Name Conventions

Use consistent, human-readable past-tense action names. These appear directly in
the audit log UI and must be meaningful to a non-developer reading the history.

**WorkflowAudit actions:**
```
WorkflowStarted       — WorkflowState created and execution began
WorkflowCompleted     — all tasks done, status → Completed
WorkflowCancelled     — terminated before completion
WorkflowEdited        — mid-process change applied (task added/removed/reordered)
```

**WorkflowTaskAudit actions:**
```
TaskAssigned          — AssignedToEmail / AssignedToUserId set or changed
TaskStarted           — status changed to InProgress
TaskCompleted         — status changed to Completed
TaskDeclined          — status changed to Declined
TaskExpired           — status changed to Expired (system-triggered by deadline)
TaskSkipped           — bypassed by a Condition node
TaskPriorityChanged   — Priority field updated
TaskDeadlineChanged   — DueDate updated during mid-process edit
TaskReassigned        — assigned to a different person after initial assignment
ExternalTokenIssued   — CompletionToken generated for external contributor
ExternalTokenUsed     — external contributor completed via token link
```

If you need an action not in this list, use the same pattern:
- Past tense
- PascalCase
- Plain English — no abbreviations

---

## Multiple Mutations in One Handler

When a handler mutates multiple states, write one audit entry per entity mutated.
All entries go into the same `SaveChangesAsync` call.

```csharp
// Example: completing a task AND advancing the workflow to Completed
var oldTaskStatus = taskState.Status.ToString();
var oldWorkflowStatus = workflowState.Status.ToString();

// Mutations
taskState.Status = TaskStatus.Completed;
workflowState.Status = WorkflowStatus.Completed;
workflowState.CompletedAt = DateTime.UtcNow;

// Audit entries — one per mutated entity
var taskAudit = new WorkflowTaskAudit
{
    Id = Guid.NewGuid(),
    WorkflowTaskStateId = taskState.Id,
    ActorUserId = _currentUser.UserId,
    ActorEmail = _currentUser.Email,
    Action = "TaskCompleted",
    OldValue = oldTaskStatus,
    NewValue = taskState.Status.ToString(),
    Timestamp = DateTime.UtcNow
};

var workflowAudit = new WorkflowAudit
{
    Id = Guid.NewGuid(),
    WorkflowStateId = workflowState.Id,
    ActorUserId = _currentUser.UserId,
    ActorEmail = _currentUser.Email,
    Action = "WorkflowCompleted",
    OldValue = oldWorkflowStatus,
    NewValue = workflowState.Status.ToString(),
    Timestamp = DateTime.UtcNow
};

// All four writes in one transaction
await _taskAuditRepo.AddAsync(taskAudit, ct);
await _workflowAuditRepo.AddAsync(workflowAudit, ct);
await _uow.SaveChangesAsync(ct);
```

---

## System-Triggered Actions (No Human Actor)

Some actions are triggered by the system — deadlines expiring, automated transitions.
For these, `ActorUserId` is null and `ActorEmail` uses a system identifier.

```csharp
var audit = new WorkflowTaskAudit
{
    Id = Guid.NewGuid(),
    WorkflowTaskStateId = taskState.Id,
    ActorUserId = null,                   // No human actor
    ActorEmail = "system@stackflow.app",  // Consistent system identifier
    Action = "TaskExpired",
    OldValue = oldStatus,
    NewValue = TaskStatus.Expired.ToString(),
    Timestamp = DateTime.UtcNow
};
```

---

## Testing Audit Entries

Every handler test for a state mutation must verify the audit entry was written.
Testing "it worked" is not enough — "it worked AND it was audited" is the standard.

```csharp
[Fact]
public async Task Handle_CompletesTask_WritesAuditEntry()
{
    // Arrange
    var auditRepoMock = new Mock<IWorkflowTaskAuditRepository>();
    // ... set up handler with audit repo mock

    // Act
    var result = await _sut.Handle(command, CancellationToken.None);

    // Assert — the mutation succeeded
    Assert.True(result.IsSuccess);

    // Assert — the audit entry was written with the correct fields
    auditRepoMock.Verify(
        a => a.AddAsync(
            It.Is<WorkflowTaskAudit>(audit =>
                audit.Action == "TaskCompleted" &&
                audit.WorkflowTaskStateId == command.TaskStateId &&
                audit.ActorEmail == _currentUser.Email &&
                audit.NewValue == TaskStatus.Completed.ToString()),
            It.IsAny<CancellationToken>()),
        Times.Once);

    // Assert — one save call covered both mutation and audit
    _uowMock.Verify(
        u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
        Times.Once);
}
```

---

## What You Must Never Do

- Mutate `WorkflowState` or `WorkflowTaskState` without writing an audit entry
- Write the audit entry in a separate `SaveChangesAsync` call — it must be atomic
- Capture `OldValue` after the mutation — always capture it before changing the entity
- Use vague action names like `"Updated"` or `"Changed"` — be specific: `"TaskReassigned"`
- Omit `ActorEmail` — it is always required, even for system-triggered actions
- Write the audit entry conditionally — if the mutation happens, the audit entry happens
