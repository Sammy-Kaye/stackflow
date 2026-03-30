// uiSlice.test.ts
// Unit tests for the Redux UI slice.
//
// What is tested:
//   Initial state       — sidebarOpen is true by default
//   setSidebarOpen      — sets sidebarOpen to the payload value (true or false)
//   toggleSidebar       — flips sidebarOpen from true to false or vice versa
//   selectUi            — the selector returns the ui state slice
//
// Test strategy:
//   Use the slice reducer directly for state mutation tests (no store needed).
//   Use a real configureStore instance for the selector test to verify it reads
//   from the correct key in the Redux root state.

import { configureStore } from '@reduxjs/toolkit';
import { describe, it, expect } from 'vitest';
import uiReducer, {
  setSidebarOpen,
  toggleSidebar,
  selectUi,
  type UiState,
} from '../uiSlice';

// ── Initial state ─────────────────────────────────────────────────────────────

describe('uiSlice — initial state', () => {
  it('has sidebarOpen set to true by default', () => {
    const state = uiReducer(undefined, { type: '@@INIT' });
    expect(state.sidebarOpen).toBe(true);
  });

  it('returns the correct initial state shape', () => {
    const state = uiReducer(undefined, { type: '@@INIT' });

    expect(state).toEqual({
      sidebarOpen: true,
    });
  });
});

// ── setSidebarOpen ────────────────────────────────────────────────────────────

describe('uiSlice — setSidebarOpen', () => {
  it('sets sidebarOpen to true when payload is true', () => {
    const state = uiReducer(undefined, setSidebarOpen(true));
    expect(state.sidebarOpen).toBe(true);
  });

  it('sets sidebarOpen to false when payload is false', () => {
    const state = uiReducer(undefined, setSidebarOpen(false));
    expect(state.sidebarOpen).toBe(false);
  });

  it('overwrites the previous value', () => {
    let state = uiReducer(undefined, setSidebarOpen(true));
    expect(state.sidebarOpen).toBe(true);

    state = uiReducer(state, setSidebarOpen(false));
    expect(state.sidebarOpen).toBe(false);

    state = uiReducer(state, setSidebarOpen(true));
    expect(state.sidebarOpen).toBe(true);
  });

  it('is idempotent — calling with the same value multiple times is safe', () => {
    let state = uiReducer(undefined, setSidebarOpen(false));
    expect(state.sidebarOpen).toBe(false);

    state = uiReducer(state, setSidebarOpen(false));
    expect(state.sidebarOpen).toBe(false);

    // Value should still be false, unchanged.
    expect(state.sidebarOpen).toBe(false);
  });
});

// ── toggleSidebar ────────────────────────────────────────────────────────────

describe('uiSlice — toggleSidebar', () => {
  it('flips sidebarOpen from true to false', () => {
    const initialState: UiState = { sidebarOpen: true };
    const state = uiReducer(initialState, toggleSidebar());
    expect(state.sidebarOpen).toBe(false);
  });

  it('flips sidebarOpen from false to true', () => {
    const initialState: UiState = { sidebarOpen: false };
    const state = uiReducer(initialState, toggleSidebar());
    expect(state.sidebarOpen).toBe(true);
  });

  it('toggles back and forth correctly', () => {
    let state = uiReducer(undefined, { type: '@@INIT' });
    expect(state.sidebarOpen).toBe(true);

    state = uiReducer(state, toggleSidebar());
    expect(state.sidebarOpen).toBe(false);

    state = uiReducer(state, toggleSidebar());
    expect(state.sidebarOpen).toBe(true);

    state = uiReducer(state, toggleSidebar());
    expect(state.sidebarOpen).toBe(false);
  });

  it('works when called from any initial state', () => {
    // Starting from true
    const fromTrue = uiReducer(undefined, toggleSidebar());
    expect(fromTrue.sidebarOpen).toBe(false);

    // Starting from false
    const startFalse: UiState = { sidebarOpen: false };
    const fromFalse = uiReducer(startFalse, toggleSidebar());
    expect(fromFalse.sidebarOpen).toBe(true);
  });
});

// ── selectUi ──────────────────────────────────────────────────────────────────

describe('uiSlice — selectUi selector', () => {
  it('returns the full ui state slice from the store', () => {
    const store = configureStore({ reducer: { ui: uiReducer } });

    const result = selectUi(store.getState());

    expect(result).toEqual({
      sidebarOpen: true,
    });
  });

  it('returns the current sidebarOpen value', () => {
    const store = configureStore({ reducer: { ui: uiReducer } });

    const result = selectUi(store.getState());

    expect(result.sidebarOpen).toBe(true);
  });

  it('reflects state changes after setSidebarOpen is dispatched', () => {
    const store = configureStore({ reducer: { ui: uiReducer } });

    store.dispatch(setSidebarOpen(false));
    const result = selectUi(store.getState());

    expect(result.sidebarOpen).toBe(false);
  });

  it('reflects state changes after toggleSidebar is dispatched', () => {
    const store = configureStore({ reducer: { ui: uiReducer } });

    // Initial state is true
    expect(selectUi(store.getState()).sidebarOpen).toBe(true);

    // After toggle
    store.dispatch(toggleSidebar());
    expect(selectUi(store.getState()).sidebarOpen).toBe(false);

    // After another toggle
    store.dispatch(toggleSidebar());
    expect(selectUi(store.getState()).sidebarOpen).toBe(true);
  });

  it('reads from the correct key in the Redux state', () => {
    // This test verifies the selector reads from state.ui, not state.uiSlice
    // or any other key. If the implementation is correct, it will find the value.
    // If the key is wrong, the selector will return undefined or throw.
    const store = configureStore({ reducer: { ui: uiReducer } });
    store.dispatch(setSidebarOpen(false));

    const result = selectUi(store.getState());

    // If the key was wrong, result would be undefined. This assertion fails if
    // the selector looks in the wrong place.
    expect(result).toBeDefined();
    expect(result.sidebarOpen).toBe(false);
  });
});
