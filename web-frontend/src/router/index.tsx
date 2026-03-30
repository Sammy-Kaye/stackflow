// router/index.tsx
// Central route registry for the StackFlow frontend.
//
// Route guard summary:
//   DevRoute        — only accessible when import.meta.env.DEV is true
//   ProtectedRoute  — requires a valid accessToken in Redux auth state;
//                     redirects to /dev-login if missing; renders <Outlet />
//   GuestRoute      — redirects authenticated users to /;
//                     renders children if no token present
//
// Route tree:
//   /                         → ProtectedRoute → AuthenticatedLayout
//     index                   → redirect to /workflows
//     /workflows              → WorkflowsPage (stub — replaced in Feature 8)
//     /tasks                  → MyTasksPage (stub — replaced in Feature 13)
//     /active                 → ActiveWorkflowsPage (stub — replaced in Feature 15)
//   /dev-login                → DevRoute → GuestRoute → DevLoginPage
//   *                         → NotFoundPage
//
// Adding a new authenticated route:
//   1. Import the page component from its module.
//   2. Add a `{ path, element }` object inside the AuthenticatedLayout children array.
//   3. That is the only change required — layout and guards are inherited automatically.

import { createBrowserRouter, Navigate } from 'react-router-dom';
import { DevRoute } from './guards/DevRoute';
import { ProtectedRoute } from './guards/ProtectedRoute';
import { GuestRoute } from './guards/GuestRoute';
import { AuthenticatedLayout } from '@/modules/shared/ui/layouts/AuthenticatedLayout';
import { DevLoginPage } from '@/modules/auth/ui/pages/DevLoginPage';
import { WorkflowsPage } from '@/modules/workflows/ui/pages/WorkflowsPage';
import { MyTasksPage } from '@/modules/tasks/ui/pages/MyTasksPage';
import { ActiveWorkflowsPage } from '@/modules/workflows/ui/pages/ActiveWorkflowsPage';
import { NotFoundPage } from '@/modules/shared/ui/pages/NotFoundPage';

export const router = createBrowserRouter([
  // Authenticated section — ProtectedRoute gate, then AuthenticatedLayout shell
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AuthenticatedLayout />,
        children: [
          // Index: redirect / → /workflows so the shell always lands somewhere concrete
          {
            index: true,
            element: <Navigate to="/workflows" replace />,
          },
          {
            path: '/workflows',
            element: <WorkflowsPage />,
          },
          {
            path: '/tasks',
            element: <MyTasksPage />,
          },
          {
            path: '/active',
            element: <ActiveWorkflowsPage />,
          },
        ],
      },
    ],
  },

  // Dev-only login page — not accessible in production builds.
  // GuestRoute prevents a logged-in user from seeing this page.
  {
    path: '/dev-login',
    element: (
      <DevRoute>
        <GuestRoute>
          <DevLoginPage />
        </GuestRoute>
      </DevRoute>
    ),
  },

  // Catch-all: any unmatched path renders the 404 page.
  {
    path: '*',
    element: <NotFoundPage />,
  },
]);
