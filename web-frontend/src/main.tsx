// main.tsx
// Application entry point.
//
// Provider order matters:
//   1. Redux Provider  — must wrap everything so apiClient interceptor can read
//                        the store synchronously on the first request.
//   2. QueryClientProvider — React Query cache scoped to the full app.
//   3. RouterProvider  — handles all client-side routing.
//   4. Toaster         — Sonner toast container, rendered at the root so toasts
//                        are always visible regardless of which route is active.

import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { Provider } from 'react-redux';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { RouterProvider } from 'react-router-dom';
import { Toaster } from 'sonner';
import { TooltipProvider } from '@/modules/shared/ui/components/tooltip';
import { store } from '@/store/store';
import { router } from '@/router/index';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Retry once on failure before showing an error state.
      // Keeps the dev experience snappy when the API is temporarily unreachable.
      retry: 1,
      staleTime: 30_000, // 30 seconds — avoids redundant refetches on tab focus
    },
  },
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Provider store={store}>
      <QueryClientProvider client={queryClient}>
        <TooltipProvider delay={300}>
          <RouterProvider router={router} />
          <Toaster richColors position="top-right" />
        </TooltipProvider>
      </QueryClientProvider>
    </Provider>
  </StrictMode>,
);
