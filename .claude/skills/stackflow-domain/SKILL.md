---
name: stackflow-domain
description: >
  Load the StackFlow domain model. Loaded once at session start by
  backend-agent, frontend-agent, and debug-edit-agent per their Context Budget.
  Do not auto-load on every entity mention — load explicitly and once per session.
---

<!--
  WHY THIS SKILL EXISTS
  ──────────────────────
  The domain model is the foundation everything else is built on.
  Without it, agents invent field names, confuse template entities with
  instance entities, and produce code that doesn't match the real schema.

  This skill loads the authoritative entity definitions so every agent
  — backend, frontend, test, docs, debug — works from the same ground truth.

  THE ONE RULE ABOVE ALL OTHERS:
  Templates are immutable blueprints. Instances are live executions.
  A change to a Workflow template NEVER modifies a running WorkflowState.
  Never conflate these two levels. Never mix their fields.
-->

# StackFlow Domain Model

---

## The Two Levels

```
TEMPLATE LEVEL          INSTANCE LEVEL
(what you design)       (what runs)
─────────────────       ──────────────────────
Workflow           →    WorkflowState
WorkflowTask       →    WorkflowTaskState
```

Templates are blueprints. They are reusable and immutable once in use.
Instances are live executions created from a template at a point in time.

---

## Template Level

### Workflow
The reusable blueprint for a process.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| Name | string | Max 200 chars. Required. |
| Description | string | Max 1000 chars. Optional. |
| WorkspaceId | Guid | Workspace this workflow belongs to |
| IsActive | bool | Whether the template is available for use |
| CreatedAt | DateTime | UTC |
| UpdatedAt | DateTime | UTC |
| Tasks | ICollection\<WorkflowTask\> | Navigation — ordered steps |

### WorkflowTask
One step inside a Workflow template.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| WorkflowId | Guid | FK → Workflow |
| Title | string | Required |
| Description | string | Optional |
| AssigneeType | AssigneeType | Internal \| External |
| DefaultAssignedToEmail | string? | Pre-fill default assignee |
| OrderIndex | int | Display and execution order |
| DueAtOffsetDays | int? | Days from workflow start — never hardcode a date |
| NodeType | NodeType | See enum below |
| ConditionConfig | string? | JSON config for Condition nodes |
| ParentTaskId | Guid? | For branching — parent node |

**AssigneeType enum**
```
Internal — assigned to a workspace member (login required)
External — assigned via token link (no login required)
```

**NodeType enum**
```
Task          — standard task, assigned to a person
Approval      — requires explicit approve/reject
Condition     — branches flow based on logic
Notification  — sends a message, no action required
ExternalStep  — token-based completion by external party
Deadline      — time-based trigger node
```

---

## Instance Level

### WorkflowState
A live running execution of a Workflow template.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| WorkflowId | Guid | FK → Workflow (the template it was spawned from) |
| WorkspaceId | Guid | Workspace scope |
| Status | WorkflowStatus | See enum below |
| ContextType | ContextType | Standalone \| Group |
| BatchId | Guid? | Groups related instances (Group context) |
| ReferenceNumber | string? | Human-readable identifier |
| StartedAt | DateTime | UTC — when execution began |
| CompletedAt | DateTime? | UTC — null until complete |
| CancelledAt | DateTime? | UTC — null unless cancelled |

**WorkflowStatus enum**
```
InProgress  — actively running
Completed   — all tasks done
Cancelled   — terminated before completion
```

**ContextType enum**
```
Standalone  — single workflow execution
Group       — one of many instances in a batch
```

### WorkflowTaskState
A live execution of one WorkflowTask step.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| WorkflowStateId | Guid | FK → WorkflowState |
| WorkflowTaskId | Guid | FK → WorkflowTask (the template step) |
| Status | TaskStatus | See enum below |
| AssignedToEmail | string? | Current assignee email |
| AssignedToUserId | Guid? | Current assignee user ID (null for external) |
| DueDate | DateTime? | Calculated from DueAtOffsetDays at spawn time |
| CompletionToken | string? | Hashed token for external completion |
| TokenExpiresAt | DateTime? | Token expiry — 7 days from issuance |
| IsTokenUsed | bool | Prevents token reuse |
| CompletionNotes | string? | Notes submitted at completion |
| DeclineReason | string? | Reason provided when declining |
| Priority | Priority | Low \| Medium \| High \| Critical |

**TaskStatus enum**
```
Pending     — not yet started
InProgress  — assignee has opened/acknowledged
Completed   — successfully finished
Declined    — assignee declined the task
Expired     — deadline passed without completion
Skipped     — bypassed by a Condition node
```

**Priority enum**
```
Low | Medium | High | Critical
```

---

## Audit Trail

Every mutation to WorkflowState or WorkflowTaskState MUST produce an audit entry.
This is non-negotiable. The audit trail is the complete, permanent history of
everything that happened to any workflow. It cannot be reconstructed after the fact.

### WorkflowAudit
Tracks every change to a WorkflowState.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| WorkflowStateId | Guid | FK → WorkflowState |
| ActorUserId | Guid? | User who triggered the action (null for system) |
| ActorEmail | string | Email of the actor |
| Action | string | Human-readable: "WorkflowStarted", "WorkflowCancelled" |
| OldValue | string? | Serialised previous state |
| NewValue | string? | Serialised new state |
| Timestamp | DateTime | UTC — when the change occurred |

### WorkflowTaskAudit
Tracks every change to a WorkflowTaskState.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | Primary key |
| WorkflowTaskStateId | Guid | FK → WorkflowTaskState |
| ActorUserId | Guid? | User who triggered the action |
| ActorEmail | string | Email of the actor |
| Action | string | Human-readable: "TaskAssigned", "TaskCompleted", "TaskDeclined" |
| OldValue | string? | Previous status or value |
| NewValue | string? | New status or value |
| Timestamp | DateTime | UTC |

---

## Relationships Summary

```
Workspace
  └── Workflow (template)
        └── WorkflowTask[] (template steps)

Workspace
  └── WorkflowState (live instance, spawned from Workflow)
        └── WorkflowTaskState[] (live steps, spawned from WorkflowTask)
              └── WorkflowTaskAudit[] (change history per task step)
        └── WorkflowAudit[] (change history per workflow instance)
```

---

## Domain Rules

- **Template → Instance is one-way.** Spawning creates a snapshot. Changing the
  template later does NOT change running instances.
- **Due dates are always calculated.** `DueDate` on WorkflowTaskState is set at
  spawn time using `WorkflowTask.DueAtOffsetDays + WorkflowState.StartedAt`.
  Never hardcode a date.
- **Tokens are always hashed.** `CompletionToken` is stored as a hash.
  The raw token is sent in the email. Never store it plaintext.
- **Audit entries share the same transaction.** Write the audit entry and the
  state mutation in the same `SaveChangesAsync` call — one atomic operation.
- **All IDs are Guid internally, returned as strings in API responses.**
- **All DateTime values are UTC internally, returned as ISO 8601 strings in API responses.**
