---
name: feature-brief-writer
description: >
  Generate a correctly formatted StackFlow Feature Brief. Loaded once at session
  start by feature-provider. Do not auto-load — load explicitly and once per
  session. Produces the exact brief structure all downstream agents depend on.
allowed-tools: Read, Write
---

<!--
  WHY THIS SKILL EXISTS
  ──────────────────────
  Every agent downstream of the Feature Provider reads the Feature Brief.
  If the brief is missing a section, vague on scope, or has an incomplete
  API contract, one or more agents will either make assumptions (bad) or
  stop and ask (slow).

  This skill ensures the brief is always complete, always consistent, and
  always uses the exact format every agent expects. It is the shared language
  of the entire StackFlow build pipeline.

  THE BRIEF IS A CONTRACT, NOT A SUGGESTION:
  The API contract section is the binding agreement between Backend Agent
  and Frontend Agent. Once agents start building, the contract does not
  change without a Pivot Brief. Getting it right here saves cycles everywhere.

  IF CLAUDE CODE CEASES TO EXIST:
  A developer can produce a Feature Brief manually by following the template
  below. Every section is explained. Every field has a purpose. The brief
  tells the builder exactly what to build without ambiguity.
-->

# StackFlow — Feature Brief Writer

---

## The Core Rule

```
A Feature Brief is complete when every section below is filled in.
An incomplete brief is not a brief — it is a conversation starter.
Do not hand an incomplete brief to any agent.
```

---

## Feature Brief Template

Copy this template exactly. Fill every section. Do not skip or rename sections.
Do not add sections not listed here — extra context belongs in "Agent instructions".

```markdown
# Feature Brief: {Feature Name}
Phase: {1 / 2 / 3}
Status: Ready for implementation
Brief date: {today's date}

---

## What this feature does (plain English)
{2–4 sentences. Write as if explaining to a developer who has never seen StackFlow.
What does the user experience when this feature is active?
What problem does it solve?
Do not use technical jargon — plain English only.}

---

## Scope — what IS in this brief
{Bullet list. Be specific. Each item must be something a developer can check off.
❌ Bad:  "Handle errors"
✅ Good: "Return 400 with { error: string } when name is missing"
❌ Bad:  "Build the UI"
✅ Good: "Build WorkflowListPage showing all workflows in a card grid"}

---

## Scope — what is NOT in this brief
{Bullet list of related things explicitly excluded.
This prevents agents from over-building. Be specific.
Example: "Authentication is out of scope — Phase 2"
Example: "Pagination is out of scope — all workflows returned for now"}

---

## Domain entities involved
{List every entity from CLAUDE.md this feature touches.
Note any new fields being added to existing entities.
Note any new entities being introduced.
Example:
  - Workflow (existing) — no new fields
  - WorkflowTask (existing) — adding NodeType field
  - WorkflowState (new entity) — see domain model}

---

## API Contract
{Every endpoint this feature exposes. See API Contract Format below.
This section is mandatory. "TBD" is not acceptable here.
Both Backend Agent and Frontend Agent build to this contract.}

---

## Frontend routes and views
{List every route that changes or gets created.
List every page component needed.
List any existing components that need to be modified.
Example:
  /workflows             → WorkflowListPage (new)
  /workflows/:id         → WorkflowDetailPage (new)
  /workflows/:id/builder → WorkflowBuilderPage (existing — add save button)}

---

## RabbitMQ events (if any)
{For each event:
  Event name: {PascalCase}
  Published by: {HandlerName}
  Consumed by: {ConsumerName}
  Payload: { field: type, field: type }
If none: write "None for this feature."}

---

## SignalR events (if any)
{For each real-time event:
  Event name: {camelCase}
  Hub: {HubName}
  Payload: { field: type }
  Frontend hook: {useHookName} — invalidates query key: {['key', id]}
If none: write "None for this feature."}

---

## Audit requirements
{List every action that must produce an audit entry.
Be specific about which entity is audited and what Action string to use.
Example:
  - WorkflowState created → WorkflowAudit: Action = "WorkflowStarted"
  - WorkflowTaskState assigned → WorkflowTaskAudit: Action = "TaskAssigned"
If no state mutations: write "No audit entries required for this feature."}

---

## Acceptance criteria
{Numbered list. Each item is a testable Given/When/Then statement.
The Test Agent uses these to write tests. The PR Reviewer checks against them.
Every "In scope" bullet above should map to at least one criterion.

1. Given a valid JWT and a name, when POST /api/workflows is called,
   then a 201 is returned with the new workflow ID and isActive: true.
2. Given an empty name, when POST /api/workflows is called,
   then a 400 is returned with { error: "Name is required" }.
3. Given a non-existent workflow ID, when GET /api/workflows/{id} is called,
   then a 404 is returned.
4. Given the workflows page loads, when there are no workflows,
   then the empty state is shown with a "Create workflow" button.}

---

## Agent instructions

Backend Agent:
{Ordered list of what to build. Reference the build order from backend-agent.md:
  1. Domain changes (new entities / new fields)
  2. Application layer (DTOs, commands, queries, handlers, validators)
  3. Infrastructure layer (EF config, repositories, migrations)
  4. API layer (controllers, DI registration)
  5. Tests (unit + integration)}

Frontend Agent:
{Ordered list of what to build. Note the wait condition explicitly:
  - Wait for Backend Agent confirmation before implementing API calls
  - List components, pages, routes in build order}

Handoff point:
{Exactly what Backend Agent must produce before Frontend Agent begins implementation.
Example: "Backend Agent must confirm POST /api/workflows and GET /api/workflows/{id}
are live and returning the contract shapes before Frontend Agent calls the API."}
```

---

## API Contract Format

Every endpoint in the brief must follow this format exactly.
Both agents build to this — ambiguity here costs a full feature cycle.

```
#### {HTTP METHOD} /api/{resource}/{optional-params}
Auth: {Public / JWT required / Admin only}

Request body:
{
  fieldName: type (required/optional),
  fieldName: type (required/optional)
}

Response {status code}:
{
  fieldName: type,
  fieldName: type
}

Error responses:
  400 { error: string }    ← validation failure
  401 { error: "Unauthorised" }  ← missing or invalid JWT
  404 { error: "{Resource} not found" }  ← include only if applicable
```

**API Contract rules — non-negotiable:**
- All IDs are UUIDs returned as `string` (never `Guid` or raw UUID)
- All dates are ISO 8601 strings (e.g. `"2025-03-28T09:00:00Z"`)
- Error responses always have shape `{ "error": string }` — nothing else
- Pagination always uses `{ items: [], totalCount: number, page: number, pageSize: number }`
- Every endpoint specifies its auth requirement
- No `{...}` or `...` placeholders — every field must be named and typed

**Example — fully specified contract:**

```
#### POST /api/workflows
Auth: JWT required

Request body:
{
  name: string (required, max 200),
  description: string (optional, max 1000),
  workspaceId: string UUID (required)
}

Response 201:
{
  id: string UUID,
  name: string,
  description: string,
  workspaceId: string UUID,
  isActive: boolean,
  createdAt: string ISO 8601
}

Error responses:
  400 { error: string }
  401 { error: "Unauthorised" }

---

#### GET /api/workflows/{id}
Auth: JWT required

Response 200:
{
  id: string UUID,
  name: string,
  description: string,
  workspaceId: string UUID,
  isActive: boolean,
  createdAt: string ISO 8601,
  updatedAt: string ISO 8601,
  tasks: [
    {
      id: string UUID,
      title: string,
      orderIndex: number,
      nodeType: string
    }
  ]
}

Error responses:
  401 { error: "Unauthorised" }
  404 { error: "Workflow not found" }
```

---

## Self-Check Before Handing Off

Before handing a brief to any agent, verify every item:

```
□ What this feature does — written in plain English, 2-4 sentences
□ Scope IN — every item is specific and checkable
□ Scope OUT — related exclusions are named explicitly
□ Domain entities — every touched entity listed, new fields noted
□ API Contract — every endpoint fully specified, no placeholders
□ Frontend routes — every route and component named
□ RabbitMQ events — listed or explicitly "None"
□ SignalR events — listed or explicitly "None"
□ Audit requirements — every mutation listed or explicitly "None"
□ Acceptance criteria — one testable statement per scope IN item
□ Agent instructions — Backend order, Frontend order, handoff point named
```

If any box is unchecked, the brief is not ready.

---

## What You Must Never Do

- Leave the API Contract as "TBD" or with `{...}` placeholders — it must be complete
- Combine two features into one brief — one brief per feature, one feature at a time
- Include implementation suggestions in the brief — what to build, not how to build it
- Skip the Scope OUT section — it is as important as Scope IN for preventing over-building
- Mark a brief as "Ready for implementation" before all sections are complete
- Change the API contract after agents have started building — that requires a Pivot Brief
