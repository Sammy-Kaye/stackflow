// NotFoundPage.tsx
// Rendered for any URL that does not match a registered route (the * catch-all).
//
// Full-screen centered layout — no sidebar, no top bar. This page renders outside
// the AuthenticatedLayout so it appears the same whether the user is logged in or not.
//
// Provides a link back to / so the user can return to the app. If they are
// authenticated, / will redirect them to /workflows. If not, / will redirect
// to /dev-login via the ProtectedRoute guard.

import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-background text-center">
      <p className="text-8xl font-bold text-muted-foreground/30">404</p>
      <h1 className="text-2xl font-semibold text-foreground">Page not found</h1>
      <p className="text-sm text-muted-foreground">
        The page you are looking for does not exist.
      </p>
      <Link
        to="/"
        className="text-sm font-medium text-primary underline-offset-4 hover:underline"
      >
        Back to home
      </Link>
    </div>
  );
}
