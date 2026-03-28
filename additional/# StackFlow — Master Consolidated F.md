# StackFlow — Master Consolidated Flow

**Document type:** Single source of truth for the full build process  
**Status:** Active — update as decisions are made  
**Last updated:** March 2026  
**Owner:** Samuel

---

## What StackFlow Is Building

StackFlow is an intelligent, adaptive workflow process engine for developers and small teams. It replaces rigid task management with living, branching workflows that adapt mid-execution, support approvals, handle external contributors via token links, and provide a full audit history of every action taken.

**Core problem it solves:** Workflows are rigid and can't adapt mid-process. Tasks get stuck, nobody knows whose turn it is, and everything lives in emails.

**Design principle:** The user should feel calm — everything organised, clear, never overwhelming.

---

## The Full Tooling Stack

```
┌─────────────────────────────────────────────────────────┐
│                    SAMUEL (You)                         │
│  Strategic decisions. Handoffs. Real testing. Reviews.  │
└───────────────────┬─────────────────────────────────────┘
                    │
        ┌───────────┼───────────────┐
        │           │               │
   CLAUDE PRO   CLAUDE CODE    CLAUDE COWORK
   Strategic    Orchestrator   File management
   thinking     + builds       Phase tracker
   Reviews      Sub-agents     Docs organisation
   CLAUDE.md    Skills         Output organisation
   decisions    MCPs
        │           │               │
        └───────────┼───────────────┘
                    │
    ┌───────────────┼────────────────────┐
    │               │                    │
  MCPs           SKILLS              SUB-AGENTS
  ─────          ──────              ──────────
  GitHub         stackflow-domain    feature-provider
  PostgreSQL     ef-migration        backend-agent
  Sentry         result-pattern      frontend-agent
  Stitch         audit-trail         pr-reviewer
  Playwright     feature-brief-      docs-agent
                 writer              test-agent
                 pr-checklist        debug-edit-agent
                 stackflow-design
                 e2e-testing
```

---

## The Master Build Flow

### Pre-Build (Do Once)

```
[ ] GitHub PAT created and GitHub MCP installed
[ ] PostgreSQL MCP installed
[ ] All 7 AGENT-*.md files converted to sub-agent format (.claude/agents/)
[ ] Sub-agent routing rules added to CLAUDE.md
[ ] stackflow-domain skill created
[ ] result-pattern skill created
[ ] audit-trail skill created
[ ] ef-migration skill created
[ ] pr-checklist skill created
[ ] feature-brief-writer skill created
[ ] Verify: claude agents → lists all 7 agents
```

### Before Real Testing Phase (add when you reach Step 6)

```
[ ] Playwright MCP installed: claude mcp add playwright --command "npx" --args "@playwright/mcp"
[ ] Playwright browsers installed: npx playwright install
[ ] playwright.config.ts created at project root
[ ] e2e-testing skill created (.claude/skills/e2e-testing/SKILL.md)
[ ] tests/e2e/ folder created
[ ] tests/e2e/helpers/auth.ts helper created
[ ] AGENT-TEST.md updated with E2E scope
```

### Design Phase (Do Before Phase 1 Frontend)

```
[ ] Page planning session — Claude Pro fresh chat
    → Output: PAGE INVENTORY document
[ ] Stitch design session — stitch.withgoogle.com (Experimental mode)
    → Landing page
    → Auth flow (4 screens)
    → Workflow builder canvas
    → My Tasks dashboard
    → Active Workflows board
    → Admin panel
    → External task page
[ ] Export from Stitch: DESIGN.md + HTML archives per screen group
[ ] Organise exports into web-frontend/src/design-reference/
[ ] stackflow-design skill populated with DESIGN.md
[ ] Stitch MCP installed (pipes designs directly into Claude Code)
[ ] Consolidation: update STACKFLOWSCOPE with page decisions
[ ] Consolidation: update AGENT-FRONTEND.md with design system decisions
```

---

## Feature Build Flow (Repeat for Every Feature)

```
┌─────────────────────────────────────────────────────┐
│  SAMUEL: "Brief: {feature name}"                    │
└──────────────────┬──────────────────────────────────┘
                   ▼
        [feature-provider agent]
        uses: feature-brief-writer skill
        produces: Feature Brief + API Contract
                   │
                   ▼
┌─────────────────────────────────────────────────────┐
│  SAMUEL reviews brief. Approves or requests changes.│
│  SAMUEL: "Build this"                               │
└────────────┬────────────────────────┬───────────────┘
             │                        │
             ▼ PARALLEL               ▼
  [backend-agent]           [frontend-agent]
  reads: Feature Brief      reads: Feature Brief
  reads: CLAUDE.md          reads: CLAUDE.md
  uses: ef-migration        WAITS for backend signal
  uses: result-pattern      uses: stackflow-design
  uses: audit-trail         uses: shadcn/ui
  uses: GitHub MCP          uses: React Query
  uses: PostgreSQL MCP      uses: Redux (auth only)
             │                        │
             ▼                        ▼ (after backend ready)
  Build complete summary    Build complete summary
             │                        │
             └──────────┬─────────────┘
                        ▼
             [pr-reviewer agent]
             uses: pr-checklist skill
             uses: GitHub MCP (read diff)
             produces: PR Review report
                        │
           ┌────────────┴────────────┐
           │ Changes required?       │ Approved?
           ▼                         ▼
   Route back to              ┌──────┴──────┐
   backend-agent or           │             │
   frontend-agent        [docs-agent]  [test-agent]
   with review report    produces:     produces:
                         docs/api/     xUnit unit tests
                         docs/         Vitest component tests
                         features/     Playwright E2E tests
                              │             │
                              └──────┬──────┘
                                     ▼
                              SAMUEL tests manually
                              (Real Tester)
                              uses: Playwright MCP
                              for exploratory testing
                                     │
                         ┌───────────┴──────────┐
                         │ Bug found?            │ All good?
                         ▼                       ▼
              [debug-edit-agent]        SAMUEL: "Update tracker:
              diagnoses root cause      {feature} is done"
              writes fix or                     │
              Change Brief                      ▼
                         │              [feature-provider]
              if fix > 3 files:         updates phase tracker
              back through              GitHub MCP closes issue
              [pr-reviewer]
                         │
              [test-agent]
              Playwright regression test
              uses: e2e-testing skill
              uses: Playwright MCP
```

---

## The Full Project Structure

```
stackflow/
├── CLAUDE.md                     ← Master project bible. Read before every session.
├── STACKFLOWSCOPE.md             ← Living scope document. Updated as phases complete.
├── docker-compose.yml            ← PostgreSQL + RabbitMQ + App
├── .env                          ← Secrets (gitignored)
├── .env.example                  ← All required env vars documented
├── .mcp.json                     ← MCP server configurations
├── playwright.config.ts          ← Playwright E2E config (add before Real Testing phase)
│
├── .claude/
│   ├── agents/                   ← Sub-agents (converted AGENT-*.md files)
│   │   ├── feature-provider.md
│   │   ├── backend-agent.md
│   │   ├── frontend-agent.md
│   │   ├── pr-reviewer.md
│   │   ├── docs-agent.md
│   │   ├── test-agent.md
│   │   └── debug-edit-agent.md
│   └── skills/                   ← Skills (domain knowledge + procedures)
│       ├── stackflow-domain/
│       │   └── SKILL.md
│       ├── ef-migration/
│       │   └── SKILL.md
│       ├── result-pattern/
│       │   └── SKILL.md
│       ├── audit-trail/
│       │   └── SKILL.md
│       ├── feature-brief-writer/
│       │   └── SKILL.md
│       ├── pr-checklist/
│       │   └── SKILL.md
│       ├── stackflow-design/
│       │   └── SKILL.md
│       └── e2e-testing/          ← Add before Real Testing phase
│           └── SKILL.md
│
├── docs/                         ← Generated by docs-agent
│   ├── api/                      ← API reference docs per feature
│   └── features/                 ← Feature summary docs
│
├── tests/
│   └── e2e/                      ← Playwright E2E tests (add before Real Testing phase)
│       ├── helpers/
│       │   └── auth.ts           ← Shared auth helper
│       ├── auth.spec.ts
│       ├── workflows.spec.ts
│       ├── tasks.spec.ts
│       └── regression/           ← Regression tests from Real Tester bugs
│
├── web-api/                      ← .NET 9 Backend
│   └── src/
│       ├── StackFlow.Domain/
│       ├── StackFlow.Application/
│       ├── StackFlow.Infrastructure/
│       └── StackFlow.Api/
│   └── tests/
│       ├── StackFlow.UnitTests/
│       └── StackFlow.IntegrationTests/
│
└── web-frontend/                 ← React 19 + TypeScript Frontend
    └── src/
        ├── design-reference/     ← READ ONLY — Stitch exports
        │   ├── DESIGN.md
        │   ├── landing/
        │   ├── auth/
        │   ├── dashboard/
        │   ├── workflows/
        │   └── admin/
        ├── lib/
        │   ├── api-client.ts
        │   └── signalr-client.ts
        ├── modules/
        │   ├── shared/
        │   ├── auth/
        │   ├── workflow-builder/
        │   ├── workflow-execution/
        │   ├── tasks/
        │   ├── templates/
        │   ├── notifications/
        │   ├── analytics/
        │   ├── calendar/
        │   ├── audit/
        │   ├── admin/
        │   └── external/
        └── store/
```

---

## Phase Tracker

### Pre-Build Setup

| Task | Status |
|---|---|
| GitHub PAT + GitHub MCP | Not started |
| PostgreSQL MCP | Not started |
| Sub-agents created (.claude/agents/) | Not started |
| Core skills created (7 of 8 — domain, ef-migration, result-pattern, audit-trail, brief-writer, pr-checklist, design) | Not started |
| Page planning session (Claude Pro) | Not started |
| Stitch design session | Not started |
| Design exports organised | Not started |
| stackflow-design skill populated | Not started |
| CLAUDE.md updated with agent routing rules | Not started |
| Playwright MCP + e2e-testing skill (before Real Testing) | Not started |

### Phase 1 — Core Engine + Drag & Drop Builder

| Feature | Status | Notes |
|---|---|---|
| Project scaffold | Not started | Solution, layers, Docker Compose |
| Domain entities | Not started | Workflow, WorkflowTask, WorkflowState, WorkflowTaskState, Audits |
| EF Core DbContext + migrations | Not started | Fluent API, one file per entity |
| Repository interfaces + implementations | Not started | |
| Custom mediator + pipeline behaviors | Not started | ValidationBehavior → LoggingBehavior → Handler |
| Workflow CRUD (templates) | Not started | Create, read, update, delete |
| WorkflowState spawn | Not started | Instantiate template into live instance |
| WorkflowTask execution | Not started | Assign, complete, decline |
| Mid-process editing | Not started | Re-assign, reorder, add/remove on live workflows |
| Audit trail | Not started | Write entry on every state mutation |
| React Flow builder UI | Not started | Drag-drop canvas |
| Template library UI | Not started | Browse, clone, manage |
| My Tasks view | Not started | Personal task queue |
| Active Workflows board | Not started | Live board of running instances |

### Phase 2 — Auth, Notifications, Approvals

| Feature | Status | Notes |
|---|---|---|
| Email + Password auth | Not started | |
| Google OAuth | Not started | |
| Email OTP | Not started | 6-digit, 10min expiry |
| Password reset flow | Not started | Token-based, anti-enumeration |
| JWT + refresh tokens | Not started | 15min access, 7day refresh |
| Role-based route guards | Not started | |
| SMTP email via MailKit | Not started | |
| RabbitMQ event consumers | Not started | |
| SignalR in-app notifications | Not started | |
| Approval nodes | Not started | |
| External task tokens | Not started | |

### Phase 3 — Analytics, Calendar, Infrastructure

| Feature | Status | Notes |
|---|---|---|
| Analytics dashboard | Not started | Recharts |
| Calendar view | Not started | |
| Google Calendar sync | Not started | |
| Microsoft Outlook sync | Not started | |
| Triggered/scheduled workflows | Not started | |
| Group workspaces | Not started | |
| Proxmox + Docker hosting | Not started | |
| Terraform infra-as-code | Not started | |

---

## Decision Log

Record key decisions here as they're made. Agents read this to avoid re-litigating settled choices.

| Date | Decision | Rationale |
|---|---|---|
| Pre-build | Custom mediator, not MediatR | Hand-rolled — full understanding, no black-box |
| Pre-build | Result pattern, not exceptions | Explicit error handling, predictable return types |
| Pre-build | PostgreSQL only | One DB engine, no SQL Server |
| Pre-build | MailKit + SMTP, not Postmark/SendGrid | Free, self-controlled |
| Pre-build | React Flow for canvas | Best-in-class node editor for React |
| Pre-build | Redux for auth/UI only, React Query for data | Clean state separation |
| Pre-build | RabbitMQ raw client, no MassTransit | Lean, consistent |
| Pre-build | Proxmox self-hosted, not Azure | Free, full control |
| Pre-build | Template / Instance separation | Templates are blueprints. Instances are executions. Never conflate. |
| Pre-build | shadcn/ui New York style | Design system foundation |
| March 2026 | Google Stitch for UI design exploration | Free, exports DESIGN.md, MCP integration with Claude Code |
| March 2026 | Sub-agents via Claude Code | Replaces manual conversation switching |
| March 2026 | Skills for domain knowledge | Persistent context without re-pasting CLAUDE.md |
| March 2026 | GitHub MCP + PostgreSQL MCP as core integrations | Eliminates copy-paste during build sessions |
| March 2026 | Playwright MCP for E2E testing | Claude Code can drive the browser — exploratory + regression testing in same session |

---

## How Samuel Works Day to Day

### Starting a feature
```
1. Open Claude Code in the stackflow/ directory
2. Say: "Brief: {feature name}"
3. Claude Code invokes feature-provider → get Feature Brief
4. Review and approve the brief
5. Say: "Build this"
6. Claude Code invokes backend-agent and frontend-agent
7. When both complete: say "Review this"
8. Claude Code invokes pr-reviewer
9. If approved: say "Document this" and "Write tests"
10. Test manually
11. Say "Update tracker: {feature} is done"
```

### When something breaks
```
1. Say: "Debug: {description of the problem}"
   OR: "Review this error: {paste stack trace}"
2. Claude Code invokes debug-edit-agent
3. Agent diagnoses, produces fix or Change Brief
4. If fix > 3 files: routes back through pr-reviewer
5. Say: "Write a regression test for this bug: {description}"
6. Claude Code invokes test-agent
```

### When direction changes
```
1. Say: "Pivot: {what's changing and why}"
2. Claude Code invokes debug-edit-agent
3. Agent produces Pivot Brief with full impact assessment
4. Samuel reviews and approves the pivot
5. Agents update in the order specified in the Pivot Brief
```

### When you want to know where things stand
```
Say: "What's done?" → Claude Code invokes feature-provider → phase tracker status
```

---

## What Each Tool Is For

| Tool | When to use it |
|---|---|
| **Claude Code** | Everything development related. Orchestrates agents, runs code, manages files. |
| **Claude Pro** | Strategic thinking. Reviewing outputs before you commit. Planning sessions (like page inventory). CLAUDE.md updates. |
| **Claude Cowork** | Moving files to the right folders. Organising Docs Agent outputs. Managing the phase tracker document. Non-dev task automation. |
| **Google Stitch** | UI design exploration and prototyping. Generating screen designs and DESIGN.md. Before Frontend Agent builds anything. |

---

## Credentials & Secrets Checklist

| Credential | Where to get it | Status |
|---|---|---|
| GitHub PAT (`repo`, `issues`, `PRs`, `metadata`, `workflows` scopes) | github.com → Settings → Developer Settings → Fine-grained PATs | Not created |
| PostgreSQL URL | Already in `.env.example` | Have it |
| Sentry auth token | sentry.io → Settings → Auth Tokens | Not created (Phase 2) |
| Stitch MCP auth | stitch.withgoogle.com → Google OAuth | Not set up (Design phase) |
| Playwright | None — connects to localhost:3000 | Not set up (before Real Testing) |

---

## Files That Agents Must Read Before Working

| Agent | Must read before starting |
|---|---|
| All agents | CLAUDE.md |
| feature-provider | STACKFLOWSCOPE.md (for current phase tracker) |
| backend-agent | CLAUDE.md + Feature Brief + API Contract |
| frontend-agent | CLAUDE.md + Feature Brief + design-reference/DESIGN.md |
| pr-reviewer | CLAUDE.md + Feature Brief + all changed files |
| docs-agent | CLAUDE.md + PR sign-off + key implementation files |
| test-agent | CLAUDE.md + PR sign-off + handler + validator files |
| debug-edit-agent | CLAUDE.md + relevant agent files + broken code / error |

---

## The Rule Samuel Enforces

> **Nothing moves forward without his explicit go-ahead.**

1. Feature Provider produces a brief → Samuel approves it before builders start
2. Builders complete → Samuel reviews completion summaries before PR review
3. PR Reviewer approves → Samuel confirms before Docs + Test start
4. Docs + Test complete → Samuel tests manually before marking done
5. Bug found → Samuel activates debug-edit-agent, reviews the fix
6. Pivot triggered → Samuel approves the Pivot Brief before anything changes
7. CLAUDE.md changes → only Samuel decides what goes in

Claude Code orchestrates. Agents build. Samuel decides.