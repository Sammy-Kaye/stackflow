// uiSlice.ts
// Redux slice for persistent UI state.
//
// WHY Redux for UI state: The sidebar open/closed state must survive route
// changes and be readable by any component in the tree without prop drilling.
// useState would be lost on component unmount; React Query is for server data.
// Redux is the correct home for persistent client-only UI preferences.
//
// State shape:
//   sidebarOpen — true when the sidebar is fully expanded; false when collapsed to icon-only.
//
// Actions:
//   setSidebarOpen(boolean) — set the sidebar state explicitly
//   toggleSidebar()         — flip the current sidebar state

import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

export interface UiState {
  sidebarOpen: boolean;
}

const initialState: UiState = {
  sidebarOpen: true,
};

const uiSlice = createSlice({
  name: 'ui',
  initialState,
  reducers: {
    setSidebarOpen: (state, action: PayloadAction<boolean>) => {
      state.sidebarOpen = action.payload;
    },

    toggleSidebar: (state) => {
      state.sidebarOpen = !state.sidebarOpen;
    },
  },
});

export const { setSidebarOpen, toggleSidebar } = uiSlice.actions;

// Typed selector — reads the ui state slice from the Redux root.
// Uses a minimal structural type so the selector remains usable in isolated tests.
// Usage: const { sidebarOpen } = useAppSelector(selectUi);
export const selectUi = (state: { ui: UiState }) => state.ui;

export default uiSlice.reducer;
