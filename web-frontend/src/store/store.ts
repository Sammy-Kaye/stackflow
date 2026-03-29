// store.ts
// Redux store — central registry for all global client-side state.
//
// WHAT belongs here: auth tokens, current user identity, persistent UI state
// (sidebar open/closed, active workspace). These are the two legitimate Redux use cases.
//
// WHAT does NOT belong here: server data (workflows, tasks, users).
// Server data lives in React Query — it handles caching, refetching, and staleness
// automatically. Putting server data in Redux would require manual cache invalidation
// and risks showing stale data after mutations.
//
// RootState and AppDispatch are re-exported here so every consumer uses the same
// typed versions without importing from their individual slices.

import { configureStore } from '@reduxjs/toolkit';
import authReducer from './authSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
  },
});

// Inferred types — derived from the store itself so they stay in sync automatically
// when slices are added or removed.
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
