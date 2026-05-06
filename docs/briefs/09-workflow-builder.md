# Feature Brief: Workflow Builder UI
Phase: 1
Feature: #9
Status: Ready for implementation
Brief date: 2026-05-06

---

## What this feature does (plain English)

The Workflow Builder is the drag-and-drop canvas where an admin creates and edits workflow
templates. The user drags node types (Task, Approval, Condition, Notification) from a left
palette onto a canvas, connects nodes with edges to form a sequence, and fills in each
node's properties in a right-hand panel. When ready, the user saves the template as a
draft or publishes it so it can be spawned into live instances. This feature replaces the
stub `WorkflowBuilderPage.tsx` that ships with Feature 6. It is frontend-only — the
backend CRUD API (`POST`, `GET`, `PUT /api/workflows`) was delivered in Feature 8 and is
consumed unchanged here.

---

## Scope — what IS in this brief

- Replace the `WorkflowBuilderPage.tsx` stub with the full builder at both
  `/workflows/new` (create mode) and `/workflows/:id/edit` (edit mode).
- Three-panel layout: left palette (node types), centre React Flow canvas, right properties
  panel (opens on node select, closes on canvas click).
- Top bar: back navigation, inline workflow name input, save/publish controls, auto-save
  indicator, and (edit mode only) an overflow menu with Delete.
- Four active node types: Task, Approval, Condition, Notification.
  ExternalStep and Deadline nodes appear in the palette but are disabled with a Phase 2
  tooltip — they cannot be dropped.
- StartNode and EndNode on the canvas: always present, fixed in place, not deletable.
- Custom styled node components for every type, each with distinct colour treatment.
- Right properties panel: fields vary by node type. All changes update React Flow state
  immediately (controlled, no submit button on the panel).
- Edges: animated, selectable, deletable via the Delete key.
- Drag a node type from the palette to the canvas to create a node at the drop position.
- Connect two nodes by dragging from one node's output handle to another node's input
  handle.
- Condition node has two named output handles: "Yes" (right) and "No" (bottom), enabling
  if/else branches.
- Inline editable workflow name in the top bar (click to edit, Enter or blur to confirm).
  Default value for new workflows: "Untitled workflow".
- `Save draft` button (create mode) / `Save changes` button (edit mode):
  serialises canvas state to `CreateWorkflowDto` / `UpdateWorkflowDto` and calls
  `POST /api/workflows` or `PUT /api/workflows/{id}` respectively.
- `Publish` button: saves first, then opens a confirmation `AlertDialog`, then calls
  `PUT /api/workflows/{id}` with `isActive: true`, then navigates to `/active`.
- Auto-save: debounced 30 seconds after any canvas change. Shows "Saving..." then "Saved"
  in the top bar.
- Unsaved changes guard: browser `beforeunload` event fires a native confirmation dialog
  when there are unsaved changes.
- Back button (`← Workflows`) navigates to `/workflows`. If there are unsaved changes, the
  user sees the unsaved changes guard first.
- Canvas controls (bottom-right): zoom in, zoom out, fit-to-screen (React Flow built-ins),
  mini-map toggle.
- Edit mode loading: fetches the existing workflow via `useWorkflow(id)`, populates the
  canvas from the tasks array.
- Edit mode amber banner when `isActive: true`: "This workflow is currently live. Changes
  will apply to new instances only — running instances are not affected."
- Edit mode overflow menu (`···`, Admin only): "Delete workflow" → `AlertDialog` →
  `useDeleteWorkflow()` → navigate to `/workflows`.
- First-use hint banner (shown once, dismissed with "Got it", persisted in `localStorage`
  under key `stackflow:builder-hint-dismissed`): "New to the builder? Start with a Task
  node — drag it onto the canvas and connect it to Start."
- Node position: stored in React Flow node `position` (x, y). On save, `orderIndex` is
  derived from the top-to-bottom visual order of nodes on the canvas (sorted by `y`
  position ascending, after excluding StartNode and EndNode).
- Load positions: when loading an existing workflow, nodes are laid out automatically in a
  linear vertical stack centred horizontally. `OrderIndex` drives the vertical order.
  No stored x/y positions in Phase 1.
- All hooks and service methods from Feature 8 (`useWorkflow`, `useCreateWorkflow`,
  `useUpdateWorkflow`, `useDeleteWorkflow`, `workflowService`) are reused — no new service
  or hook files needed.

---

## Scope — what is NOT in this brief

- Real authentication or role enforcement beyond what already exists in the router guard
  (`AdminRoute` protects both builder routes already).
- Undo/redo — explicitly out of scope until Phase 2.
- Saving node x/y positions to the database — Phase 1 uses automatic linear layout on load;
  the visual canvas positions are ephemeral.
- ExternalStep and Deadline node implementation — nodes appear disabled in palette only.
- Parallel branch support beyond a single Condition if/else — Phase 2.
- External contributor token flow — Phase 2.
- File attachment fields on task nodes — Phase 2.
- WorkflowState spawning from this page — that is Feature 11; the Publish button routes
  to `/active`, it does not spawn an instance.
- Category field: the workflow may have a category (field exists in `UpdateWorkflowDto`),
  but the builder does not expose a category input in Phase 1 — it is left as `null`.
- Workspace users dropdown in the properties panel (Assigned to field): Phase 1 uses a
  plain text input for `defaultAssignedToEmail`; user picker is Phase 2.
- Template Library integration — Feature 10 handles pre-populating the builder from a
  template; this brief covers the builder in isolation.
- Tests — handled separately by the Test Agent after PR approval.
- Any changes to the router (`router/index.tsx`) — routes already point to
  `WorkflowBuilderPage` and require no modification.
- Any changes to existing hooks or service files from Feature 8.

---

## Domain entities involved

- `Workflow` — read on load (`GET /api/workflows/{id}`), created on first save
  (`POST /api/workflows`), updated on subsequent saves (`PUT /api/workflows/{id}`).
  Fields used: `id`, `name`, `description`, `category`, `isActive`, `tasks`.
- `WorkflowTask` — every node on the canvas (except StartNode and EndNode) maps to one
  `WorkflowTask`. Fields used: `title`, `description`, `assigneeType`,
  `defaultAssignedToEmail`, `orderIndex`, `dueAtOffsetDays`, `nodeType`, `conditionConfig`,
  `parentTaskId`.
- No new domain entities or fields introduced.

---

## API Contract

This feature is frontend-only. All endpoints are defined in the Feature 8 brief and
API contract (`docs/briefs/08-workflow-crud.md`). The builder consumes three of the
five endpoints.

#### GET /api/workflows/{id}
Auth: JWT required

Used in edit mode to load the existing workflow and its task list.

Response 200:
```json
{
  "id": "string UUID",
  "name": "string",
  "description": "string | null",
  "category": "string | null",
  "workspaceId": "string UUID",
  "isActive": "boolean",
  "createdAt": "string ISO 8601",
  "updatedAt": "string ISO 8601",
  "tasks": [
    {
      "id": "string UUID",
      "workflowId": "string UUID",
      "title": "string",
      "description": "string | null",
      "assigneeType": "string (Internal | External)",
      "defaultAssignedToEmail": "string | null",
      "orderIndex": "number",
      "dueAtOffsetDays": "number | null",
      "nodeType": "string (Task | Approval | Condition | Notification | ExternalStep | Deadline)",
      "conditionConfig": "string | null",
      "parentTaskId": "string UUID | null"
    }
  ]
}
```

Response 401: { "error": "Unauthorised" }
Response 404: { "error": "Workflow not found" }

---

#### POST /api/workflows
Auth: JWT required

Used in create mode on first save ("Save draft").

Request body:
```json
{
  "name": "string (required, max 200)",
  "description": "string | null",
  "category": "string | null",
  "tasks": [
    {
      "title": "string (required, max 200)",
      "description": "string | null",
      "assigneeType": "string (Internal | External)",
      "defaultAssignedToEmail": "string | null",
      "orderIndex": "number",
      "dueAtOffsetDays": "number | null",
      "nodeType": "string (Task | Approval | Condition | Notification | ExternalStep | Deadline)",
      "conditionConfig": "string | null",
      "parentTaskId": "string UUID | null"
    }
  ]
}
```

Response 201: full `WorkflowDto` (same shape as GET /api/workflows/{id} response above).

Response 400: { "error": "string" }
Response 401: { "error": "Unauthorised" }

Builder behaviour after 201: stores the returned `id` in component state so that all
subsequent saves in the same session use `PUT /api/workflows/{id}` instead of `POST`.

---

#### PUT /api/workflows/{id}
Auth: JWT required

Used on every save after the first (edit mode, or after the first POST in create mode).
Also used by the Publish action with `isActive: true`.

Request body:
```json
{
  "name": "string (required, max 200)",
  "description": "string | null",
  "category": "string | null",
  "isActive": "boolean",
  "tasks": [
    {
      "title": "string (required, max 200)",
      "description": "string | null",
      "assigneeType": "string (Internal | External)",
      "defaultAssignedToEmail": "string | null",
      "orderIndex": "number",
      "dueAtOffsetDays": "number | null",
      "nodeType": "string (Task | Approval | Condition | Notification | ExternalStep | Deadline)",
      "conditionConfig": "string | null",
      "parentTaskId": "string UUID | null"
    }
  ]
}
```

Response 200: full `WorkflowDto`.

Response 400: { "error": "string" }
Response 401: { "error": "Unauthorised" }
Response 404: { "error": "Workflow not found" }

---

#### DELETE /api/workflows/{id}
Auth: JWT required, Admin role required

Used by the `···` overflow menu "Delete workflow" action in edit mode.

Response 204: empty body.

Response 400: { "error": "string" }
Response 401: { "error": "Unauthorised" }
Response 403: (no body)
Response 404: { "error": "Workflow not found" }

---

## Frontend routes and views

No new routes. Both routes already exist in `router/index.tsx` and point to
`WorkflowBuilderPage`. This feature replaces the page's internals only.

```
/workflows/new        → WorkflowBuilderPage (create mode — no :id param)
/workflows/:id/edit   → WorkflowBuilderPage (edit mode — :id param present)
```

### Component breakdown

All new files live under `src/modules/workflows/ui/`.

```
src/modules/workflows/ui/
  pages/
    WorkflowBuilderPage.tsx         ← Replace stub. Top-level page component.
                                       Reads :id param to determine create vs edit mode.
                                       Owns all React Flow state (nodes, edges).
                                       Owns save logic (POST vs PUT decision).
                                       Renders the three-panel layout.

  components/
    builder/
      BuilderTopBar.tsx             ← Back button, workflow name input, save/publish
                                       buttons, auto-save status indicator, overflow menu.
                                       Receives: workflowName, isDirty, isSaving,
                                       isPublished, onNameChange, onSave, onPublish,
                                       onDelete (edit mode only).

      NodePalette.tsx               ← Left panel. Fixed 120px wide. Vertical list of
                                       draggable node type items. Each item is draggable
                                       via HTML5 drag-and-drop (onDragStart sets
                                       dataTransfer type to 'application/reactflow' and
                                       the node type string as data). Phase 2 items shown
                                       disabled with tooltip.

      PropertiesPanel.tsx           ← Right panel. Fixed 280px wide. Conditionally
                                       rendered when a node is selected. Renders
                                       TaskProperties, ApprovalProperties,
                                       ConditionProperties, or NotificationProperties
                                       based on selected node type. Closes on canvas
                                       deselect.

      TaskProperties.tsx            ← Form fields for Task and Approval nodes:
                                       Title (required), Description (optional textarea),
                                       Assigned to email (text input, optional),
                                       Due offset (number input, optional). All inputs are
                                       controlled — onChange fires immediately into
                                       React Flow node data via setNodes.

      ConditionProperties.tsx       ← Form fields for Condition nodes:
                                       Yes label (text input, default "Yes"),
                                       No label (text input, default "No").
                                       Controlled, same pattern as TaskProperties.

      NotificationProperties.tsx    ← Form fields for Notification nodes:
                                       Title (required text input),
                                       Message (required textarea).
                                       Controlled.

      FirstUseHint.tsx              ← Dismissible amber banner shown at the top of the
                                       canvas on first visit. "Got it" button sets
                                       localStorage key and hides it.

    nodes/
      TaskNode.tsx                  ← React Flow custom node. White card, blue left
                                       border (4px). Shows: title, assignee email (if
                                       set), due offset badge (e.g. "+3 days"). Single
                                       target handle (top), single source handle (bottom).

      ApprovalNode.tsx              ← White card, purple left border. Shows: title,
                                       "Requires approval" label, approver email (if set).
                                       Same handle layout as TaskNode.

      ConditionNode.tsx             ← Diamond shape (rotated square div). Amber border.
                                       Shows: "If / Else" label, condition label (from
                                       node data). One target handle (top). Two source
                                       handles: "Yes" (right, labelled), "No" (bottom,
                                       labelled).

      NotificationNode.tsx          ← White card, teal left border. Shows: title,
                                       "Notification" label. Same handle layout as
                                       TaskNode.

      StartNode.tsx                 ← Pill/oval shape, green fill. Label: "Start".
                                       Source handle only (bottom). Not deletable, not
                                       draggable (enforced via node `draggable: false`
                                       and omitting it from selectable nodes). Position
                                       fixed at { x: 360, y: 40 } on mount.

      EndNode.tsx                   ← Pill/oval shape, grey fill. Label: "End".
                                       Target handle only (top). Not deletable, not
                                       draggable. Positioned below the last task node
                                       on mount (y = last node y + 160).
```

### React Flow canvas configuration

```
nodeTypes registered:
  task         → TaskNode
  approval     → ApprovalNode
  condition    → ConditionNode
  notification → NotificationNode
  start        → StartNode
  end          → EndNode

edgeOptions:
  type: 'smoothstep'
  animated: true
  deletable: true

Canvas props:
  fitView: true (on load only)
  deleteKeyCode: 'Delete'
  onDrop: handler to create node at drop position
  onDragOver: preventDefault to enable drop
  onNodeClick: sets selectedNodeId in state
  onPaneClick: clears selectedNodeId
  onNodesChange: standard React Flow handler
  onEdgesChange: standard React Flow handler
  onConnect: standard React Flow handler (adds edge)
  onNodesDelete: filters out start/end node deletions (guard)
```

### Node-to-task serialisation

When saving, the builder converts React Flow nodes into the `tasks` array for the API
request body. The mapping is:

```
React Flow node → CreateWorkflowTaskDto
  node.data.title              → title
  node.data.description        → description
  node.data.assigneeType       → assigneeType (default: 'Internal')
  node.data.assigneeEmail      → defaultAssignedToEmail
  node.data.dueAtOffsetDays    → dueAtOffsetDays
  node.type (string)           → nodeType (capitalised: 'task' → 'Task', etc.)
  node.data.conditionConfig    → conditionConfig (JSON string for Condition nodes)
  node.data.parentTaskId       → parentTaskId
  derived from sorted y-pos    → orderIndex (0-based, StartNode and EndNode excluded)
```

StartNode and EndNode are never included in the `tasks` array sent to the API.

### Canvas-to-node population (edit mode load)

When loading an existing workflow from the API, the builder converts tasks into React Flow
nodes using this layout algorithm:

```
StartNode:        position { x: 360, y: 40 }
Task nodes:       position { x: 320, y: 40 + (orderIndex + 1) * 160 }
EndNode:          position { x: 360, y: 40 + (tasks.length + 1) * 160 }
```

Edges are not stored in Phase 1 — when loading, the builder auto-connects nodes in
orderIndex sequence: Start → task[0] → task[1] → ... → task[n] → End. Condition node
edges cannot be auto-reconstructed from orderIndex alone in Phase 1 — they are connected
Start-to-End linearly the same as other nodes. The user re-draws branches manually if
needed.

---

## RabbitMQ events (if any)

None for this feature.

---

## SignalR events (if any)

None for this feature.

---

## Audit requirements

No audit entries required for this feature. Template CRUD (creating and editing workflow
blueprints) is not audited. The audit trail is for WorkflowState and WorkflowTaskState
mutations only (Feature 12).

---

## Acceptance criteria

1. Given the user navigates to `/workflows/new`, when the page loads, then the builder
   renders with a StartNode and EndNode on the canvas, an empty name input reading
   "Untitled workflow", and the left palette visible with four draggable node types.

2. Given the user is on `/workflows/new`, when they drag a Task node from the palette
   and drop it onto the canvas, then a new TaskNode appears at the drop position.

3. Given a TaskNode exists on the canvas, when the user clicks it, then the right
   properties panel opens showing Title, Description, Assigned to, and Due offset fields.

4. Given the properties panel is open for a TaskNode, when the user types in the Title
   field, then the node label on the canvas updates in real time without any save action.

5. Given the user has connected StartNode to a TaskNode and TaskNode to EndNode, when they
   click "Save draft", then `POST /api/workflows` is called with the correct name and a
   tasks array containing one entry for the TaskNode, and the top bar shows "Saved".

6. Given the user has saved once and a workflow ID exists, when they click "Save draft"
   again, then `PUT /api/workflows/{id}` is called (not POST), and "Saved" is shown.

7. Given the user clicks "Publish", when they confirm the dialog, then
   `PUT /api/workflows/{id}` is called with `isActive: true` and the user is navigated
   to `/active`.

8. Given the user navigates to `/workflows/:id/edit` for a workflow with three tasks,
   when the page loads, then the canvas shows StartNode, three task nodes in vertical
   sequence, and EndNode, all connected with edges.

9. Given the loaded workflow has `isActive: true`, when the builder renders in edit mode,
   then an amber banner "This workflow is currently live. Changes will apply to new
   instances only — running instances are not affected." is visible.

10. Given the user is in edit mode, when they open the `···` overflow menu and click
    "Delete workflow", then a confirmation AlertDialog appears, and on confirm,
    `useDeleteWorkflow()` is called and the user is navigated to `/workflows`.

11. Given the user has made changes without saving, when they click the browser back button
    or close the tab, then the browser fires a native "leave site?" confirmation dialog.

12. Given the user has made changes without saving, when they click "← Workflows" in the
    top bar, then they are prompted about unsaved changes before navigation proceeds.

13. Given the user clicks the canvas background (not a node), when a node was previously
    selected, then the properties panel closes.

14. Given the user selects an edge on the canvas and presses the Delete key, then the edge
    is removed.

15. Given the user drags a ConditionNode from the palette, when the node renders on the
    canvas, then it has a diamond shape with two output handles labelled "Yes" and "No".

16. Given the user drags an ExternalStep node type from the palette, when they attempt to
    drop it, then the node is not created and the palette item shows a "Phase 2" tooltip.

17. Given the page has not been visited before, when the user opens the builder for the
    first time, then the first-use hint banner is visible at the top of the canvas.

18. Given the first-use hint is visible, when the user clicks "Got it", then the banner
    disappears and does not appear on subsequent visits (persisted in localStorage).

19. Given the user makes a canvas change and waits 30 seconds (auto-save trigger), when
    the auto-save fires, then the top bar shows "Saving..." then "Saved" and the workflow
    is persisted via the API.

20. Given the user clicks "Fit to screen" in the canvas controls, when the click fires,
    then React Flow's fitView function centres all nodes in the viewport.

---

## Agent instructions

**Frontend Agent — ordered build sequence:**

The backend API is already live from Feature 8. All five endpoints are available at
`http://localhost:5000/api/workflows`. The existing hooks and service are in
`src/modules/workflows/`. Do not modify them.

1. Install `reactflow` if not already in `package.json`. Run `npm install reactflow`.
   Import base styles in the builder page: `import 'reactflow/dist/style.css'`.

2. Write the six custom node components in `src/modules/workflows/ui/components/nodes/`:
   `TaskNode.tsx`, `ApprovalNode.tsx`, `ConditionNode.tsx`, `NotificationNode.tsx`,
   `StartNode.tsx`, `EndNode.tsx`.
   Each node uses shadcn/ui base-nova tokens for colour (no raw hex values).
   Each node registers React Flow `Handle` components with correct `type` and `position`.

3. Write the properties panel sub-components:
   `TaskProperties.tsx`, `ConditionProperties.tsx`, `NotificationProperties.tsx`.
   Each receives `nodeData` and an `onChange(field, value)` callback. No form library
   needed — these are simple controlled inputs, not a submitted form.

4. Write `PropertiesPanel.tsx` — selects the correct sub-component based on node type,
   renders in the right panel slot, returns null when no node is selected.

5. Write `NodePalette.tsx` — left panel. Each draggable item sets
   `event.dataTransfer.setData('application/reactflow', nodeType)` on `dragStart`.
   ExternalStep and Deadline items render with `opacity-50 cursor-not-allowed` and a
   shadcn/ui `Tooltip` reading "Coming in Phase 2". They must also call
   `event.preventDefault()` on their dragStart to prevent dropping.

6. Write `BuilderTopBar.tsx` — receives all state as props from the page. Inline name
   input uses a controlled `input` element (not RHF — this is a single field, not a form).
   The auto-save indicator is a `span` with conditional text: "Saving...", "Saved",
   or "Unsaved changes". The `···` menu uses shadcn/ui `DropdownMenu`.

7. Write `FirstUseHint.tsx` — reads `localStorage.getItem('stackflow:builder-hint-dismissed')`
   on mount; if not set, renders the amber banner. "Got it" calls
   `localStorage.setItem('stackflow:builder-hint-dismissed', 'true')` and hides the banner.

8. Write `WorkflowBuilderPage.tsx` — this is the integration point. Steps:
   a. Read `:id` param from `useParams()`. If present: edit mode; if absent: create mode.
   b. In edit mode: call `useWorkflow(id)` and convert the tasks array to React Flow nodes
      and edges using the layout algorithm defined in this brief.
   c. Initialise React Flow state with `useNodesState` and `useEdgesState`.
   d. Set up `nodeTypes` map referencing all six custom node components.
   e. Wire `onDrop` and `onDragOver` handlers for palette drag-and-drop.
   f. Implement `saveWorkflow()` function: serialises nodes to tasks array (excluding
      StartNode/EndNode), determines POST vs PUT, calls the correct mutation, handles
      "Saved" / "Unsaved changes" status indicator.
   g. Implement `publishWorkflow()` function: calls `saveWorkflow()` first, then on
      success calls `PUT /api/workflows/{id}` with `isActive: true` via `useUpdateWorkflow`,
      then navigates to `/active`.
   h. Wire `beforeunload` event listener in a `useEffect` — adds listener when `isDirty`
      is true, removes it when false or on unmount.
   i. Render the three-panel layout using CSS grid or flex:
      `NodePalette` (120px fixed) | React Flow canvas (flex-grow) | `PropertiesPanel`
      (280px fixed, conditionally rendered).
   j. Render `BuilderTopBar` above the three panels.
   k. Render `FirstUseHint` inside the canvas area, above the React Flow component.
   l. Render the amber "live workflow" banner in edit mode when `workflow.isActive` is true.
   m. In create mode after first successful POST, store the returned workflow `id` in a
      `useRef` so subsequent saves call PUT without a route change.

9. Run `npm run build` (or `npx tsc --noEmit`) and fix all TypeScript errors before
   declaring done.

**Backend Agent:** Not required for this feature. The API is already built.

**Handoff point:** Not applicable — this is a frontend-only feature. The Frontend Agent
can begin immediately. The Feature 8 backend is the only dependency and it is already live.
