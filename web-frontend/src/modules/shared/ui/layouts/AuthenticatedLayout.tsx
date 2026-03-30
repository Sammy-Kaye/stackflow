// AuthenticatedLayout.tsx
// The root layout for all authenticated screens.
//
// Structure:
//   ┌─────────────────────────────────────────┐
//   │ TopBar (h-14, fixed to top)             │
//   ├───────────┬─────────────────────────────┤
//   │ Sidebar   │ <main> (Outlet)             │
//   │ (h-full)  │ scrolls independently       │
//   └───────────┴─────────────────────────────┘
//
// The outer shell takes the full viewport height (h-screen). The top bar is fixed
// at the top. Below the top bar, the sidebar and main content area fill the
// remaining height. The sidebar does not scroll. Only the <main> area scrolls.
//
// The Sidebar handles its own open/closed state via Redux. AuthenticatedLayout
// does not need to know about it — the sidebar's own CSS transition handles the
// width change.
//
// <Outlet /> renders the matched child route — e.g. WorkflowsPage at /workflows.

import { Outlet } from 'react-router-dom';
import { Sidebar } from '../components/Sidebar';
import { TopBar } from '../components/TopBar';

export function AuthenticatedLayout() {
  return (
    <div className="flex h-screen flex-col overflow-hidden bg-background">
      {/* Top bar — spans the full width above the sidebar + content */}
      <TopBar />

      {/* Body: sidebar + main content side by side */}
      <div className="flex flex-1 overflow-hidden">
        <Sidebar />

        {/* Main content area — only this region scrolls */}
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
