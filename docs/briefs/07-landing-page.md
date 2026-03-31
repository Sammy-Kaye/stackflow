# Feature Brief: Landing Page
Phase: 1
Status: Ready for implementation
Brief date: 2026-03-31

---

## What this feature does (plain English)

The landing page is the first screen a visitor sees when they arrive at the root URL
(`/`). In Phase 1 it acts as a public marketing page and the sole entry point into the
app — there is no separate login page. A single "Enter app" button triggers the dev
login stub, which returns a JWT and drops the user straight into the authenticated
shell. The page conveys what StackFlow is, why it exists, and what it costs, without
demanding any credentials.

---

## Scope — what IS in this brief

- A `LandingPage` component rendered at the root path `/` for unauthenticated visitors
- Navbar with StackFlow wordmark, nav links (Features, Pricing), and an "Enter app"
  button that calls `useDevLogin` and navigates to `/workflows` on success
- Navbar background transitions from transparent to a dark filled state when the user
  scrolls past 60px
- On mobile the nav links collapse into a hamburger; tapping it opens a full-screen
  overlay menu
- Hero section: full-viewport-height, centred, with headline, subheadline, primary
  "Enter app" CTA, and a styled `div` mockup of the app dashboard as the hero visual
  (no screenshot — built entirely from divs/spans styled to the design tokens)
- Features section: three cards in a row — "Build once, run forever",
  "Everyone knows their next step", "Every action, recorded"
- "How it works" section: three numbered steps — Build, Assign and launch,
  Track and complete — connected by a horizontal line on desktop
- Pricing section: three tiers in a row — Starter ($0), Team ($29/mo, highlighted as
  most popular), Pro ($79/mo). Every pricing CTA button navigates to `/workflows`
  (demo mode — no real registration in Phase 1)
- Bottom CTA section: "Ready to stop manual work?" card with a final "Enter app" button
- Footer: wordmark, links (Product, Pricing, Support), copyright line
- "Enter app" buttons show a spinner while `useDevLogin` is in flight
- If `useDevLogin` fails, a Sonner toast: "Could not connect to the server. Is the API
  running?"
- Authenticated users visiting `/` are redirected to `/workflows` via `GuestRoute`
  (this guard already exists — the router must be updated to apply it to `/`)
- Page uses the design token palette and typography from `design-reference/DESIGN.md`
  and matches the visual layout in `design-reference/landing/code.html`

---

## Scope — what is NOT in this brief

- Real authentication (Email+Password, Google OAuth, OTP) — Phase 2
- A separate `/login` or `/register` route — Phase 2
- Functional nav links for "Features", "Solutions", "Resources" — they are present in
  the navbar but anchor-scroll only; no separate pages
- Any backend work — this feature is frontend only
- Token persistence across page refresh — Phase 2 (Redux token is in-memory only)
- Analytics or event tracking on CTA clicks
- Blog, docs, or any additional marketing pages

---

## Domain entities involved

None. This feature does not touch any backend domain entities. It consumes the existing
`POST /api/auth/dev-login` endpoint (already live from Feature 2) and stores the
result in Redux (already wired from Feature 2).

---

## API Contract

This feature is frontend-only. It consumes one existing endpoint — no new endpoints
are introduced.

#### POST /api/auth/dev-login
Auth: Public

Request body: (none)

Response 200:
{
  accessToken: string,
  expiresAt: string (ISO 8601),
  user: {
    id: string (UUID),
    email: string,
    role: string,
    workspaceId: string (UUID)
  }
}

Error responses:
  500 { error: string }   (API unavailable — caught by onError in useDevLogin)

No new endpoints. No contract changes.

---

## Frontend routes and views

### Route changes

The current router has:
- `/` behind `ProtectedRoute` — redirects unauthenticated visitors to `/dev-login`
- `/dev-login` behind `DevRoute + GuestRoute`

The router must be updated so that:
- `/` is **public** and renders `LandingPage` when no token is present
- `/` redirects authenticated users to `/workflows`
- The `/dev-login` route is removed — it becomes redundant once the landing page
  provides the "Enter app" entry point
- All currently authenticated routes (`/workflows`, `/tasks`, `/active`) remain behind
  `ProtectedRoute` exactly as before

Revised route tree after this feature:

```
/                       → GuestRoute → LandingPage
                          (authenticated users redirected to /workflows)
/                       → ProtectedRoute → AuthenticatedLayout
  /workflows            → WorkflowsPage
  /tasks                → MyTasksPage
  /active               → ActiveWorkflowsPage
/dev-login              → removed (or kept as alias — see Agent instructions)
*                       → NotFoundPage
```

### New components and pages

- `LandingPage` — `web-frontend/src/modules/landing/ui/pages/LandingPage.tsx`
  Route-level component. Composes all section components listed below.

- `LandingNavbar` — `web-frontend/src/modules/landing/ui/components/LandingNavbar.tsx`
  Fixed top navbar. Manages scroll-based background via a `useEffect` + `useState`
  tracking `window.scrollY`. Contains "Enter app" button.

- `LandingHero` — `web-frontend/src/modules/landing/ui/components/LandingHero.tsx`
  Full-height hero with headline, subheadline, primary CTA, and dashboard mockup div.

- `LandingFeatures` — `web-frontend/src/modules/landing/ui/components/LandingFeatures.tsx`
  Three-column feature card grid.

- `LandingHowItWorks` — `web-frontend/src/modules/landing/ui/components/LandingHowItWorks.tsx`
  Three numbered steps with a connecting horizontal line.

- `LandingPricing` — `web-frontend/src/modules/landing/ui/components/LandingPricing.tsx`
  Three-tier pricing cards. All CTAs navigate to `/workflows` via `useNavigate` +
  `useDevLogin` (same pattern as the navbar button).

- `LandingCta` — `web-frontend/src/modules/landing/ui/components/LandingCta.tsx`
  Bottom CTA card. Contains the final "Enter app" button.

- `LandingFooter` — `web-frontend/src/modules/landing/ui/components/LandingFooter.tsx`
  Wordmark, links, copyright.

### Existing files modified

- `web-frontend/src/router/index.tsx` — updated to register `LandingPage` at `/` with
  `GuestRoute`, remove the `/dev-login` route, and ensure the authenticated layout
  subtree continues to function unchanged.

---

## RabbitMQ events (if any)

None for this feature.

---

## SignalR events (if any)

None for this feature.

---

## Audit requirements

No audit entries required for this feature. Dev login writes no audit records — it is a
stub that exists only for Phase 1.

---

## Acceptance criteria

1. Given an unauthenticated user, when they navigate to `http://localhost:3000/`, then
   `LandingPage` renders — the navbar, hero, features, how-it-works, pricing, CTA, and
   footer sections are all visible.

2. Given a user on the landing page, when they click any "Enter app" button and the API
   responds successfully, then `useDevLogin` fires, the JWT is stored in Redux, and the
   user is navigated to `/workflows`.

3. Given a user on the landing page, when they click "Enter app" and the API is
   unreachable, then the button returns to its default state and a Sonner toast displays
   "Could not connect to the server. Is the API running?"

4. Given a user on the landing page, when they click "Enter app", then the button
   displays a spinner for the duration of the in-flight request.

5. Given an authenticated user (token present in Redux), when they navigate to `/`, then
   they are redirected to `/workflows` without seeing the landing page.

6. Given a user on the landing page with the page scrolled to 0px, when they view the
   navbar, then it has a transparent background.

7. Given a user scrolling down past 60px, when they view the navbar, then its
   background transitions to the dark filled state (bg-background/80 with backdrop blur).

8. Given a user on a mobile viewport (< 768px), when they view the navbar, then the nav
   links are hidden and a hamburger icon is visible.

9. Given a user on a mobile viewport, when they tap the hamburger icon, then a
   full-screen overlay menu opens showing nav links and the "Enter app" button.

10. Given a user on the landing page, when they view the pricing section, then three
    tiers are shown — Starter ($0), Team ($29/mo, marked "MOST POPULAR"), Pro ($79/mo).

11. Given a user on the landing page, when they click any pricing CTA button, then
    `useDevLogin` fires (same as "Enter app") and navigates to `/workflows` on success.

12. Given the app is built in production mode (`import.meta.env.DEV === false`), when a
    user navigates to `/dev-login`, then they land on `NotFoundPage` (the route no longer
    exists).

---

## Agent instructions

Backend Agent:
No backend work required for this feature. Do not make any changes to the API.

Frontend Agent:
1. Create the `landing` module directory structure:
   `web-frontend/src/modules/landing/ui/pages/` and
   `web-frontend/src/modules/landing/ui/components/`
2. Build `LandingFooter` first — it has no dependencies and establishes the design token
   usage pattern for the rest of the page.
3. Build `LandingNavbar` — implement the scroll listener with `useEffect`/`useState`,
   the mobile hamburger toggle, and the "Enter app" button wired to `useDevLogin`.
4. Build `LandingHero` — headline, subheadline, CTA button (reuse the same `useDevLogin`
   call pattern from the navbar), and the dashboard mockup div styled to the design
   reference. The mockup is a purely visual div — no real data, no real components from
   other modules.
5. Build `LandingFeatures` — three cards with icons and copy matching the design
   reference.
6. Build `LandingHowItWorks` — three numbered step items with the connecting line.
7. Build `LandingPricing` — three tier cards. All CTA buttons call `useDevLogin` and
   navigate to `/workflows` on success.
8. Build `LandingCta` — the bottom CTA card.
9. Compose all section components into `LandingPage`.
10. Update `web-frontend/src/router/index.tsx`:
    - Register `LandingPage` at `/` wrapped in `GuestRoute` (not `ProtectedRoute`).
    - Remove the `/dev-login` route.
    - Verify all authenticated routes (`/workflows`, `/tasks`, `/active`) remain behind
      `ProtectedRoute` + `AuthenticatedLayout` unchanged.
    - Remove the `DevLoginPage` import if it is no longer referenced.
11. Verify the design token palette in `design-reference/DESIGN.md` is honoured
    throughout: surface hierarchy, teal gradient on primary CTAs, teal glow shadow,
    Manrope for headings, Inter for body text, no explicit 1px borders.
12. Verify the visual output matches `design-reference/landing/code.html`. The HTML
    file is reference only — do not copy-paste its markup.

Handoff point:
This feature is frontend-only. There is no backend handoff. The Frontend Agent may
begin immediately. The only dependency is that the API is running locally so that
the "Enter app" button can be manually tested end-to-end.
