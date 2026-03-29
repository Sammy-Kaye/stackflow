---
name: stackflow-design
description: >
  Load StackFlow visual design system and screen references. Loaded once at
  session start by frontend-agent before writing any component. Do not
  auto-load — load explicitly and once per session.
allowed-tools: Read
---

<!--
  WHY THIS SKILL EXISTS
  ──────────────────────
  Without a design reference, the Frontend Agent makes visual decisions
  independently on every session — colours shift, spacing drifts, component
  choices become inconsistent. The result is a UI that looks like it was
  built by several different people.

  This skill is the single source of truth for what StackFlow looks and
  feels like. Every component, every page, every spacing decision traces
  back to the approved Stitch designs loaded here.

  THE READ-ONLY CONTRACT:
  The design-reference/ folder is READ ONLY. The HTML files exported from
  Stitch are visual specifications — not implementation files. The Frontend
  Agent reads them, understands the visual intent, then rebuilds properly
  in React using shadcn/ui components. It never copies Stitch HTML into the
  codebase. The architecture is always correct React — the visual is always
  from the approved designs.

  WHEN TO POPULATE THIS SKILL:
  This skill has two layers:
    Layer 1 (this file) — design principles and structure, available now
    Layer 2 (DESIGN.md) — exported from Stitch, added after the design session

  Until the Stitch design session is complete, build to Layer 1 standards.
  Once DESIGN.md is exported, populate Section 3 of this file with the
  design tokens and reference it from every component build.

  IF CLAUDE CODE CEASES TO EXIST:
  A developer can follow the design principles and screen reference locations
  below to produce UI consistent with the approved StackFlow design language.
  Open the reference HTML for the screen being built, then implement in React
  using shadcn/ui New York style to match the visual intent.
-->

# StackFlow — Design System

---

## Section 1 — Design Principles (Always Active)

These principles apply to every screen, every component, every state.
They do not change after the Stitch session. They are the foundation.

### The Core Design Intent

> **The user should feel calm — everything organised, clear, and never overwhelming.**

StackFlow users are managing live workflows. They are often checking statuses, acting
on tasks, or reviewing what happened. The UI must support fast scanning and confident
action — not create cognitive load.

Reference tone: **Linear** or **Notion** crossed with a process tool.

### Tone & Mood

- Dark mode as default — calm, focused, professional
- Deep teal as primary accent — used intentionally, not decoratively
- Clean sans-serif typography — Inter or equivalent
- Data-dense where needed — but never cluttered
- White space is structural, not decorative — it separates logical groups

### What "Calm" Means in Practice

```
✅ Consistent spacing scale — elements align, nothing feels randomly placed
✅ Muted grays for secondary content — hierarchy is visible at a glance
✅ Status communicated by colour AND shape — not colour alone
✅ Loading states that feel intentional — skeletons, not blank space
✅ Destructive actions separated visually — red, confirmation required
✅ Empty states that explain what to do — not just "nothing here"

❌ Bright competing colours
❌ Elements that shift layout when loading
❌ Modals opening modals
❌ Unlabelled icon-only buttons (except standard universal icons)
❌ More than 3 levels of visual hierarchy on one screen
```

---

## Section 2 — Component Library (Always Active)

### Library: shadcn/ui — New York Style

**Always extend shadcn/ui primitives. Never rebuild from scratch.**

```
Component source: shadcn/ui (New York style variant)
Icon library: Lucide React
```

The New York style uses slightly tighter spacing and crisper borders than the default
shadcn/ui variant. It suits a data-dense SaaS layout well.

### Core Component Mapping

| UI Element | shadcn/ui Component | Notes |
|---|---|---|
| Cards | `Card`, `CardHeader`, `CardContent`, `CardTitle` | Use for workflow cards, task cards |
| Status labels | `Badge` | Variant per status — see Status Colours below |
| Buttons | `Button` | variant: default, secondary, destructive, ghost, outline |
| Form fields | `Input`, `Textarea`, `Select` | Always inside `Form` from react-hook-form |
| Modals | `Dialog` | Not `AlertDialog` — that's for destructive confirmation only |
| Destructive confirm | `AlertDialog` | Required for delete, cancel, revoke actions |
| Notifications | Sonner `toast()` | Not shadcn/ui Toast — Sonner for all toasts |
| Dropdown menus | `DropdownMenu` | For action menus on cards, table rows |
| Tooltips | `Tooltip` | For icon-only buttons and truncated text |
| Data tables | `Table` | With sortable headers where applicable |
| Page navigation | `Breadcrumb` | For nested routes |
| Sidebar | `Sheet` (mobile), custom (desktop) | Notification centre uses Sheet |

### Status Colours

These Badge variants map to StackFlow statuses. Use consistently across all screens.

```typescript
// WorkflowStatus badges
const workflowStatusVariant = {
  InProgress:  'default',    // teal/primary
  Completed:   'success',    // green
  Cancelled:   'destructive', // red
} as const;

// WorkflowTaskStatus badges
const taskStatusVariant = {
  Pending:    'secondary',   // muted gray
  InProgress: 'default',     // teal/primary
  Completed:  'success',     // green
  Declined:   'destructive', // red
  Expired:    'warning',     // amber
  Skipped:    'outline',     // subtle
} as const;

// Priority badges
const priorityVariant = {
  Low:      'secondary',
  Medium:   'default',
  High:     'warning',    // amber
  Critical: 'destructive', // red
} as const;
```

### Node Type Colours (Workflow Builder Canvas)

```
Task          → slate/gray
Approval      → purple
Condition     → teal (primary accent)
Deadline      → amber
Notification  → blue
ExternalStep  → coral/orange
```

---

## Section 3 — Stitch Design Reference

<!--
  ════════════════════════════════════════════════════════
  POPULATE THIS SECTION AFTER THE STITCH DESIGN SESSION
  ════════════════════════════════════════════════════════

  Once the Stitch design session is complete and DESIGN.md is exported:

  1. Place DESIGN.md in: web-frontend/src/design-reference/DESIGN.md
  2. Place HTML archives in their subfolders (see structure below)
  3. Replace the placeholder text in this section with:
     - Confirmed colour tokens from DESIGN.md
     - Confirmed typography scale
     - Any component decisions made during the design session
     - Any deviations from the default shadcn/ui New York style

  Until this section is populated, build to Section 1 and 2 standards.
  ════════════════════════════════════════════════════════
-->

### Design Reference Location

```
web-frontend/src/design-reference/         ← READ ONLY — never modify these files
├── DESIGN.md                              ← Master design system export from Stitch
├── landing/
│   └── index.html                         ← Landing page visual reference
├── auth/
│   ├── login.html
│   ├── register.html
│   ├── otp.html
│   └── reset-password.html
├── dashboard/
│   └── my-tasks.html
├── workflows/
│   ├── builder.html                       ← Workflow builder canvas
│   ├── board.html                         ← Active workflows board
│   └── templates.html                     ← Template library
├── admin/
│   └── panel.html
└── notifications/
    └── centre.html
```

### How to Use Screen References

Before building any page or complex component:

1. **Open the corresponding `.html` file** for the screen you are building
2. **Read it as a visual brief** — understand the layout, hierarchy, spacing intent
3. **Do not copy-paste the HTML** — it is Stitch output, not React
4. **Build React components** using shadcn/ui that match the visual intent
5. **If the Stitch design conflicts** with shadcn/ui patterns — flag to Samuel

### Screen Reference Map

| Screen | Reference file | Key components to match |
|---|---|---|
| Landing page | `landing/index.html` | Hero, features grid, pricing tiers, CTA |
| Login | `auth/login.html` | Centred card, Google OAuth button, show/hide password |
| Register | `auth/register.html` | Same card style as login |
| OTP entry | `auth/otp.html` | 6-digit input blocks, countdown timer |
| Password reset | `auth/reset-password.html` | New password + confirm, show/hide |
| My Tasks dashboard | `dashboard/my-tasks.html` | Task list, filters, priority indicators |
| Workflow builder | `workflows/builder.html` | Canvas, node palette sidebar, toolbar |
| Active workflows board | `workflows/board.html` | Kanban-style cards, status grouping |
| Template library | `workflows/templates.html` | Card grid, search, category filter |
| Admin panel | `admin/panel.html` | User management, workspace settings |
| Notification centre | `notifications/centre.html` | Slide-out panel, notification list |

### Design Tokens (Populate after Stitch session)

```
Background:       [populate from DESIGN.md]
Surface:          [populate from DESIGN.md]
Surface elevated: [populate from DESIGN.md]
Border:           [populate from DESIGN.md]
Primary accent:   [populate from DESIGN.md — expected: deep teal]
Text primary:     [populate from DESIGN.md]
Text secondary:   [populate from DESIGN.md]
Text muted:       [populate from DESIGN.md]
Success:          [populate from DESIGN.md]
Warning:          [populate from DESIGN.md]
Error/Destructive:[populate from DESIGN.md]
```

---

## Section 4 — Layout Conventions (Always Active)

### Page Layout Structure

```
┌─────────────────────────────────────────────────┐
│  App Shell                                       │
│  ┌────────┬───────────────────────────────────┐  │
│  │        │  Page Header                      │  │
│  │        │  (title + primary action button)  │  │
│  │  Side  ├───────────────────────────────────┤  │
│  │  bar   │  Page Content                     │  │
│  │        │  (cards, tables, canvas)           │  │
│  │        │                                   │  │
│  └────────┴───────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

### Consistent Patterns

**Every page must have:**
- A clear page title (H1, consistent size)
- One primary action button (create, add, start) — top right
- Consistent padding: `p-6` on desktop, `p-4` on mobile

**Every data list must have:**
- A loading skeleton matching the list shape
- An empty state with explanatory text and a call-to-action
- An error state with a retry option or message

**Every form must have:**
- Labels above inputs — never placeholder-only
- Visible required field indicators
- Submit button disabled while submitting
- Validation messages below the field, not in a separate banner

---

## What You Must Never Do

- Copy-paste HTML from Stitch `.html` files into React components
- Use inline styles — always Tailwind classes
- Create new colour values outside the design token system
- Build a custom primitive that shadcn/ui already provides
- Use `any` type on design-related props — always type component props explicitly
- Ignore the screen reference file when building a screen it covers
- Override shadcn/ui New York style defaults without flagging to Samuel
