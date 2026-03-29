// DevLoginPage.tsx
// Dev-only login page — renders only when import.meta.env.DEV is true.
//
// In a production build (import.meta.env.DEV === false) this component
// immediately redirects to / and renders nothing. The route itself is also
// conditionally registered in the router, providing defence in depth.
//
// When rendered in dev mode, it shows a single "Log in as dev user" button.
// On click, useDevLogin calls POST /api/auth/dev-login, dispatches the token
// to Redux, and navigates to /. If the request fails, a Sonner toast appears.
//
// No form fields — this is a zero-friction dev convenience tool, not a login form.
// No validation — there is nothing to validate.

import { Navigate } from 'react-router-dom';
import { Button } from '@/modules/shared/ui/components/button';
import { useDevLogin } from '../../hooks/useDevLogin';

export function DevLoginPage() {
  // Guard: if this component is somehow reached in a production build,
  // redirect immediately. The route registration does the same check,
  // but belt-and-suspenders is correct for a security-adjacent guard.
  if (!import.meta.env.DEV) {
    return <Navigate to="/" replace />;
  }

  return <DevLoginContent />;
}

// Separated so the hook (which requires React context) is only called
// after the DEV guard has passed. Calling hooks before a conditional
// return would violate the Rules of Hooks.
function DevLoginContent() {
  const devLogin = useDevLogin();

  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      <div className="flex flex-col items-center gap-6 rounded-xl border border-border bg-card p-10 shadow-sm">
        <div className="flex flex-col items-center gap-2 text-center">
          <h1 className="text-2xl font-semibold tracking-tight">StackFlow</h1>
          <p className="text-sm text-muted-foreground">
            Development environment — stub login only
          </p>
        </div>

        <Button
          onClick={() => devLogin.mutate()}
          disabled={devLogin.isPending}
          size="lg"
          className="w-full"
        >
          {devLogin.isPending ? 'Logging in...' : 'Log in as dev user'}
        </Button>

        <p className="text-xs text-muted-foreground">
          This page does not exist in production builds.
        </p>
      </div>
    </div>
  );
}
