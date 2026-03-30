// authSlice.ts
// Redux slice for authentication state.
//
// WHY Redux for auth: The access token must be readable synchronously by the
// apiClient request interceptor before any HTTP call is made. React Query is
// async and cannot serve this purpose. Redux is the right home for auth state.
//
// Token storage: in-memory only (Redux store). No localStorage, no cookies.
// The token is lost on page refresh — acceptable for the Phase 1 dev stub.
// Phase 2 real auth will add token persistence at that time.
//
// State shape mirrors the JWT claims: sub → userId, email, role, workspaceId.
// All fields are nullable: null means the user is not authenticated.
//
// Actions:
//   setCredentials(DevLoginResponseDto) — populate state after a successful login
//   clearCredentials()                  — wipe state on logout

import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import type { DevLoginResponseDto } from '@/modules/auth/dtos/AuthDto';

interface AuthState {
  accessToken: string | null;
  userId: string | null;
  email: string | null;
  role: string | null;
  workspaceId: string | null;
}

const initialState: AuthState = {
  accessToken: null,
  userId: null,
  email: null,
  role: null,
  workspaceId: null,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCredentials: (state, action: PayloadAction<DevLoginResponseDto>) => {
      const { accessToken, user } = action.payload;
      state.accessToken = accessToken;
      state.userId = user.id;
      state.email = user.email;
      state.role = user.role;
      state.workspaceId = user.workspaceId;
    },

    clearCredentials: (state) => {
      state.accessToken = null;
      state.userId = null;
      state.email = null;
      state.role = null;
      state.workspaceId = null;
    },
  },
});

export const { setCredentials, clearCredentials } = authSlice.actions;

// Typed selector — reads the auth state slice from the Redux root.
// Uses a minimal structural type { auth: AuthState } rather than the full RootState
// so the selector remains usable in tests that configure a store with only the
// auth reducer, without forcing the caller to supply unrelated slice state.
// Usage: const { accessToken } = useAppSelector(selectAuth);
export const selectAuth = (state: { auth: AuthState }) => state.auth;

export default authSlice.reducer;
