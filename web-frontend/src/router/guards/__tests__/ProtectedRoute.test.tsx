// ProtectedRoute.test.tsx
// Unit tests for the ProtectedRoute guard component.
//
// What is tested:
//   Unauthenticated user (accessToken is null) → redirects to / (landing page)
//   Authenticated user (accessToken is present) → renders <Outlet />
//
// Test strategy:
//   Use MemoryRouter to simulate navigation context and <Routes> structure.
//   Mock Redux store with configureStore + a minimal auth reducer state.
//   Render the guard inside the router to test navigation behavior.
//   Assert on the rendered output: either the redirect happened or the outlet
//   child component is visible.

import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { describe, it, expect } from 'vitest';
import { ProtectedRoute } from '../ProtectedRoute';
import authReducer, { setCredentials } from '@/store/authSlice';
import type { DevLoginResponseDto } from '@/modules/auth/dtos/AuthDto';

// ── Test fixture ──────────────────────────────────────────────────────────────
// A complete DevLoginResponseDto for authenticated state tests.
const stubLoginResponse: DevLoginResponseDto = {
  accessToken: 'eyJhbGciOiJIUzI1NiJ9.stub.signature',
  expiresAt: '2026-03-29T10:00:00.000Z',
  user: {
    id: '00000000-0000-0000-0000-000000000001',
    email: 'dev@stackflow.local',
    role: 'Admin',
    workspaceId: '00000000-0000-0000-0000-000000000002',
  },
};

// ── Test helper ───────────────────────────────────────────────────────────────
// Creates a minimal test store with only the auth reducer.
// Optionally populates it with credentials if authenticated is true.
function createTestStore(authenticated = false) {
  const store = configureStore({ reducer: { auth: authReducer } });

  if (authenticated) {
    store.dispatch(setCredentials(stubLoginResponse));
  }

  return store;
}

// ── Test helper ───────────────────────────────────────────────────────────────
// Renders the ProtectedRoute inside a router context with a protected and an
// unprotected route so we can test redirect behavior.
// The /protected route uses ProtectedRoute guard; / (landing page) is unprotected.
// The ProtectedRoute renders a child component with text "Protected Content".
function renderWithRouter(store: ReturnType<typeof createTestStore>) {
  return render(
    <Provider store={store}>
      <MemoryRouter initialEntries={['/protected']}>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="protected" element={<div>Protected Content</div>} />
          </Route>
          <Route path="/" element={<div>Landing Page</div>} />
        </Routes>
      </MemoryRouter>
    </Provider>
  );
}

// ── ProtectedRoute tests ──────────────────────────────────────────────────────

describe('ProtectedRoute', () => {
  describe('unauthenticated user (accessToken is null)', () => {
    it('redirects to / (landing page) when accessToken is not set', () => {
      const store = createTestStore(false);
      renderWithRouter(store);

      // The redirect should have happened, so we expect to see the landing page content.
      expect(screen.getByText('Landing Page')).toBeInTheDocument();
    });

    it('does not render the protected route content when unauthenticated', () => {
      const store = createTestStore(false);
      renderWithRouter(store);

      // The protected content should NOT be visible because the redirect happened.
      expect(screen.queryByText('Protected Content')).not.toBeInTheDocument();
    });
  });

  describe('authenticated user (accessToken is present)', () => {
    it('renders the outlet when accessToken is set', () => {
      const store = createTestStore(true);
      renderWithRouter(store);

      // The guard should pass and the protected content should render.
      expect(screen.getByText('Protected Content')).toBeInTheDocument();
    });

    it('does not render the landing page when authenticated', () => {
      const store = createTestStore(true);
      renderWithRouter(store);

      // The landing page should NOT be visible because the guard passed.
      expect(screen.queryByText('Landing Page')).not.toBeInTheDocument();
    });
  });

  describe('outlet rendering', () => {
    it('renders <Outlet /> which allows nested routes to render their content', () => {
      // This test verifies the guard does not interfere with the outlet mechanism.
      const testStore = configureStore({ reducer: { auth: authReducer } });
      testStore.dispatch(setCredentials(stubLoginResponse));

      render(
        <Provider store={testStore}>
          <MemoryRouter initialEntries={['/']}>
            <Routes>
              <Route element={<ProtectedRoute />}>
                <Route path="/" element={<div>Root Protected Page</div>} />
              </Route>
            </Routes>
          </MemoryRouter>
        </Provider>
      );

      // The nested route's element should render via the Outlet.
      expect(screen.getByText('Root Protected Page')).toBeInTheDocument();
    });
  });
});
