// GuestRoute.tsx
// Route guard that redirects authenticated users away from guest-only pages.
//
// If accessToken is present (user is already logged in), they are redirected
// to / — which in turn redirects to /workflows via the index route. This prevents
// a logged-in user from reaching the login page and seeing a confusing state.
//
// If accessToken is null, the children are rendered as-is. This guard wraps a
// single page (not a layout route), so it uses `children` rather than <Outlet />.
//
// Usage in router:
//   <DevRoute>
//     <GuestRoute>
//       <DevLoginPage />
//     </GuestRoute>
//   </DevRoute>

import { Navigate } from 'react-router-dom';
import { useAppSelector } from '@/store/hooks';
import { selectAuth } from '@/store/authSlice';

interface GuestRouteProps {
  children: React.ReactNode;
}

export function GuestRoute({ children }: GuestRouteProps) {
  const { accessToken } = useAppSelector(selectAuth);

  if (accessToken) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}
