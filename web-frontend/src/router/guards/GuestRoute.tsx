// GuestRoute.tsx
// Route guard that redirects authenticated users away from guest-only pages.
//
// If accessToken is present (user is already logged in), they are redirected
// to /workflows — bypassing the landing page and landing directly in the app.
// This prevents a logged-in user from seeing the marketing page again.
//
// If accessToken is null, the children are rendered as-is. This guard wraps a
// single page (not a layout route), so it uses `children` rather than <Outlet />.
//
// Usage in router:
//   <GuestRoute>
//     <LandingPage />
//   </GuestRoute>

import { Navigate } from 'react-router-dom';
import { useAppSelector } from '@/store/hooks';
import { selectAuth } from '@/store/authSlice';

interface GuestRouteProps {
  children: React.ReactNode;
}

export function GuestRoute({ children }: GuestRouteProps) {
  const { accessToken } = useAppSelector(selectAuth);

  if (accessToken) {
    return <Navigate to="/workflows" replace />;
  }

  return <>{children}</>;
}
