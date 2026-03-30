// ProtectedRoute.tsx
// Route guard that requires a valid access token in Redux auth state.
//
// If accessToken is null (user is not authenticated), the user is redirected
// to /dev-login and the guarded layout is never rendered. The `replace` prop
// prevents the guarded URL from being added to the browser history — so the
// back button after redirect does not loop the user back to the protected route.
//
// This guard wraps a layout route (not a single page), so it renders <Outlet />
// when the user is authenticated. The nested routes render inside the Outlet.
//
// Usage in router:
//   {
//     element: <ProtectedRoute />,
//     children: [
//       { element: <AuthenticatedLayout />, children: [...] }
//     ]
//   }

import { Navigate, Outlet } from 'react-router-dom';
import { useAppSelector } from '@/store/hooks';
import { selectAuth } from '@/store/authSlice';

export function ProtectedRoute() {
  const { accessToken } = useAppSelector(selectAuth);

  if (!accessToken) {
    return <Navigate to="/dev-login" replace />;
  }

  return <Outlet />;
}
