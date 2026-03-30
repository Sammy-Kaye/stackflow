# App Shell + Routing — Frontend Reference

> Last updated: 2026-03-30
> Feature status: Approved — PR reviewed
> Related files: `web-frontend/src/router/`, `web-frontend/src/store/uiSlice.ts`, `web-frontend/src/modules/shared/ui/`

---

## Route Table

The complete authenticated and unauthenticated route tree.

| Path | Guard | Component | Layout | Purpose |
|---|---|---|---|---|
| `/` | `ProtectedRoute` | (redirects to `/workflows`) | `AuthenticatedLayout` | Root authenticated entry point |
| `/workflows` | `ProtectedRoute` | `WorkflowsPage` (stub) | `AuthenticatedLayout` | Workflows section — placeholder until Feature 8 |
| `/tasks` | `ProtectedRoute` | `MyTasksPage` (stub) | `AuthenticatedLayout` | My Tasks section — placeholder until Feature 13 |
| `/active` | `ProtectedRoute` | `ActiveWorkflowsPage` (stub) | `AuthenticatedLayout` | Active Workflows section — placeholder until Feature 15 |
| `/dev-login` | `DevRoute` + `GuestRoute` | `DevLoginPage` | (none) | Development-only login page — redirects authenticated users to `/` |
| `*` | (none) | `NotFoundPage` | (none) | Catch-all for undefined routes |

---

## Route Guards

### ProtectedRoute

**Location:** `web-frontend/src/router/guards/ProtectedRoute.tsx`

Guards all authenticated routes. Reads `accessToken` from Redux `authSlice`.

**Behavior:**
- If `accessToken` is null: redirects to `/dev-login` with `replace: true` (prevents back-button loop)
- If `accessToken` is present: renders `<Outlet />` (children routes render inside)

**Usage in router:**
```typescript
{
  element: <ProtectedRoute />,
  children: [
    { element: <AuthenticatedLayout />, children: [...] }
  ]
}
```

**Technical notes:**
- This guard wraps a layout route, not a page route, so it renders `<Outlet />` to allow nested routes to render
- The `replace: true` in the redirect prevents the protected URL from being added to browser history

### GuestRoute

**Location:** `web-frontend/src/router/guards/GuestRoute.tsx`

Guards guest-only pages (login). Reads `accessToken` from Redux `authSlice`.

**Behavior:**
- If `accessToken` is present: redirects to `/` with `replace: true` (prevents a logged-in user seeing the login page)
- If `accessToken` is null: renders children as-is

**Usage in router:**
```typescript
<DevRoute>
  <GuestRoute>
    <DevLoginPage />
  </GuestRoute>
</DevRoute>
```

**Technical notes:**
- This guard wraps a single page, not a layout, so it uses `children` prop instead of `<Outlet />`
- Both guards read from the same `selectAuth` selector, keeping auth logic in one place

---

## Layout Components

### AuthenticatedLayout

**Location:** `web-frontend/src/modules/shared/ui/layouts/AuthenticatedLayout.tsx`

The primary shell for all authenticated screens. Composes `Sidebar`, `TopBar`, and a scrollable main content area.

**Structure:**
```
<div class="flex h-screen flex-col">
  <TopBar />
  <div class="flex flex-1">
    <Sidebar />
    <main class="flex-1 overflow-y-auto">
      <Outlet />
    </main>
  </div>
</div>
```

**Layout rules:**
- Outer shell: full viewport height (`h-screen`), no scroll
- Top bar: fixed 56px height (`h-14`), spans full width, fixed position
- Sidebar: fixed position, takes available height below top bar, does NOT scroll internally
- Main content: flex-grow to fill remaining space, scrolls independently (`overflow-y-auto`)

**Props:** None — this is a layout route element, not a regular component.

### UnauthenticatedLayout

**Location:** `web-frontend/src/modules/shared/ui/layouts/UnauthenticatedLayout.tsx`

Placeholder layout for future authentication pages (login, register, OTP, password reset). Not yet wired into the router.

**Structure:**
```typescript
interface UnauthenticatedLayoutProps {
  children: React.ReactNode;
}
```

**Behavior:** Renders children in a full-screen centered container (`min-h-screen`, `flex items-center justify-center`).

**Usage (Phase 2):** Will wrap auth pages via nested layout route pattern:
```typescript
{
  element: <GuestRoute>,
  children: [
    { element: <UnauthenticatedLayout />, children: [
      { path: '/login', element: <LoginPage /> },
      { path: '/register', element: <RegisterPage /> },
    ]}
  ]
}
```

---

## Sidebar Components

### Sidebar

**Location:** `web-frontend/src/modules/shared/ui/components/Sidebar.tsx`

Persistent left-side navigation bar. Reads `sidebarOpen` from Redux `uiSlice`. Dispatches `toggleSidebar` on collapse button click.

**Props:** None — reads Redux state directly.

**State:**
- `sidebarOpen` (boolean) — Redux `uiSlice.sidebarOpen`

**Behavior:**
- Expanded: `w-56` (14rem) — shows icon + label for each nav item
- Collapsed: `w-14` (3.5rem) — shows icon only; labels hidden but revealed via tooltip on hover
- Width transitions smoothly with CSS `transition-all duration-200`

**Navigation items (Phase 1 placeholders):**

| Label | Path | Icon | Destination |
|---|---|---|---|
| Workflows | `/workflows` | `Workflow` (lucide-react) | WorkflowsPage (stub) — replaced in Feature 8 |
| My Tasks | `/tasks` | `CheckSquare` (lucide-react) | MyTasksPage (stub) — replaced in Feature 13 |
| Active Workflows | `/active` | `LayoutDashboard` (lucide-react) | ActiveWorkflowsPage (stub) — replaced in Feature 15 |

**Collapse button:**
- Location: Bottom of sidebar, above footer
- Icon: `ChevronLeft` when expanded, `ChevronRight` when collapsed
- Action: Dispatches `toggleSidebar()` to Redux `uiSlice`
- Text: "Collapse" (hidden when collapsed, shown when expanded)

**Design notes:**
- Background: `bg-sidebar` (design token)
- Logo area: 56px fixed header with "SF" text avatar and "StackFlow" label (hidden when collapsed)
- Flex layout ensures nav fills available space; collapse button always at bottom
- Uses shadcn/ui `Tooltip` component for collapsed state labels (base-nova style)

### NavItem

**Location:** `web-frontend/src/modules/shared/ui/components/NavItem.tsx`

A single navigation entry in the sidebar.

**Props:**

| Prop | Type | Required | Description |
|---|---|---|---|
| `to` | string | Yes | Route path (e.g., `/workflows`) |
| `label` | string | Yes | Human-readable label (e.g., "Workflows") |
| `icon` | React.ReactNode | Yes | Lucide-react icon element |
| `collapsed` | boolean | Yes | When true, shows icon-only; when false, shows icon + label |

**Behavior:**
- Uses `NavLink` from `react-router-dom` — automatically applies active styling when the route matches
- Active state: `bg-accent text-accent-foreground`
- Hover state: `hover:bg-accent hover:text-accent-foreground`
- When `collapsed: true`: renders icon-only, wrapped in a `Tooltip` that shows the label on hover
- When `collapsed: false`: renders icon and label side by side

**Example usage:**
```typescript
<NavItem
  to="/workflows"
  label="Workflows"
  icon={<Workflow className="size-5" />}
  collapsed={sidebarOpen === false}
/>
```

**Design notes:**
- Link classes: dynamic via `NavLink` function-as-className prop (receives `{ isActive }`)
- When collapsed, tooltip uses `side="right"` and `sideOffset={8}` to appear to the right of the icon
- Icon size: 20px (`size-5`), always visible
- Text truncates if label exceeds sidebar width (expanded state only)

---

## Top Bar Components

### TopBar

**Location:** `web-frontend/src/modules/shared/ui/components/TopBar.tsx`

Horizontal header bar spanning the full width. Displays workspace information on the left and user controls on the right.

**Props:** None — reads Redux state directly.

**State:**
- `email` (string | null) — from Redux `authSlice.email`
- `workspaceId` (string | null) — from Redux `authSlice.workspaceId`

**Left section (Workspace label):**
- Uppercase label: "WORKSPACE"
- Value: The current `workspaceId` as a placeholder string (actual workspace name API does not exist until Feature 8)
- Fallback: "—" if `workspaceId` is null
- Max width: 200px with truncation

**Right section (User area):**
- Text initials avatar: Circular div with first letter of email, uppercased (e.g., "S" for "samuel@example.com")
  - Fallback initial: "?" if email is null
  - Size: 32px (`size-8`)
  - Background: `bg-primary`
  - Text: `text-primary-foreground`, `text-xs font-semibold`
- Email text: Shows the authenticated user's email address
  - Hidden on small screens (`hidden sm:block`)
  - Text color: `text-muted-foreground`
  - Font: `text-sm`
- Logout button: Icon button with `LogOut` icon from lucide-react
  - Variant: `ghost`
  - Size: `icon-sm`
  - Action: Calls `handleLogout()`
  - Aria label: "Log out"

**Logout flow:**
1. User clicks logout button
2. `clearCredentials()` dispatches to Redux auth slice (clears `accessToken`, `email`, `workspaceId`, `role`)
3. `navigate('/dev-login')` redirects to login page
4. `ProtectedRoute` guard on next route navigation checks for `accessToken`, finds it null, redirects to `/dev-login` if needed
5. User sees the dev login page

**Design notes:**
- Height: 56px (`h-14`)
- Border: `border-b border-border` (matches design token)
- Padding: `px-4` horizontal, centered vertically with `flex items-center`
- Spacing: `justify-between` for left/right separation, `gap-3` within sections
- All text uses semantic colors: `text-foreground`, `text-muted-foreground`

---

## Redux State Shape: uiSlice

**Location:** `web-frontend/src/store/uiSlice.ts`

Redux slice for persistent client-side UI state.

### State Interface

```typescript
interface UiState {
  sidebarOpen: boolean;
}

const initialState: UiState = {
  sidebarOpen: true,  // sidebar expanded by default
};
```

### Actions

| Action | Payload | Effect | Usage |
|---|---|---|---|
| `setSidebarOpen` | `boolean` | Sets sidebar state explicitly to the provided value | `dispatch(setSidebarOpen(false))` |
| `toggleSidebar` | none | Flips the current sidebar state (true → false, false → true) | `dispatch(toggleSidebar())` — used by collapse button in Sidebar |

### Selector

**Function:** `selectUi(state: { ui: UiState }): UiState`

**Usage:**
```typescript
const { sidebarOpen } = useAppSelector(selectUi);
```

**Why this shape:** The selector accepts a structural type `{ ui: UiState }` rather than `RootState`. This keeps the selector testable in isolation without importing the entire store type.

### Redux Integration

Registered in `web-frontend/src/store/store.ts`:
```typescript
export const store = configureStore({
  reducer: {
    auth: authReducer,
    ui: uiReducer,  // registered here
  },
});
```

The slice is then accessible as `state.ui` in any Redux selector or middleware.

---

## Stub Pages

All three stub pages follow the same structure: a heading, subtext, and a placeholder message. These are replaced with real implementations in later features.

### WorkflowsPage

**Location:** `web-frontend/src/modules/workflows/ui/pages/WorkflowsPage.tsx`

- Path: `/workflows`
- Heading: "Workflows"
- Message: "Workflow management coming in a future feature."
- Replaced in: Feature 8 (Workflow CRUD)

### MyTasksPage

**Location:** `web-frontend/src/modules/tasks/ui/pages/MyTasksPage.tsx`

- Path: `/tasks`
- Heading: "My Tasks"
- Message: "Task dashboard coming in a future feature."
- Replaced in: Feature 13 (My Tasks Dashboard)

### ActiveWorkflowsPage

**Location:** `web-frontend/src/modules/workflows/ui/pages/ActiveWorkflowsPage.tsx`

- Path: `/active`
- Heading: "Active Workflows"
- Message: "Active workflow board coming in a future feature."
- Replaced in: Feature 15 (Active Workflows Board)

---

## NotFoundPage

**Location:** `web-frontend/src/modules/shared/ui/pages/NotFoundPage.tsx`

Rendered for any URL that does not match a registered route (the `*` catch-all).

**Layout:** Full-screen centered, no sidebar or top bar.

**Content:**
- Large "404" text (size: `text-8xl`, opacity: `text-muted-foreground/30`)
- Heading: "Page not found" (`text-2xl font-semibold`)
- Subtext: "The page you are looking for does not exist." (`text-sm`)
- Link: "Back to home" → `/` (styled as a text link with hover underline)

**Behavior:**
- If user is authenticated: navigating to `/` redirects to `/workflows`
- If user is unauthenticated: navigating to `/` redirects to `/dev-login` via `ProtectedRoute` guard

---

## How to Add a New Authenticated Route

### Step 1: Create the page component

Create a new page in the appropriate module (e.g., `web-frontend/src/modules/{feature}/ui/pages/{FeaturePage}.tsx`).

### Step 2: Import it in the router

Open `web-frontend/src/router/index.tsx` and add the import:
```typescript
import { FeaturePage } from '@/modules/{feature}/ui/pages/FeaturePage';
```

### Step 3: Add the route to the AuthenticatedLayout children

Find the `AuthenticatedLayout` children array (inside `ProtectedRoute`'s children) and add a new route object:
```typescript
{
  path: '/feature',
  element: <FeaturePage />,
}
```

That is the only change required. The new route automatically inherits:
- `ProtectedRoute` guard (redirects to `/dev-login` if not authenticated)
- `AuthenticatedLayout` shell (sidebar, top bar, main area layout)
- Scroll behavior (only main area scrolls)
- Sidebar navigation state (persisted in Redux)

The page renders inside the `<Outlet />` in the main content area of `AuthenticatedLayout`.

---

## How Sidebar Collapse Works

### User Flow

1. User clicks the collapse button at the bottom of the sidebar
2. Button's `onClick` handler dispatches `toggleSidebar()` to Redux `uiSlice`
3. Redux updates `state.ui.sidebarOpen` (true → false or false → true)
4. The `Sidebar` component re-renders because it subscribes to `selectUi` via `useAppSelector`
5. Sidebar width class updates: `w-56` → `w-14` (or vice versa)
6. CSS transition `transition-all duration-200` animates the width change smoothly
7. `NavItem` components receive the updated `collapsed` prop and re-render
   - If collapsed: hide label, show tooltip on hover
   - If expanded: show icon + label side by side

### State Persistence

The `sidebarOpen` state lives in Redux, not in `useState`. This means:
- State survives route navigation (e.g., user clicks a nav item and navigates to `/workflows` — sidebar collapse state is retained)
- State is readable by any component in the tree without prop drilling
- Can be persisted to `localStorage` in a future feature (Phase 2) via Redux middleware

### CSS Transitions

The sidebar uses Tailwind's `transition-all duration-200`:
```typescript
className={cn(
  'transition-all duration-200',
  sidebarOpen ? 'w-56' : 'w-14',
)}
```

This smoothly animates the width change when `sidebarOpen` changes, avoiding a jarring snap.

---

## Provider Order (main.tsx)

The provider stack in `web-frontend/src/main.tsx` must be in this exact order:

1. **Redux Provider** — wraps everything so the `apiClient` interceptor can read the store synchronously on the first request
2. **QueryClientProvider** — React Query cache scoped to the full app
3. **TooltipProvider** — shadcn/ui tooltip component provider (wraps `RouterProvider` and `Toaster`)
4. **RouterProvider** — handles all client-side routing
5. **Toaster** — Sonner toast container, rendered at root level so toasts are always visible

**Why this order matters:**
- Redux must come first because `apiClient` (defined in infrastructure) reads the store in its axios interceptor
- QueryClient must wrap routing to ensure all routes have access to React Query hooks
- TooltipProvider must wrap RouterProvider so tooltips work everywhere
- Toaster must be at the root so toasts overlay all routes

---

## Design System Classes

**Sidebar-specific Tailwind tokens:**
- `bg-sidebar` — sidebar background color
- `border-sidebar-border` — sidebar border color
- `text-sidebar-foreground` — sidebar text color

**Shared design tokens:**
- `bg-background`, `text-foreground` — page background and text
- `bg-accent`, `text-accent-foreground` — active/hover states
- `text-muted-foreground` — secondary text
- `border-border` — standard border color
