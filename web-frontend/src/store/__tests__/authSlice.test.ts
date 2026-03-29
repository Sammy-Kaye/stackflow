// authSlice.test.ts
// Unit tests for the Redux auth slice.
//
// What is tested:
//   Initial state     — the slice starts with all fields null (not authenticated).
//   setCredentials    — populates every field from a DevLoginResponseDto payload.
//   clearCredentials  — resets every field back to null.
//   selectAuth        — the selector returns the auth slice from a real store instance.
//
// What is NOT tested:
//   The apiClient interceptor that reads from the store — that is tested as part
//   of the useDevLogin hook suite, where the full mutation + dispatch cycle runs.
//   Redux store wiring (store.ts) — tested indirectly via selectAuth below.
//
// Strategy: use the slice reducer directly for action tests (no store needed) and
// use a real configureStore instance for the selector test. This keeps action tests
// fast and pure, while the selector test exercises the real RootState shape.

import { configureStore } from '@reduxjs/toolkit';
import authReducer, {
  setCredentials,
  clearCredentials,
  selectAuth,
} from '../authSlice';
import type { DevLoginResponseDto } from '@/modules/auth/dtos/AuthDto';

// ── Shared fixture ────────────────────────────────────────────────────────────
// A complete DevLoginResponseDto matching the API contract. Used across multiple
// tests so changes to the DTO surface as a single update here.
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

// ── Initial state ─────────────────────────────────────────────────────────────

describe('authSlice — initial state', () => {
  it('starts with accessToken as null', () => {
    const state = authReducer(undefined, { type: '@@INIT' });
    expect(state.accessToken).toBeNull();
  });

  it('starts with userId as null', () => {
    const state = authReducer(undefined, { type: '@@INIT' });
    expect(state.userId).toBeNull();
  });

  it('starts with email as null', () => {
    const state = authReducer(undefined, { type: '@@INIT' });
    expect(state.email).toBeNull();
  });

  it('starts with role as null', () => {
    const state = authReducer(undefined, { type: '@@INIT' });
    expect(state.role).toBeNull();
  });

  it('starts with workspaceId as null', () => {
    const state = authReducer(undefined, { type: '@@INIT' });
    expect(state.workspaceId).toBeNull();
  });
});

// ── setCredentials ────────────────────────────────────────────────────────────

describe('authSlice — setCredentials', () => {
  it('sets accessToken from the response payload', () => {
    const state = authReducer(undefined, setCredentials(stubLoginResponse));
    expect(state.accessToken).toBe(stubLoginResponse.accessToken);
  });

  it('sets userId from user.id in the response payload', () => {
    const state = authReducer(undefined, setCredentials(stubLoginResponse));
    expect(state.userId).toBe(stubLoginResponse.user.id);
  });

  it('sets email from user.email in the response payload', () => {
    const state = authReducer(undefined, setCredentials(stubLoginResponse));
    expect(state.email).toBe(stubLoginResponse.user.email);
  });

  it('sets role from user.role in the response payload', () => {
    const state = authReducer(undefined, setCredentials(stubLoginResponse));
    expect(state.role).toBe(stubLoginResponse.user.role);
  });

  it('sets workspaceId from user.workspaceId in the response payload', () => {
    const state = authReducer(undefined, setCredentials(stubLoginResponse));
    expect(state.workspaceId).toBe(stubLoginResponse.user.workspaceId);
  });

  it('populates all five fields in a single dispatch', () => {
    // Snapshot assertion: verifies nothing was silently omitted if the state shape grows.
    const state = authReducer(undefined, setCredentials(stubLoginResponse));

    expect(state).toEqual({
      accessToken: stubLoginResponse.accessToken,
      userId: stubLoginResponse.user.id,
      email: stubLoginResponse.user.email,
      role: stubLoginResponse.user.role,
      workspaceId: stubLoginResponse.user.workspaceId,
    });
  });

  it('overwrites a previously set token when called a second time', () => {
    // Verifies idempotent login: a second login replaces the previous credentials.
    const secondResponse: DevLoginResponseDto = {
      ...stubLoginResponse,
      accessToken: 'new-token',
      user: { ...stubLoginResponse.user, id: '00000000-0000-0000-0000-000000000099' },
    };

    const afterFirst = authReducer(undefined, setCredentials(stubLoginResponse));
    const afterSecond = authReducer(afterFirst, setCredentials(secondResponse));

    expect(afterSecond.accessToken).toBe('new-token');
    expect(afterSecond.userId).toBe('00000000-0000-0000-0000-000000000099');
  });
});

// ── clearCredentials ──────────────────────────────────────────────────────────

describe('authSlice — clearCredentials', () => {
  // Start from a populated state for all clearCredentials tests.
  const populatedState = authReducer(undefined, setCredentials(stubLoginResponse));

  it('resets accessToken to null', () => {
    const state = authReducer(populatedState, clearCredentials());
    expect(state.accessToken).toBeNull();
  });

  it('resets userId to null', () => {
    const state = authReducer(populatedState, clearCredentials());
    expect(state.userId).toBeNull();
  });

  it('resets email to null', () => {
    const state = authReducer(populatedState, clearCredentials());
    expect(state.email).toBeNull();
  });

  it('resets role to null', () => {
    const state = authReducer(populatedState, clearCredentials());
    expect(state.role).toBeNull();
  });

  it('resets workspaceId to null', () => {
    const state = authReducer(populatedState, clearCredentials());
    expect(state.workspaceId).toBeNull();
  });

  it('resets all five fields to null in a single dispatch', () => {
    // Snapshot assertion: verifies nothing was missed if the state shape grows.
    const state = authReducer(populatedState, clearCredentials());

    expect(state).toEqual({
      accessToken: null,
      userId: null,
      email: null,
      role: null,
      workspaceId: null,
    });
  });

  it('is safe to call when already in the initial (unauthenticated) state', () => {
    // Calling clearCredentials on an already-cleared store must not throw or corrupt state.
    const state = authReducer(undefined, clearCredentials());

    expect(state).toEqual({
      accessToken: null,
      userId: null,
      email: null,
      role: null,
      workspaceId: null,
    });
  });
});

// ── selectAuth ────────────────────────────────────────────────────────────────

describe('authSlice — selectAuth selector', () => {
  // Uses a real store so the selector is tested against the actual RootState shape.
  // This ensures the 'auth' key in configureStore({ reducer: { auth: ... } }) matches
  // what selectAuth reads.

  it('returns the full auth state slice from the store', () => {
    const store = configureStore({ reducer: { auth: authReducer } });

    const result = selectAuth(store.getState());

    expect(result).toEqual({
      accessToken: null,
      userId: null,
      email: null,
      role: null,
      workspaceId: null,
    });
  });

  it('returns populated auth state after setCredentials is dispatched', () => {
    const store = configureStore({ reducer: { auth: authReducer } });
    store.dispatch(setCredentials(stubLoginResponse));

    const result = selectAuth(store.getState());

    expect(result.accessToken).toBe(stubLoginResponse.accessToken);
    expect(result.userId).toBe(stubLoginResponse.user.id);
    expect(result.email).toBe(stubLoginResponse.user.email);
    expect(result.role).toBe(stubLoginResponse.user.role);
    expect(result.workspaceId).toBe(stubLoginResponse.user.workspaceId);
  });

  it('returns null fields after clearCredentials is dispatched', () => {
    const store = configureStore({ reducer: { auth: authReducer } });
    store.dispatch(setCredentials(stubLoginResponse));
    store.dispatch(clearCredentials());

    const result = selectAuth(store.getState());

    expect(result.accessToken).toBeNull();
    expect(result.userId).toBeNull();
    expect(result.email).toBeNull();
    expect(result.role).toBeNull();
    expect(result.workspaceId).toBeNull();
  });
});
