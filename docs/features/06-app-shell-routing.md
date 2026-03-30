# App Shell + Routing

> Last updated: 2026-03-30
> Phase: 1
> Status: Complete — PR approved

---

## What It Does

This feature builds the structural shell of the StackFlow frontend — a persistent two-column layout with a collapsible sidebar on the left, a top bar across the top, and a scrollable main content area. Every authenticated page renders inside this shell. The router enforces authentication (redirects unauthenticated users to `/dev-login`), prevents logged-in users from accessing the login page, and provides a catch-all 404 page for undefined routes. The sidebar can be collapsed to icon-only width; the state persists across route navigation via Redux.

---

## How It Works

When a user accesses the app:

1. The router in `web-frontend/src/router/index.tsx` checks the requested path
2. If the path is `/dev-login`:
   - `DevRoute` checks if the app is running in development mode
   - `GuestRoute` checks if the user has an `accessToken` in Redux
   - If authenticated, user is redirected to `/` (then to `/workflows`)
   - If unauthenticated, `DevLoginPage` is rendered
3. For any other authenticated route (e.g., `/workflows`):
   - `ProtectedRoute` checks for `accessToken` in Redux
   - If missing, user is redirected to `/dev-login`
   - If present, `AuthenticatedLayout` wraps the page
4. Inside `AuthenticatedLayout`:
   - The `TopBar` displays the current workspace ID (placeholder) and user email
   - The `Sidebar` displays three navigation links: Workflows, My Tasks, Active Workflows
   - The main content area (`<main>`) scrolls independently; the sidebar and top bar stay fixed
5. When the user clicks the collapse button in the sidebar:
   - `toggleSidebar()` dispatches to Redux `uiSlice`
   - Redux updates `sidebarOpen` (true ↔ false)
   - `Sidebar` re-renders with width transition `w-56 → w-14` (or vice versa)
   - `NavItem` components hide labels and show tooltips on hover when collapsed
6. When the user clicks logout in the top bar:
   - `clearCredentials()` dispatches to Redux auth slice
   - The `accessToken` is cleared
   - User is navigated to `/dev-login`
   - Next route navigation triggers `ProtectedRoute` guard, which redirects to `/dev-login` if needed

The entire flow is end-to-end testable: a developer can run the frontend dev server, log in via the dev auth stub (Feature 2), see the full shell, navigate between sections, toggle the sidebar, and log out.

---

## Key Files

| File | Purpose |
|---|---|
| `web-frontend/src/router/index.tsx` | Central route registry; defines ProtectedRoute → AuthenticatedLayout tree |
| `web-frontend/src/router/guards/ProtectedRoute.tsx` | Route guard for authenticated routes; redirects to /dev-login if no token |
| `web-frontend/src/router/guards/GuestRoute.tsx` | Route guard for guest-only routes; redirects authenticated users to / |
| `web-frontend/src/modules/shared/ui/layouts/AuthenticatedLayout.tsx` | Root layout: sidebar + top bar + scrollable main area |
| `web-frontend/src/modules/shared/ui/layouts/UnauthenticatedLayout.tsx` | Scaffold layout for Phase 2 auth pages (login, register, etc.) |
| `web-frontend/src/modules/shared/ui/components/Sidebar.tsx` | Left-side navigation bar with collapse toggle; reads sidebarOpen from Redux |
| `web-frontend/src/modules/shared/ui/components/TopBar.tsx` | Header bar showing workspace and user controls; logout button |
| `web-frontend/src/modules/shared/ui/components/NavItem.tsx` | Single navigation entry with icon, label, and tooltip when collapsed |
| `web-frontend/src/store/uiSlice.ts` | Redux slice for sidebar collapse state (sidebarOpen boolean) |
| `web-frontend/src/store/store.ts` | Redux store configuration; registers uiReducer |
| `web-frontend/src/modules/workflows/ui/pages/WorkflowsPage.tsx` | Stub page at /workflows — placeholder until Feature 8 |
| `web-frontend/src/modules/tasks/ui/pages/MyTasksPage.tsx` | Stub page at /tasks — placeholder until Feature 13 |
| `web-frontend/src/modules/workflows/ui/pages/ActiveWorkflowsPage.tsx` | Stub page at /active — placeholder until Feature 15 |
| `web-frontend/src/modules/shared/ui/pages/NotFoundPage.tsx` | Rendered for undefined routes (the * catch-all) |
| `web-frontend/src/main.tsx` | Application entry point; provider order: Redux → QueryClient → Tooltip → Router → Toaster |

---

## Database Changes

No database changes in this feature.

---

## Events

No RabbitMQ events in this feature.

---

## Real-time (SignalR)

No SignalR events in this feature.

---

## Known Limitations or Caveats

- **Workspace name is a placeholder:** The top bar displays the raw `workspaceId` string (e.g., `00000000-0000-0000-0000-000000000001`) instead of a human-readable workspace name. A workspace name API does not exist until Feature 8+. Until then, the workspace ID serves as the label.

- **Mobile layout not implemented:** The Phase 1 build is desktop-only. No hamburger menu, no mobile-responsive sidebar collapse. A mobile-friendly layout (with hamburger menu) will be added in Phase 3 if needed.

- **Sidebar state not persisted to localStorage:** The collapse state lives in Redux and survives route navigation, but is lost on page refresh. Local persistence could be added via Redux middleware in a future feature (Phase 2), but is not in scope for Phase 1.

- **No user avatar image:** The top bar shows text initials only (first letter of email). Gravatar or profile image upload does not exist. The design uses a simple circular div with the initial letter.

- **No notification bell:** The top bar has no unread notification indicator or notification panel. Real-time notifications arrive in Phase 2 via SignalR and are displayed as in-app toast messages (Sonner) and a badge on the sidebar or top bar.

- **Stub pages have no real content:** The three stub pages (Workflows, My Tasks, Active Workflows) are placeholders with heading text and a message saying "Coming in a future feature." They are replaced with real implementations in Features 8, 13, and 15, respectively.

---

## Notes: Brief vs Implementation

Implementation matches the Feature Brief exactly. All acceptance criteria are met:

- Route guards (ProtectedRoute, GuestRoute) enforce authentication correctly
- Sidebar collapse works with Redux state and CSS transitions
- Top bar displays email and workspace ID, with working logout
- Stub pages render in the authenticated shell
- NotFoundPage catches undefined routes
- Sidebar navigation links to all three stub pages
- Provider order in main.tsx is correct for Redux + React Query + Router integration

No deviations or additional features were added beyond the brief.

---

## How to Use This Feature

### For Frontend Developers

**Navigating the codebase:**
1. All authenticated routes are registered in `web-frontend/src/router/index.tsx`
2. To add a new authenticated page:
   - Create the page component (e.g., `web-frontend/src/modules/{feature}/ui/pages/{FeaturePage}.tsx`)
   - Import it in `router/index.tsx`
   - Add a route object to the `AuthenticatedLayout` children array
   - The page automatically gets the shell, guards, and scroll behavior
3. The sidebar navigation is hardcoded in `Sidebar.tsx` — add new links there and update the `NAV_ITEMS` array when new features ship

**Modifying the shell:**
- Sidebar width: edit `w-56` (expanded) and `w-14` (collapsed) in `Sidebar.tsx`
- Sidebar colors: use `bg-sidebar`, `border-sidebar-border`, `text-sidebar-foreground` design tokens
- Top bar height: both top bar and logo area are `h-14` (56px)
- Main area padding: `p-6` in `AuthenticatedLayout` — adjust if needed for different page types

**Redux state (sidebar collapse):**
- Read: `const { sidebarOpen } = useAppSelector(selectUi);`
- Update: `dispatch(setSidebarOpen(false))` or `dispatch(toggleSidebar())`
- The state is in Redux, not useState, because it must survive route navigation

**Provider setup (adding new tools):**
- Never add a provider above Redux in `main.tsx` — Redux must be outermost so `apiClient` can read it
- Add new providers between QueryClient and Router if they need access to both
- Add new providers below Router only if they are route-specific (rare)

### For Samuel (Testing Manually)

1. Run `npm run dev` in `web-frontend/`
2. Open `http://localhost:3000` in a browser
3. You are redirected to `/dev-login` (not authenticated)
4. Click "Log in as dev user" — this calls the Feature 2 dev auth endpoint and stores the token in Redux
5. You are redirected to `/workflows` and see the full app shell
6. Click the collapse button at the bottom of the sidebar — the sidebar animates to icon-only width
7. Hover over the icons — tooltips appear showing the labels
8. Click the expand button — the sidebar animates back to full width
9. Click "My Tasks" in the sidebar — the URL changes to `/tasks` and the page updates without a full reload
10. Click "Active Workflows" — the URL changes to `/active`
11. Click the logout button (LogOut icon in the top bar, right side)
12. You are redirected to `/dev-login` and can see "Log in as dev user" again
13. Try navigating to a fake URL like `http://localhost:3000/does-not-exist` — the NotFoundPage renders with a link back to home
14. Verify that all three stub pages render with their headings and "coming in a future feature" messages

---

## Future Extensions

**Phase 2 (Auth, Notifications, Approvals):**
- Replace `DevLoginPage` inline layout with `UnauthenticatedLayout` wrapper for consistency
- Add real login, register, OTP, and password reset pages inside `UnauthenticatedLayout`
- Add a notification bell to the top bar showing unread count and recent notifications (via SignalR)
- Connect `TopBar` to real workspace name API (Feature 8+) so workspace label shows human-readable name

**Phase 3 (Analytics, Calendar, Group Workspaces):**
- Add breadcrumb navigation to the top bar or main content area (optional)
- Add mobile hamburger menu if Phase 3 includes mobile support
- Persist sidebar collapse state to localStorage via Redux middleware
- Add user avatar image to top bar (Gravatar or profile upload)
- Add role-based sidebar visibility (some items only visible to Admins, etc.)
