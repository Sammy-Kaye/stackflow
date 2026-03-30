# CLAUDE.md Update Proposal: App Shell + Routing (Feature 6)

## Summary

This proposal recommends adding two sections to CLAUDE.md documenting patterns introduced in Feature 6:

1. **Route Guard Pattern** — how authentication guards work in the router
2. **Redux for Persistent UI State** — when to use Redux for client-side UI preferences like sidebar collapse

These patterns are foundational for the frontend and should be documented in CLAUDE.md so all future frontend agents understand them from the start.

---

## Proposed Addition to CLAUDE.md

### Section Location
Under the existing "Frontend Architecture" section, after "State Management" and before "Service Layer".

### Proposed Text

```markdown
### Route Guards

Authentication and guest-only routes are enforced via guard components in `web-frontend/src/router/guards/`.

**ProtectedRoute** (`ProtectedRoute.tsx`)
```
Wraps authenticated routes. Reads `accessToken` from Redux auth state. If missing, redirects to `/dev-login` and prevents the guarded route from rendering. If present, renders `<Outlet />` to allow nested routes.

Usage in router:
```typescript
{
  element: <ProtectedRoute />,
  children: [{ element: <AuthenticatedLayout />, children: [...] }]
}
```

The `replace` prop on `<Navigate>` prevents the protected URL from being added to browser history — the back button does not loop the user back to the protected route.
```

**GuestRoute** (`GuestRoute.tsx`)
```
Wraps guest-only routes (login page). If `accessToken` is present, redirects to `/`. If absent, renders children. Use this to prevent a logged-in user from seeing the login page.

Usage in router:
```typescript
<GuestRoute>
  <DevLoginPage />
</GuestRoute>
```

This guard wraps a single page, not a layout, so it accepts `children` prop instead of `<Outlet />`.
```

**Both guards read from the same `selectAuth` selector,** keeping authentication logic in one place. Never implement auth logic directly in a page component — always use a guard.

---

### Sidebar Collapse State Pattern

Persistent UI state like "is the sidebar open?" lives in Redux, not `useState`. This is one of the two legitimate use cases for Redux (the other is auth tokens).

**Why Redux for UI state:**
```
- State must survive route navigation (user clicks sidebar link → page updates → sidebar remains in previous state)
- State is read by multiple independent components (Sidebar, NavItem)
- Avoiding prop drilling — useState in a parent, pass to 10 children, becomes fragile
- Future persistence — state can be saved to localStorage via Redux middleware without touching component code
```

**Example: sidebarOpen in uiSlice**

```typescript
// web-frontend/src/store/uiSlice.ts
const uiSlice = createSlice({
  name: 'ui',
  initialState: { sidebarOpen: true },
  reducers: {
    setSidebarOpen: (state, action) => { state.sidebarOpen = action.payload; },
    toggleSidebar: (state) => { state.sidebarOpen = !state.sidebarOpen; },
  },
});
```

```typescript
// In Sidebar component
const { sidebarOpen } = useAppSelector(selectUi);
const dispatch = useAppDispatch();

<button onClick={() => dispatch(toggleSidebar())}>
  Collapse
</button>
```

The state can now be read and updated by any component without prop drilling. When the user navigates to a new route, the state persists because Redux persists — the Sidebar component unmounts and remounts with the same Redux state value.

**When NOT to use Redux for state:**
- Server data (workflows, tasks, users) → React Query (it handles caching and staleness)
- Transient form state (dirty flags, validation errors) → React Hook Form or `useState`
- Transient UI state (modal open, dropdown expanded) → `useState` (OK to lose on route change)
```

---

## Why This Update Is Needed

**Route guards** are used on every authenticated route in the application. A frontend agent building a new feature will need to understand how authentication redirects work and why the router is structured with nested `ProtectedRoute` and `AuthenticatedLayout` elements.

**Redux for UI state** is a frequent decision point. A new frontend agent might be tempted to use Redux for server data (wrong — React Query does that) or use `useState` in a top-level component for sidebar state (wrong — doesn't survive route navigation). Having this pattern documented in CLAUDE.md sets the correct expectation.

Without this context, future agents might:
- Implement route guards directly in page components (duplicates auth logic)
- Use `useState` for sidebar collapse (state resets on navigation)
- Misuse Redux for server data instead of React Query
- Prop drill UI preferences through 5 layers of components

---

## Samuel's Decision

Samuel reviews this proposal and decides:
- **Accept:** Integrate the proposed text into CLAUDE.md under "Frontend Architecture" section
- **Reject:** No CLAUDE.md update needed; the docs already cover this well enough
- **Edit:** Take the spirit of this proposal, modify the wording, and integrate a revised version into CLAUDE.md

The decision is Samuel's alone. This agent does not modify CLAUDE.md.
