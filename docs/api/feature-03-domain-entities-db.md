# Domain Entities + DB — Schema Reference

> Last updated: 2026-03-29
> Feature status: Approved — PR reviewed
> Related files:
>   web-api/src/StackFlow.Domain/Models/
>   web-api/src/StackFlow.Domain/Enums/
>   web-api/src/StackFlow.Domain/Constants/WellKnownIds.cs
>   web-api/src/StackFlow.Infrastructure/Persistence/AppDbContext.cs
>   web-api/src/StackFlow.Infrastructure/Persistence/Configurations/

This feature contains no HTTP endpoints. It establishes the full database schema,
EF Core configuration, and seed data that every subsequent feature builds on.
This document is the reference for the tables, columns, constraints, and seeded rows
that exist after the InitialCreate migration has been applied.

---

## Enums

Enums are stored as integers in the database (EF Core default). The integer value
is the zero-based ordinal position of each member as declared in the enum file.

### UserRole

| Value | Integer |
|---|---|
| Admin | 0 |
| Member | 1 |

File: `web-api/src/StackFlow.Domain/Enums/UserRole.cs`

---

### AssigneeType

| Value | Integer |
|---|---|
| Internal | 0 |
| External | 1 |

File: `web-api/src/StackFlow.Domain/Enums/AssigneeType.cs`

---

### NodeType

| Value | Integer |
|---|---|
| Task | 0 |
| Approval | 1 |
| Condition | 2 |
| Notification | 3 |
| ExternalStep | 4 |
| Deadline | 5 |

File: `web-api/src/StackFlow.Domain/Enums/NodeType.cs`

---

### WorkflowStatus

Used as the `Status` column on the `WorkflowStates` table.

| Value | Integer |
|---|---|
| InProgress | 0 |
| Completed | 1 |
| Cancelled | 2 |

File: `web-api/src/StackFlow.Domain/Enums/WorkflowStatus.cs`

---

### ContextType

| Value | Integer |
|---|---|
| Standalone | 0 |
| Group | 1 |

File: `web-api/src/StackFlow.Domain/Enums/ContextType.cs`

---

### WorkflowTaskStatus

Used as the `Status` column on the `WorkflowTaskStates` table.

| Value | Integer |
|---|---|
| Pending | 0 |
| InProgress | 1 |
| Completed | 2 |
| Declined | 3 |
| Expired | 4 |
| Skipped | 5 |

File: `web-api/src/StackFlow.Domain/Enums/WorkflowTaskStatus.cs`

---

### Priority

| Value | Integer |
|---|---|
| Low | 0 |
| Medium | 1 |
| High | 2 |
| Critical | 3 |

File: `web-api/src/StackFlow.Domain/Enums/Priority.cs`

---

## WellKnownIds

Fixed Guids for system-seeded entities. These values are stable — changing them
requires a migration to update the seeded rows.

| Constant | Value | Purpose |
|---|---|---|
| `WellKnownIds.DemoWorkspaceId` | `00000000-0000-0000-0000-000000000001` | Demo workspace pre-populated for Phase 1 |
| `WellKnownIds.GlobalWorkspaceId` | `00000000-0000-0000-0000-000000000002` | Houses the seeded workflow templates available to all workspaces |

File: `web-api/src/StackFlow.Domain/Constants/WellKnownIds.cs`

---

## Tables

### Workspaces

Top-level organisational container. All workflows, users, and workflow instances
belong to a workspace.

| Column | Type | Nullable | Constraints |
|---|---|---|---|
| Id | uuid | No | PK |
| Name | varchar(200) | No | |
| CreatedAt | timestamptz | No | UTC preserved on read |

FK constraints: none (root table)

---

### Users

A registered user who is a member of a workspace.

Note: Phase 2 will add password hash, OTP, Google OAuth, and refresh token columns
to this table. The Phase 1 schema is intentionally minimal.

| Column | Type | Nullable | Constraints |
|---|---|---|---|
| Id | uuid | No | PK |
| WorkspaceId | uuid | No | FK -> Workspaces(Id) RESTRICT |
| Email | varchar(256) | No | |
| FullName | varchar(200) | No | |
| Role | integer | No | UserRole enum |
| CreatedAt | timestamptz | No | UTC preserved on read |

---

### Workflows

A workflow template — the reusable blueprint. Not a live execution.
Live executions are rows in `WorkflowStates`.

| Column | Type | Nullable | Constraints |
|---|---|---|---|
| Id | uuid | No | PK |
| WorkspaceId | uuid | No | FK -> Workspaces(Id) RESTRICT |
| Name | varchar(200) | No | |
| Description | varchar(2000) | No | |
| Category | varchar(200) | Yes | Optional grouping label (e.g. "HR", "Finance") |
| IsActive | boolean | No | |
| CreatedAt | timestamptz | No | UTC preserved on read |
| UpdatedAt | timestamptz | No | UTC preserved on read |

---

### WorkflowTasks

One step in a workflow template. Not a live execution.
Live execution copies are rows in `WorkflowTaskStates`.

| Column | Type | Nullable | Constraints |
|---|---|---|---|
| Id | uuid | No | PK |
| WorkflowId | uuid | No | FK -> Workflows(Id) CASCADE |
| Title | varchar(200) | No | |
| Description | varchar(2000) | No | |
| AssigneeType | integer | No | AssigneeType enum |
| DefaultAssignedToEmail | varchar(256) | Yes | Pre-filled assignee for Internal tasks |
| OrderIndex | integer | No | 0-based position in the workflow sequence |
| DueAtOffsetDays | integer | No | Days after spawn date that this task is due |
| NodeType | integer | No | NodeType enum |
| ConditionConfig | varchar(4000) | Yes | Serialised JSON branch rules; null for non-Condition nodes |
| ParentTaskId | uuid | Yes | FK -> WorkflowTasks(Id) RESTRICT; null for top-level tasks |

Cascade rule: deleting a `Workflow` cascades to delete all its `WorkflowTask` rows.
Self-reference: a child branch task points back to its parent Condition node via `ParentTaskId`.

---

### WorkflowStates

A live running instance of a workflow template. Created when a user spawns a workflow.
Multiple rows can exist for the same `WorkflowId` (e.g. the same onboarding template
used for many new starters).

| Column | Type | Nullable | Constraints |
|---|---|---|---|
| Id | uuid | No | PK |
| WorkflowId | uuid | No | FK -> Workflows(Id) RESTRICT |
| WorkspaceId | uuid | No | FK -> Workspaces(Id) RESTRICT |
| Status | integer | No | WorkflowStatus enum |
| ContextType | integer | No | ContextType enum |
| BatchId | uuid | Yes | Groups instances spawned together; null for Standalone |
| ReferenceNumber | varchar(100) | No | Human-readable reference (e.g. "WF-2026-001") |
| StartedAt | timestamptz | No | UTC preserved on read |
| CompletedAt | timestamptz | Yes | UTC preserved on read |
| CancelledAt | timestamptz | Yes | UTC preserved on read |

---

### WorkflowTaskStates

A live instance of one task step within a running workflow instance.
Created for each `WorkflowTask` when a `WorkflowState` is spawned.

| Column | Type | Nullable | Constraints |
|---|---|---|---|
| Id | uuid | No | PK |
| WorkflowStateId | uuid | No | FK -> WorkflowStates(Id) CASCADE |
| WorkflowTaskId | uuid | No | FK -> WorkflowTasks(Id) RESTRICT |
| Status | integer | No | WorkflowTaskStatus enum |
| AssignedToEmail | varchar(256) | No | Email of the assignee at spawn time |
| AssignedToUserId | uuid | Yes | Null for external contributors |
| DueDate | timestamptz | Yes | Calculated from WorkflowTask.DueAtOffsetDays at spawn; UTC preserved on read |
| CompletionToken | varchar(500) | Yes | Phase 2 — stored hashed; null until issued |
| TokenExpiresAt | timestamptz | Yes | UTC preserved on read |
| IsTokenUsed | boolean | No | |
| CompletionNotes | varchar(2000) | Yes | Populated by assignee on completion |
| DeclineReason | varchar(2000) | Yes | Populated by assignee on decline |
| Priority | integer | No | Priority enum |

Cascade rule: deleting a `WorkflowState` cascades to delete all its `WorkflowTaskState` rows.

---

### WorkflowAudits

Immutable audit record for every state change on a `WorkflowState`.
Written by command handlers — never updated, never deleted.

| Column | Type | Nullable | Constraints |
|---|---|---|---|
| Id | uuid | No | PK |
| WorkflowStateId | uuid | No | FK -> WorkflowStates(Id) RESTRICT |
| ActorUserId | uuid | Yes | Null for automated system actions |
| ActorEmail | varchar(256) | No | |
| Action | varchar(200) | No | Human-readable name (e.g. "WorkflowStarted") |
| OldValue | varchar(4000) | Yes | Serialised previous value; null for create events |
| NewValue | varchar(4000) | Yes | Serialised new value |
| Timestamp | timestamptz | No | UTC preserved on read |

Restrict rule: a `WorkflowState` cannot be deleted while audit records reference it.

---

### WorkflowTaskAudits

Immutable audit record for every state change on a `WorkflowTaskState`.
Written by command handlers — never updated, never deleted.

| Column | Type | Nullable | Constraints |
|---|---|---|---|
| Id | uuid | No | PK |
| WorkflowTaskStateId | uuid | No | FK -> WorkflowTaskStates(Id) RESTRICT |
| ActorUserId | uuid | Yes | Null for automated system actions |
| ActorEmail | varchar(256) | No | |
| Action | varchar(200) | No | Human-readable name (e.g. "TaskCompleted") |
| OldValue | varchar(4000) | Yes | Serialised previous value; null for create events |
| NewValue | varchar(4000) | Yes | Serialised new value |
| Timestamp | timestamptz | No | UTC preserved on read |

Restrict rule: a `WorkflowTaskState` cannot be deleted while audit records reference it.

---

## Seed Data

All seed rows are inserted by the `InitialCreate` migration via EF Core's `HasData`.
All Guids and timestamps are fixed — the migration is deterministic and idempotent.
The seed timestamp for all rows is `2026-01-01T00:00:00Z`.

### Workspaces (2 rows)

| Id | Name |
|---|---|
| `00000000-0000-0000-0000-000000000001` | Demo Workspace |
| `00000000-0000-0000-0000-000000000002` | Global |

### Workflows (3 rows, all in GlobalWorkspaceId)

| Id | Name | Category |
|---|---|---|
| `10000000-0000-0000-0000-000000000001` | Employee Onboarding | HR |
| `10000000-0000-0000-0000-000000000002` | Purchase Approval | Finance |
| `10000000-0000-0000-0000-000000000003` | Client Offboarding | Operations |

### WorkflowTasks (15 rows)

**Employee Onboarding (6 tasks)**

| Id | OrderIndex | Title | NodeType | DueAtOffsetDays |
|---|---|---|---|---|
| `20000000-0000-0000-0000-000000000001` | 0 | Send offer letter | Task | 1 |
| `20000000-0000-0000-0000-000000000002` | 1 | Set up workstation | Task | 3 |
| `20000000-0000-0000-0000-000000000003` | 2 | Schedule orientation session | Task | 3 |
| `20000000-0000-0000-0000-000000000004` | 3 | Complete HR paperwork | Task | 5 |
| `20000000-0000-0000-0000-000000000005` | 4 | Assign buddy / mentor | Task | 5 |
| `20000000-0000-0000-0000-000000000006` | 5 | 30-day check-in | Task | 30 |

**Purchase Approval (4 tasks)**

| Id | OrderIndex | Title | NodeType | DueAtOffsetDays |
|---|---|---|---|---|
| `20000000-0000-0000-0000-000000000007` | 0 | Submit purchase request | Task | 1 |
| `20000000-0000-0000-0000-000000000008` | 1 | Manager approval | Approval | 3 |
| `20000000-0000-0000-0000-000000000009` | 2 | Raise purchase order | Task | 5 |
| `20000000-0000-0000-0000-000000000010` | 3 | Confirm delivery and close | Task | 14 |

**Client Offboarding (5 tasks)**

| Id | OrderIndex | Title | NodeType | DueAtOffsetDays |
|---|---|---|---|---|
| `20000000-0000-0000-0000-000000000011` | 0 | Send offboarding notification | Task | 1 |
| `20000000-0000-0000-0000-000000000012` | 1 | Retrieve client assets | Task | 3 |
| `20000000-0000-0000-0000-000000000013` | 2 | Revoke system access | Task | 3 |
| `20000000-0000-0000-0000-000000000014` | 3 | Issue final invoice | Task | 5 |
| `20000000-0000-0000-0000-000000000015` | 4 | Conduct exit interview | Task | 7 |

All 15 seed tasks have `AssigneeType = Internal`, `DefaultAssignedToEmail = null`,
`ConditionConfig = null`, and `ParentTaskId = null`.

---

## Foreign Key Cascade Summary

| Relationship | On Delete |
|---|---|
| User -> Workspace | Restrict |
| Workflow -> Workspace | Restrict |
| WorkflowTask -> Workflow | Cascade |
| WorkflowTask (child) -> WorkflowTask (parent) | Restrict |
| WorkflowState -> Workflow | Restrict |
| WorkflowState -> Workspace | Restrict |
| WorkflowTaskState -> WorkflowState | Cascade |
| WorkflowTaskState -> WorkflowTask | Restrict |
| WorkflowAudit -> WorkflowState | Restrict |
| WorkflowTaskAudit -> WorkflowTaskState | Restrict |

The only cascades are: deleting a Workflow deletes its task definitions, and deleting
a WorkflowState deletes its task state rows. All other relationships are Restrict —
parent rows cannot be deleted while child rows exist.
