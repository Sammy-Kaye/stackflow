// DevRoute.tsx
// Route guard that only allows access when running in dev mode.
//
// If import.meta.env.DEV is false (production build), the user is redirected
// to / and the guarded page is never rendered. This is defence in depth:
// the page component also has an internal guard, but the route guard prevents
// the component from mounting at all in production.

import { Navigate } from 'react-router-dom';

interface DevRouteProps {
  children: React.ReactNode;
}

export function DevRoute({ children }: DevRouteProps) {
  if (!import.meta.env.DEV) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}
