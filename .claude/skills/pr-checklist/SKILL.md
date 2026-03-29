---
name: pr-checklist
description: >
  Run the full StackFlow PR review checklist. Loaded once at session start by
  pr-reviewer. Do not auto-load — load explicitly and once at the start of
  each review session. Every item must be checked on every review.
allowed-tools: Read, Glob, Grep
---

<!--
  WHY THIS SKILL EXISTS
  ──────────────────────
  The PR review is the last automated quality gate before Samuel does real
  testing. If a violation slips through here, it either gets caught by Samuel
  (slow, manual, frustrating) or it ships (much worse).

  This skill is the complete, authoritative checklist for StackFlow code
  quality. It covers architecture, patterns, security, tests, and the API
  contract — everything that matters.

  WHY EVERY ITEM RUNS EVERY TIME:
  Pattern violations compound. A shortcut in one feature becomes the
  precedent for the next. Running the full checklist on every feature keeps
  the codebase consistent and prevents gradual drift from CLAUDE.md.

  IF CLAUDE CODE CEASES TO EXIST:
  A developer can perform this review manually by working through each
  section below. Mark each item, then produce the report. The process is
  the same whether a human or an agent runs it.
-->

# StackFlow — PR Review Checklist

---

## How to Use This Checklist

1. Read the Feature Brief to understand what was supposed to be built
2. Read the Backend Agent and/or Frontend Agent completion summaries
3. Read the relevant implementation files
4. Work through every section below — mark each item as:
   - **✅ PASS** — correct, no action needed
   - **❌ FAIL** — required fix, must be resolved before approval
   - **⚠️ SUGGESTION** — optional improvement Samuel may choose to act on
5. Produce the review report in the format at the bottom
6. Never approve with open ❌ items

---

## Backend Checklist

### Architecture & Layer Rules
```
□ Clean Architecture layers respected — no cross-layer imports in the wrong direction
  (Domain has zero imports from Application, Infrastructure, or Api)
  (Application has zero imports from Infrastructure or Api)
  Check: grep for "using StackFlow.Infrastructure" inside Application folder
□ Domain entities have NO data annotations — [Required], [MaxLength] etc. are violations
  Check: grep for "\[Required\]\|\[MaxLength\]\|\[Key\]" in Domain/Models/
□ Application layer has no EF Core references
  Check: grep for "using Microsoft.EntityFrameworkCore" in Application folder
□ Commands use ICommand<T> marker — not IRequest<T> directly
□ Queries use IQuery<T> marker — not IRequest<T> directly
□ Handlers implement IRequestHandler<TRequest, TResponse> with correct generics
□ Each handler handles exactly one command or query — no multi-purpose handlers
```

### Result Pattern
```
□ All handler return types are Result<T> or Result — no raw types returned
□ Result.Fail() used for all business logic failures (not found, invalid state, permissions)
□ No business exceptions thrown from handlers (NotFoundException, etc.)
□ Infrastructure exceptions (DbException) NOT caught in handlers — let middleware handle
□ BaseApiController.HandleResult() used in every controller action
```

### Mediator & Pipeline
```
□ No MediatR references anywhere — check all .csproj files for MediatR package
  Check: grep -r "MediatR" web-api/
□ No manual handler registrations in Program.cs or extension methods
□ Validators implement IValidator<T> and are in the Application assembly
  (Assembly scanning only picks up validators in the registered assembly)
□ Pipeline order correct: ValidationBehavior → LoggingBehavior → Handler
```

### Controllers
```
□ Zero business logic in controllers — each action is one Mediator.Send() line
□ All actions delegate to Mediator.Send()
□ [Authorize] attribute present on all endpoints that require JWT (per Feature Brief)
□ [WorkspaceRole] attribute applied where workspace scoping is required
□ Route naming follows api/{resource} convention — lowercase, plural nouns
□ No route conflicts with existing controllers
```

### Repository & Data Access
```
□ Repository interfaces defined in Application/Common/Interfaces/
□ Repository implementations in Infrastructure/Persistence/Repositories/ only
□ SaveChangesAsync(ct) called in handler via IUnitOfWork — never inside a repository
  Check: grep for "SaveChangesAsync" in Infrastructure/Repositories/
□ No N+1 queries — check for repository calls inside foreach or LINQ loops
□ All queries that return data filter by WorkspaceId where applicable
□ No raw SQL strings — all queries via EF LINQ
```

### Audit Trail
```
□ Every command mutating WorkflowState writes a WorkflowAudit entry
□ Every command mutating WorkflowTaskState writes a WorkflowTaskAudit entry
□ All required audit fields present: ActorUserId, ActorEmail, Action, OldValue, NewValue, Timestamp
□ OldValue captured BEFORE the mutation — not after
□ Audit entry saved in the SAME SaveChangesAsync call as the mutation
□ Action strings are descriptive past-tense verbs (e.g. "TaskCompleted", not "Update")
```

### Events & Email
```
□ RabbitMQ events published AFTER SaveChangesAsync — never before
  Check: verify PublishAsync appears after SaveChangesAsync in every handler
□ Event consumers implement IEventHandler<TEvent>
□ Email sent via IEmailService only — no direct MailKit calls in handlers or controllers
```

### Security
```
□ No sensitive data (passwords, tokens, OTP codes) in log output
  Check: grep for "token\|password\|otp" in logging calls
□ Password reset tokens stored hashed — never plaintext
□ OTP codes stored hashed — never plaintext
□ External task completion tokens stored hashed
□ /forgot-password always returns HTTP 200 regardless of whether email exists
□ No hardcoded secrets or connection strings in any file
  Check: grep for "Password=\|ApiKey=\|Secret=" in src/
```

### Migrations
```
□ Migration file exists for any new tables or schema changes
□ Migration name follows: {YYYYMMDDHHmm}_{PascalCaseDescription}
□ Up() SQL reviewed — no accidental DROP TABLE or DROP COLUMN
□ NOT NULL columns on existing tables have a default value or are on a new table
□ Down() method correctly reverses all Up() operations
□ Foreign key columns have indexes
```

### Tests
```
□ Unit tests exist for handler logic — happy path and every business rule failure
□ Unit tests exist for each validator — required fields, length limits, valid input
□ Integration test for the happy path endpoint (correct status + response shape)
□ Integration test for key failure cases — 400 (invalid input), 401 (unauth), 404 (not found)
□ Tests for state mutations verify audit entry was written (not just "it worked")
□ No tests that only test implementation details — tests cover behaviour and contracts
```

### API Contract Verification
```
□ Every endpoint in the Feature Brief's API contract is implemented
□ HTTP methods match the contract exactly
□ Route paths match the contract exactly
□ Request body field names and types match exactly (case-sensitive)
□ Response field names and types match exactly
□ Auth requirements match the contract
□ No extra undocumented endpoints added without flagging to Samuel
```

---

## Frontend Checklist

### Architecture & Module Structure
```
□ Feature module structure followed: entities/ dtos/ enums/ infrastructure/ hooks/ ui/
□ No direct apiClient or axios calls in component files or page files
  Check: grep -r "apiClient\|axios" src/modules/{feature}/ui/
□ No second Axios instance created
  Check: grep -r "new axios\|axios.create" src/ (should only appear in api-client.ts)
□ Module only imports from its own folder or modules/shared/ — no cross-feature imports
  Check: grep -r "from '../{other-feature}" src/modules/{feature}/
```

### State Management
```
□ Server data in React Query — NOT in Redux
  Check: no workflow/task/user data in any Redux slice
□ Auth tokens and workspace ID in Redux — NOT in React Query
  Check: no useQuery hooks fetching auth tokens
□ No useState that mirrors React Query data (redundant state)
□ No useEffect manually syncing React Query data into local state
```

### API Consumption
```
□ All endpoints consumed match the Feature Brief's API contract
□ Request DTO field names match contract exactly (case-sensitive)
□ Response DTO field names match contract exactly
□ Query keys are specific: ['workflows', id] — not generic ['data']
□ invalidateQueries called with correct query keys on mutation success
□ Service methods are in infrastructure/{feature}-service.ts only
```

### Forms
```
□ Every form uses React Hook Form + Zod
□ No uncontrolled inputs (no ref.current.value, no document.querySelector)
□ Zod schema validates all required fields with meaningful error messages
□ Password fields have show/hide toggle
□ Submit button disabled while isSubmitting or mutation isPending
```

### UX Standards
```
□ Loading skeleton shown for every async state — no empty white space
□ Error state handled with Sonner toast + readable message
□ Destructive actions (delete, cancel) have AlertDialog confirmation
□ All dates formatted with date-fns — no .toLocaleDateString() anywhere
  Check: grep -r "toLocaleDateString\|toLocaleString" src/modules/{feature}/
```

### Real-Time (if SignalR in brief)
```
□ SignalR subscriptions cleaned up in useEffect return function
  Check: every signalrClient.on() has a matching .off() in cleanup
□ SignalR events invalidate the correct React Query cache keys
```

### Routing
```
□ New routes added to router/index.tsx
□ Correct guard applied: ProtectedRoute / AdminRoute / GuestRoute / Public
□ External token page (/complete/:token) is public — no auth guard
```

### TypeScript
```
□ No use of any type — all types are explicit
□ Entity interfaces in entities/ match CLAUDE.md domain model field names exactly
□ DTO types in dtos/ match API contract shapes exactly
□ No @ts-ignore or @ts-expect-error comments without explanation
```

---

## Review Report Format

Produce this exact structure. Do not skip sections. If a section has no items, write "None."

```
# PR Review: {Feature Name}
Reviewed by: PR Reviewer Agent
Date: {today's date}
Overall: ✅ Approved / ❌ Changes required

---

## Required changes (must fix before marking done)

### Backend
1. [File: path/to/file.cs, Line ~{N}] {Issue description} — {why it matters}
2. ...

### Frontend
1. [File: path/to/file.ts, Line ~{N}] {Issue description} — {why it matters}
2. ...

---

## Optional suggestions

### Backend
- {Suggestion} — {rationale}

### Frontend
- {Suggestion} — {rationale}

---

## API contract verification
[x] {METHOD} /api/{route} — implemented, matches contract
[x] {METHOD} /api/{route} — implemented, matches contract
[ ] {METHOD} /api/{route} — MISSING or shape does not match contract

---

## Checklist summary
Backend: {X}/{Y} items passed
Frontend: {X}/{Y} items passed

---

## Verdict

{If approved}:
Feature is ready. Hand to Docs Agent and Test Agent in parallel.

{If changes required}:
Return to {Backend Agent / Frontend Agent / both}.
Paste the "Required changes" section above into the relevant agent's context.
Re-submit for review when all fixes are complete.

---

## CLAUDE.md violations detected (if any)
⚠️ File: {path}
   Violation: {rule broken}
   Severity: Must fix / Should fix
   Route to: {Backend Agent / Frontend Agent}
```

---

## What You Must Never Do

- Skip any checklist section — run every item, every review
- Approve with open ❌ items — all required changes must be resolved first
- Review work in progress — only review features the builder has marked complete
- Change CLAUDE.md standards in response to a violation — flag to Samuel instead
- Consolidate two features into one review — one Feature Brief per review
- Write implementation code to fix issues — describe the problem, let the builder fix it
