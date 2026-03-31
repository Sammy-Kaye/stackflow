// router/index.tsx
// Central route registry for the StackFlow frontend.
//
// Route guard summary:
//   ProtectedRoute  — requires a valid accessToken in Redux auth state;
//                     redirects to / (landing page) if missing; renders <Outlet />
//   GuestRoute      — redirects authenticated users to /workflows;
//                     renders children if no token present
//
// Route tree:
//   /                         → GuestRoute → LandingPage (public marketing page)
//   ProtectedRoute (pathless) → AuthenticatedLayout
//     /workflows              → WorkflowsPage (stub — replaced in Feature 8)
//     /tasks                  → MyTasksPage (stub — replaced in Feature 13)
//     /active                 → ActiveWorkflowsPage (stub — replaced in Feature 15)
//   *                         → NotFoundPage
//
// Adding a new authenticated route:
//   1. Import the page component from its module.
//   2. Add a `{ path, element }` object inside the AuthenticatedLayout children array.
//   3. That is the only change required — layout and guards are inherited automatically.

import { createBrowserRouter } from 'react-router-dom';
import { ProtectedRoute } from './guards/ProtectedRoute';
import { GuestRoute } from './guards/GuestRoute';
import { AuthenticatedLayout } from '@/modules/shared/ui/layouts/AuthenticatedLayout';
import { LandingPage } from '@/modules/landing/ui/pages/LandingPage';
import { WorkflowsPage } from '@/modules/workflows/ui/pages/WorkflowsPage';
import { MyTasksPage } from '@/modules/tasks/ui/pages/MyTasksPage';
import { ActiveWorkflowsPage } from '@/modules/workflows/ui/pages/ActiveWorkflowsPage';
import { NotFoundPage } from '@/modules/shared/ui/pages/NotFoundPage';

export const router = createBrowserRouter([
  // Public landing page — redirects to /workflows if already authenticated
  {
    path: '/',
    element: (
      <GuestRoute>
        <LandingPage />
      </GuestRoute>
    ),
  },

  // Authenticated section — ProtectedRoute gate (pathless), then AuthenticatedLayout shell
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AuthenticatedLayout />,
        children: [
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

  // Catch-all: any unmatched path renders the 404 page.
  {
    path: '*',
    element: <NotFoundPage />,
  },
]);
