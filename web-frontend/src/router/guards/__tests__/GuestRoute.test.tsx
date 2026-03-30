// GuestRoute.test.tsx
// Unit tests for the GuestRoute guard component.
//
// What is tested:
//   Unauthenticated user (accessToken is null) → renders children
//   Authenticated user (accessToken is present) → redirects to /
//
// Test strategy:
//   Use MemoryRouter to simulate navigation context.
//   Mock Redux store with configureStore + a minimal auth reducer state.
//   Render the guard with children inside the router to test rendering and redirect.
//   Assert on the rendered output: either the children render or the redirect
//   happened and a different route's content appears.

import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { describe, it, expect } from 'vitest';
import { GuestRoute } from '../GuestRoute';
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
// Renders the GuestRoute with a child component inside a router context.
// The /login route uses GuestRoute; the / route is where authenticated users
// are redirected to.
function renderWithRouter(store: ReturnType<typeof createTestStore>) {
  return render(
    <Provider store={store}>
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route
            path="/login"
            element={
              <GuestRoute>
                <div>Guest Route Children</div>
              </GuestRoute>
            }
          />
          <Route path="/" element={<div>Home Page</div>} />
        </Routes>
      </MemoryRouter>
    </Provider>
  );
}

// ── GuestRoute tests ──────────────────────────────────────────────────────────

describe('GuestRoute', () => {
  describe('unauthenticated user (accessToken is null)', () => {
    it('renders children when accessToken is not set', () => {
      const store = createTestStore(false);
      renderWithRouter(store);

      // The guard should pass and the children should render.
      expect(screen.getByText('Guest Route Children')).toBeInTheDocument();
    });

    it('does not redirect to / when unauthenticated', () => {
      const store = createTestStore(false);
      renderWithRouter(store);

      // The home page should NOT be visible because the guard passed.
      expect(screen.queryByText('Home Page')).not.toBeInTheDocument();
    });
  });

  describe('authenticated user (accessToken is present)', () => {
    it('redirects to / when accessToken is set', () => {
      const store = createTestStore(true);
      renderWithRouter(store);

      // The redirect should have happened, so we expect to see the home page content.
      expect(screen.getByText('Home Page')).toBeInTheDocument();
    });

    it('does not render the guest route children when authenticated', () => {
      const store = createTestStore(true);
      renderWithRouter(store);

      // The guest route children should NOT be visible because the redirect happened.
      expect(screen.queryByText('Guest Route Children')).not.toBeInTheDocument();
    });
  });

  describe('children rendering', () => {
    it('renders children as-is without wrapping', () => {
      // Verify the guard passes through children directly when unauthenticated.
      const store = createTestStore(false);

      render(
        <Provider store={store}>
          <MemoryRouter initialEntries={['/login']}>
            <Routes>
              <Route
                path="/login"
                element={
                  <GuestRoute>
                    <span data-testid="test-child">Test Child Content</span>
                  </GuestRoute>
                }
              />
            </Routes>
          </MemoryRouter>
        </Provider>
      );

      // The child element should be rendered without any wrapper.
      expect(screen.getByTestId('test-child')).toBeInTheDocument();
      expect(screen.getByText('Test Child Content')).toBeInTheDocument();
    });
  });
});
