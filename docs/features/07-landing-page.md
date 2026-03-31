# Landing Page

> Last updated: 2026-03-31
> Phase: 1
> Status: Complete — PR approved

---

## What it does

The landing page is the first public screen a visitor encounters at the root URL (`/`).
It serves as both a marketing page describing what StackFlow is and the entry point
into the application. A single "Enter app" button triggers dev login, which returns
a JWT and navigates the user directly into the authenticated workflow dashboard.
Authenticated users bypass the landing page entirely, redirected to `/workflows`.

---

## How it works

When an unauthenticated visitor navigates to `/`, the `GuestRoute` guard checks Redux
for an access token. If none exists, `LandingPage` renders. The page is a vertical scroll
through seven sections: navbar, hero, features, how-it-works, pricing, call-to-action,
and footer. Each "Enter app" button (navbar, hero, pricing, CTA) calls `useDevLogin()`,
which fires `POST /api/auth/dev-login`, stores the JWT in Redux via `setCredentials()`,
and then navigates to `/workflows`. If the API is unreachable, `useDevLogin`'s error
handler displays a Sonner toast: "Could not connect to the server. Is the API running?"

The navbar is fixed at the top and transitions from transparent to a dark filled background
(bg-surface/80 with backdrop blur) when the scroll position exceeds 60px. On mobile
(viewport < 768px), the nav links collapse into a hamburger menu that opens a full-screen
overlay. The overlay menu closes when the user clicks a link, taps the button, or clicks
the X icon.

All styling uses StackFlow design token CSS variables (`--sf-primary`, `--sf-surface`, etc.),
ensuring visual consistency with the authenticated app.

---

## Key files

| File | Purpose |
|---|---|
| `web-frontend/src/modules/landing/ui/pages/LandingPage.tsx` | Route-level page component — composes all landing sections |
| `web-frontend/src/modules/landing/ui/components/LandingNavbar.tsx` | Fixed navbar with scroll-based background transition, mobile hamburger, "Enter app" button |
| `web-frontend/src/modules/landing/ui/components/LandingHero.tsx` | Full-height hero section with headline, subtext, CTA, and dashboard mockup visual |
| `web-frontend/src/modules/landing/ui/components/LandingFeatures.tsx` | Three-card feature grid highlighting core capabilities |
| `web-frontend/src/modules/landing/ui/components/LandingHowItWorks.tsx` | Three numbered steps (Build, Assign, Track) with connecting line on desktop |
| `web-frontend/src/modules/landing/ui/components/LandingPricing.tsx` | Three-tier pricing cards (Starter $0, Team $29, Pro $79) with isolated CTA button instances |
| `web-frontend/src/modules/landing/ui/components/LandingCta.tsx` | Bottom call-to-action card with headline and final "Enter app" button |
| `web-frontend/src/modules/landing/ui/components/LandingFooter.tsx` | Footer with wordmark, link columns, and copyright |
| `web-frontend/src/router/index.tsx` | Central router — updated to register LandingPage at `/` with GuestRoute, removed `/dev-login` route |
| `web-frontend/src/router/guards/GuestRoute.tsx` | Route guard — renders children if no token present; redirects authenticated users to `/workflows` |
| `web-frontend/src/router/guards/ProtectedRoute.tsx` | Route guard for authenticated section — redirects unauthenticated users to `/` |
| `web-frontend/src/modules/auth/hooks/useDevLogin.ts` | React Query mutation hook — calls dev-login endpoint, stores JWT, navigates to dashboard |
| `web-frontend/src/modules/auth/infrastructure/auth-service.ts` | Service layer — single source of truth for auth API calls |
| `web-frontend/src/modules/auth/dtos/AuthDto.ts` | TypeScript interfaces for dev-login and me endpoint responses |

---

## Database changes

No database changes in this feature.

---

## Events

No RabbitMQ events in this feature.

---

## Real-time (SignalR)

No SignalR events in this feature.

---

## Route changes

The router was updated to support public access to the landing page:

**New route:**
- `/` — GuestRoute (unauthenticated only) → `LandingPage` (marketing page)
  - If authenticated, redirects to `/workflows`

**Removed route:**
- `/dev-login` — this route is no longer needed; the landing page provides the "Enter app" entry point

**Unchanged routes:**
- `/workflows`, `/tasks`, `/active` — remain behind `ProtectedRoute` + `AuthenticatedLayout` exactly as before
- `*` — catch-all NotFoundPage (404)

---

## Component tree and responsibilities

```
LandingPage (page)
├─ LandingNavbar
│  └─ Fixed navbar + mobile overlay
│     └─ "Enter app" button (calls useDevLogin)
├─ LandingHero
│  ├─ Headline / subheadline
│  ├─ "Enter app" button
│  └─ Dashboard mockup (static div-based visual)
├─ LandingFeatures
│  └─ Three feature cards (Layers, Users, ClipboardList icons)
├─ LandingHowItWorks
│  └─ Three numbered steps with connecting line
├─ LandingPricing
│  ├─ Three tier cards (Starter / Team / Pro)
│  └─ PricingCtaButton × 3 (each with its own useDevLogin instance)
├─ LandingCta
│  ├─ Headline / subtext
│  └─ "Enter app" button
└─ LandingFooter
   └─ Wordmark, links, copyright
```

---

## Guard logic: GuestRoute and ProtectedRoute flow

**GuestRoute** (used at `/`)
- Reads `accessToken` from Redux auth state
- If token present → redirect to `/workflows` (authenticated user, no need for landing page)
- If token absent → render children (show landing page)

**ProtectedRoute** (wraps authenticated section)
- Reads `accessToken` from Redux auth state
- If token absent → redirect to `/` (unauthenticated, send to landing page)
- If token present → render `<Outlet />` (show authenticated layout and nested routes)

**Result:**
- Unauthenticated user at `/` → sees LandingPage
- Authenticated user at `/` → redirected to `/workflows`
- Unauthenticated user at `/workflows` → redirected to `/`
- Authenticated user at `/workflows` → sees AuthenticatedLayout + WorkflowsPage

---

## Dev and prod API base URL strategy

**Development** (localhost)
- API base URL hardcoded in `api-client.ts` as `http://localhost:5000`
- When running via `docker compose up -d`, the backend listens on port 5000
- Frontend Vite dev server runs on port 3000
- No proxying in dev — Axios makes direct requests to the API

**Production** (deployment via Terraform + Ansible)
- API base URL set via environment variable `VITE_API_URL`
- Typically: `https://api.stackflow.example.com` (same domain as frontend via Nginx reverse proxy)
- Vite build substitutes the value at build time
- Frontend and API share the same domain, so CORS headers are minimal

Implementation in `api-client.ts`:
```typescript
const baseURL = import.meta.env.VITE_API_URL || 'http://localhost:5000';
```

---

## Key decisions made during build

**PricingCtaButton isolation:** Each pricing tier CTA button is a separate component with
its own `useDevLogin()` instance. This ensures that only the clicked button displays a
spinner — not all three buttons simultaneously. If all buttons shared a single mutation,
clicking Team would show spinners on Starter and Pro as well.

**Navigate directly without redirect middleware:** After successful dev login, `useDevLogin`
calls `navigate('/workflows')` directly. There is no intermediate redirect or auth check.
The app assumes that if `setCredentials` populated Redux, the subsequent request to
`/workflows` will succeed.

**Error toast always fixed string:** The error toast shows "Could not connect to the server.
Is the API running?" regardless of the underlying error. This is intentional — on a public
marketing page, detailed error messages confuse visitors and expose internal details.
The raw error is logged to the browser console for developer debugging.

**Dashboard mockup is pure div markup:** The hero section includes a visual mockup of the
app dashboard (sidebar, workflow cards, etc.). This mockup is built entirely from divs and
styled with design tokens. No real app components (like `WorkflowCard`) are imported. This
decouples the landing page from internal modules, preventing unnecessary rebuilds and keeping
the landing page load time independent of the app's complexity.

**Scroll-based navbar transition at 60px:** The navbar background becomes opaque when
`scrollY > 60px`. This threshold was chosen to activate the filled background after the
user scrolls past the hero section, not during it. If the threshold were 0px, the navbar
would have a filled background from the moment the page loads, making it visually heavy
against the white hero text.

**Mobile hamburger overlay is full-screen:** The mobile menu is a fixed overlay covering
the entire viewport. This prevents scroll behind the menu (via `document.body.style.overflow = 'hidden'`)
and ensures the overlay menu is always legible and accessible on small screens.

---

## Notes: Brief vs implementation

Implementation matches the Feature Brief exactly. All acceptance criteria were met:
- Landing page renders at `/` for unauthenticated users
- "Enter app" buttons call dev login and navigate to `/workflows`
- Error toast displays when API is unreachable
- Buttons show spinners during request
- Authenticated users are redirected from `/` to `/workflows`
- Navbar background is transparent at scroll position 0, filled at scroll > 60px
- Mobile hamburger opens a full-screen overlay menu
- Three pricing tiers shown with correct labels and amounts
- All pricing buttons trigger dev login
- `/dev-login` route was removed
- Design tokens from DESIGN.md were applied throughout
- Visual layout matches `design-reference/landing/code.html`
