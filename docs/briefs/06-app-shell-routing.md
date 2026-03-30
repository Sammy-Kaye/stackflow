# Feature Brief: App Shell + Routing
Phase: 1
Status: Ready for implementation

---

## What this feature does (plain English)

This feature builds the structural chrome of the StackFlow frontend — the persistent
layout that every authenticated screen sits inside. After this feature ships, Samuel
can log in via the Dev Auth Stub, land in a full application shell with a sidebar and
top bar, and navigate between stub pages for the three main Phase 1 sections. Unauthenticated
visitors are redirected to the dev login page; authenticated visitors are redirected away
from the login page. The dev login flow (already scaffolded in Feature 2) is wired into
this shell so the end-to-end flow is testable immediately.

---

## Scope — what IS in this brief

- `AuthenticatedLayout` component: two-column layout with a collapsible sidebar on the
  left and a top bar across the top; all authenticated routes render inside this layout
- `UnauthenticatedLayout` component: full-screen centered card layout for future auth
  pages (login, register, etc.)
- `ProtectedRoute` guard: reads `accessToken` from Redux; redirects to `/dev-login` if
  null; renders children if present
- `GuestRoute` guard: reads `accessToken` from Redux; redirects to `/` if non-null
  (prevents a logged-in user hitting the login page); renders children if null
- Sidebar component with navigation links to: Workflows (`/workflows`), My Tasks
  (`/tasks`), Active Workflows (`/active`) — all link to stub pages for now
- Sidebar collapse toggle: clicking a button collapses the sidebar to icon-only width;
  state persisted in Redux (`uiSlice`)
- Top bar component: displays the workspace name (read from Redux auth state
  `workspaceId` — use the literal ID as placeholder until a workspace name API exists)
  and a user area showing the authenticated user's email and a logout button
- Logout action: clears Redux auth state (`clearCredentials`) and navigates to
  `/dev-login`; no API call required (Phase 1 — token is in-memory only)
- Stub pages: `WorkflowsPage`, `MyTasksPage`, `ActiveWorkflowsPage` — each renders a
  heading and a "coming soon" message; these are replaced in later features
- Updated router: root path `/` renders `AuthenticatedLayout` with nested routes;
  `/dev-login` retains its existing `DevRoute` + `GuestRoute` wrapping;
  unknown paths fall through to a simple `NotFoundPage`
- `uiSlice` Redux slice: one boolean field `sidebarOpen` (default `true`); two actions
  `setSidebarOpen(boolean)` and `toggleSidebar()`; registered in `store.ts`
- `NotFoundPage`: simple full-screen "404 — Page not found" with a link back to `/`

---

## Scope — what is NOT in this brief

- Real workspace names — the top bar shows the workspaceId string until a workspace
  name API exists (Feature 8+)
- User avatar images — text initials only; no image upload or Gravatar
- Notification bell or unread count — Phase 2 (SignalR)
- Breadcrumb navigation — belongs in the individual feature pages
- Any real content in the stub pages — they are placeholders only
- Mobile responsive layout — Phase 1 is desktop-only; no hamburger menu
- Keyboard shortcut to toggle sidebar
- Real authentication (JWT refresh, Google OAuth, OTP) — Phase 2

---

## Domain entities involved

No backend domain entities. This feature is purely a frontend structural concern.

The frontend reads the following fields already present in Redux `AuthState`:
- `accessToken` — presence/absence determines authenticated vs. unauthenticated state
- `email` — displayed in the top bar user area
- `workspaceId` — displayed in the top bar workspace label (as placeholder text)
- `role` — available for future use; not displayed in this feature

---

## API Contract

None for this feature.

The only backend interaction is the existing Dev Auth Stub already built in Feature 2:
- `POST /api/auth/dev-login` — called by the existing `useDevLogin` hook; no changes
- `GET /api/auth/me` — not called in this feature; available for future hydration

---

## Frontend routes and views

### Route tree (full updated router)

```
/                           → AuthenticatedLayout (ProtectedRoute)
  index                     → redirect to /workflows
  /workflows                → WorkflowsPage (stub)
  /tasks                    → MyTasksPage (stub)
  /active                   → ActiveWorkflowsPage (stub)

/dev-login                  → DevRoute → GuestRoute → DevLoginPage (existing)

*                           → NotFoundPage
```

### New files

**Layouts**
- `web-frontend/src/modules/shared/ui/layouts/AuthenticatedLayout.tsx`
- `web-frontend/src/modules/shared/ui/layouts/UnauthenticatedLayout.tsx`

**Layout sub-components** (used only by `AuthenticatedLayout`)
- `web-frontend/src/modules/shared/ui/components/Sidebar.tsx`
- `web-frontend/src/modules/shared/ui/components/TopBar.tsx`
- `web-frontend/src/modules/shared/ui/components/NavItem.tsx`

**Route guards** (new files alongside the existing `DevRoute`)
- `web-frontend/src/router/guards/ProtectedRoute.tsx`
- `web-frontend/src/router/guards/GuestRoute.tsx`

**Redux slice**
- `web-frontend/src/store/uiSlice.ts`

**Stub pages**
- `web-frontend/src/modules/workflows/ui/pages/WorkflowsPage.tsx`
- `web-frontend/src/modules/tasks/ui/pages/MyTasksPage.tsx`
- `web-frontend/src/modules/workflows/ui/pages/ActiveWorkflowsPage.tsx`

**Not-found page**
- `web-frontend/src/modules/shared/ui/pages/NotFoundPage.tsx`

### Modified files

- `web-frontend/src/router/index.tsx` — replace placeholder root route with the full
  nested route tree described above
- `web-frontend/src/store/store.ts` — register `uiReducer` from `uiSlice`

---

## RabbitMQ events (if any)

None for this feature.

---

## SignalR events (if any)

None for this feature.

---

## Audit requirements

None for this feature. No server-side state is mutated.

---

## Acceptance criteria

1. Given the user is not authenticated (no token in Redux), when they navigate to `/`,
   then they are redirected to `/dev-login` and the authenticated layout is not rendered.

2. Given the user is not authenticated, when they navigate to `/workflows`, then they
   are redirected to `/dev-login`.

3. Given the user is not authenticated, when they navigate to `/dev-login`, then the
   `DevLoginPage` is rendered (not redirected away).

4. Given the user is authenticated (token present in Redux), when they navigate to
   `/dev-login`, then they are redirected to `/` (which itself redirects to `/workflows`).

5. Given the user is authenticated, when they navigate to `/`, then they are
   redirected to `/workflows` and the `AuthenticatedLayout` is rendered with the sidebar
   and top bar visible.

6. Given the user is authenticated and on `/workflows`, when they click "My Tasks" in
   the sidebar, then the URL changes to `/tasks` and `MyTasksPage` renders inside the
   existing layout without a full page reload.

7. Given the user is authenticated and on any authenticated route, when they click the
   sidebar collapse toggle, then the sidebar collapses to icon-only width, and the
   `sidebarOpen` value in Redux changes to `false`.

8. Given the sidebar is collapsed (`sidebarOpen` is `false`), when the user clicks the
   expand toggle, then the sidebar returns to full width and `sidebarOpen` is `true`.

9. Given the user is authenticated, when they view the top bar, then their email address
   is visible and the workspaceId is displayed as the workspace label.

10. Given the user is authenticated, when they click the logout button in the top bar,
    then Redux auth state is cleared, they are navigated to `/dev-login`, and subsequent
    navigation to `/` redirects back to `/dev-login`.

11. Given any user navigates to a path that does not exist (e.g. `/does-not-exist`), then
    `NotFoundPage` renders with a link back to `/`.

---

## Agent instructions

**Backend Agent:** No work for this feature. Skip.

**Frontend Agent:**

Build in this sequence:

1. Create `web-frontend/src/store/uiSlice.ts` with `sidebarOpen` state, `setSidebarOpen`,
   and `toggleSidebar` actions. Register `uiReducer` in `store.ts`.

2. Create `web-frontend/src/router/guards/ProtectedRoute.tsx`. Reads `accessToken` from
   Redux via `useAppSelector(selectAuth)`. If null, renders `<Navigate to="/dev-login" replace />`.
   If present, renders `<Outlet />` (this guard wraps a layout route, not a single page).

3. Create `web-frontend/src/router/guards/GuestRoute.tsx`. Reads `accessToken` from Redux.
   If present, renders `<Navigate to="/" replace />`. If null, renders children
   (wraps a single page, not a layout route, so use `children` not `<Outlet />`).

4. Create `web-frontend/src/modules/shared/ui/components/NavItem.tsx`. Props: `to: string`,
   `label: string`, `icon: React.ReactNode`, `collapsed: boolean`. Uses `NavLink` from
   `react-router-dom` to apply an active style when the route matches. When `collapsed`
   is true, renders icon only (tooltip with label on hover via shadcn `Tooltip`). When
   false, renders icon + label side by side.

5. Create `web-frontend/src/modules/shared/ui/components/Sidebar.tsx`. Reads `sidebarOpen`
   from Redux. Dispatches `toggleSidebar` on the collapse button click. Renders three
   `NavItem` entries: Workflows (`/workflows`), My Tasks (`/tasks`),
   Active Workflows (`/active`). Use lucide-react icons: `Workflow` for Workflows,
   `CheckSquare` for My Tasks, `LayoutDashboard` for Active Workflows. The collapse
   toggle button uses a `ChevronLeft` / `ChevronRight` icon depending on state.
   Sidebar width transitions smoothly with a CSS transition on width (not a hard swap).

6. Create `web-frontend/src/modules/shared/ui/components/TopBar.tsx`. Reads `email` and
   `workspaceId` from Redux. Displays workspace label on the left and user area on the
   right. User area: text initials avatar (first letter of email, uppercased, in a
   circular div), email text, and a logout button. Logout dispatches `clearCredentials`
   and calls `navigate('/dev-login')`.

7. Create `web-frontend/src/modules/shared/ui/layouts/AuthenticatedLayout.tsx`. Composes
   `Sidebar` + `TopBar` + `<main>` content area. The main area renders `<Outlet />`.
   Full viewport height, no page scroll on the outer shell — only the `<main>` area
   scrolls independently.

8. Create `web-frontend/src/modules/shared/ui/layouts/UnauthenticatedLayout.tsx`. Simple
   full-screen centered layout rendering `{children}`. No sidebar, no top bar. This layout
   exists for Phase 2 auth pages — it is not wired into the router in this feature
   (the existing `DevLoginPage` manages its own layout). Scaffold it now so it is ready.

9. Create stub pages:
   - `web-frontend/src/modules/workflows/ui/pages/WorkflowsPage.tsx` — heading "Workflows",
     subtext "Workflow management coming in a future feature."
   - `web-frontend/src/modules/tasks/ui/pages/MyTasksPage.tsx` — heading "My Tasks",
     subtext "Task dashboard coming in a future feature."
   - `web-frontend/src/modules/workflows/ui/pages/ActiveWorkflowsPage.tsx` — heading
     "Active Workflows", subtext "Active workflow board coming in a future feature."

10. Create `web-frontend/src/modules/shared/ui/pages/NotFoundPage.tsx`. Full-screen
    centered layout with "404" large, "Page not found" below, and a `Link` to `/`.

11. Update `web-frontend/src/router/index.tsx` to the full route tree. The root `/` route
    uses `ProtectedRoute` as its element with `AuthenticatedLayout` nested inside it as
    the layout component (use a nested route structure: `ProtectedRoute` renders `Outlet`,
    its child is the layout route with `AuthenticatedLayout` rendering `Outlet`, which
    in turn contains the feature routes). The index route of the authenticated section
    redirects to `/workflows`. The `*` catch-all renders `NotFoundPage`.

**Handoff point:** There is no backend handoff. The Frontend Agent owns this feature
end-to-end. Samuel will manually test it by running the frontend dev server, clicking
"Log in as dev user" on `/dev-login`, confirming the shell appears, navigating between
the three sections, testing sidebar collapse, and confirming logout works.
