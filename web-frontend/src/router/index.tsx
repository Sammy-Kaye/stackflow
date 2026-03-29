// router/index.tsx
// Central route registry for the StackFlow frontend.
//
// Route guards:
//   DevRoute      — only accessible when import.meta.env.DEV is true
//   ProtectedRoute — requires a valid JWT in Redux auth state (Phase 2)
//   AdminRoute    — requires admin role (Phase 2)
//   GuestRoute    — redirects away if already logged in (Phase 2)
//
// Adding a new route:
//   1. Import the page component from its module.
//   2. Add an object to the routes array with path, element, and the
//      correct guard wrapping the page component.
//   3. That's it — no other file needs to change.

import { createBrowserRouter } from 'react-router-dom';
import { DevRoute } from './guards/DevRoute';
import { DevLoginPage } from '@/modules/auth/ui/pages/DevLoginPage';

export const router = createBrowserRouter([
  // Dev-only login page — not accessible in production builds.
  // The DevRoute guard redirects to / if import.meta.env.DEV is false.
  {
    path: '/dev-login',
    element: (
      <DevRoute>
        <DevLoginPage />
      </DevRoute>
    ),
  },

  // Placeholder root — will be replaced by the App Shell in Feature 6.
  {
    path: '/',
    element: <div>StackFlow — App Shell coming in Feature 6</div>,
  },
]);
