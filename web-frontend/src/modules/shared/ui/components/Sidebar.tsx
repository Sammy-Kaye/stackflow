// Sidebar.tsx
// The primary navigation sidebar for the authenticated application shell.
//
// Reads sidebarOpen from Redux uiSlice and dispatches toggleSidebar when the
// collapse button is clicked. Sidebar open/closed state is global and persistent
// across route changes — it must live in Redux, not local useState.
//
// Layout behaviour:
//   - Expanded: w-56 (14rem) — shows icon + label for each nav item
//   - Collapsed: w-14 (3.5rem) — shows icon only; tooltips reveal labels on hover
//   - Width transitions smoothly via CSS transition-all to avoid a jarring snap
//
// Navigation items (all stub pages for Phase 1):
//   Workflows       → /workflows  (Workflow icon)
//   My Tasks        → /tasks      (CheckSquare icon)
//   Active Workflows → /active    (LayoutDashboard icon)
//
// The collapse toggle sits at the bottom of the sidebar. ChevronLeft when open,
// ChevronRight when closed.

import {
  Workflow,
  CheckSquare,
  LayoutDashboard,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { selectUi, toggleSidebar } from '@/store/uiSlice';
import { NavItem } from './NavItem';
import { cn } from '@/modules/shared/lib/utils';

const NAV_ITEMS = [
  {
    to: '/workflows',
    label: 'Workflows',
    icon: <Workflow className="size-5" />,
  },
  {
    to: '/tasks',
    label: 'My Tasks',
    icon: <CheckSquare className="size-5" />,
  },
  {
    to: '/active',
    label: 'Active Workflows',
    icon: <LayoutDashboard className="size-5" />,
  },
] as const;

export function Sidebar() {
  const dispatch = useAppDispatch();
  const { sidebarOpen } = useAppSelector(selectUi);

  return (
    <aside
      className={cn(
        'flex h-full flex-col bg-sidebar transition-all duration-200',
        sidebarOpen ? 'w-56' : 'w-14',
      )}
    >
      {/* Logo / brand area */}
      <div
        className={cn(
          'flex h-14 items-center border-b border-sidebar-border px-3 shrink-0',
          sidebarOpen ? 'gap-2.5' : 'justify-center',
        )}
      >
        <div className="flex size-7 items-center justify-center rounded-md bg-primary text-primary-foreground text-xs font-bold shrink-0">
          SF
        </div>
        {sidebarOpen && (
          <span className="text-sm font-semibold text-sidebar-foreground truncate">
            StackFlow
          </span>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex flex-1 flex-col gap-1 overflow-y-auto p-2">
        {NAV_ITEMS.map((item) => (
          <NavItem
            key={item.to}
            to={item.to}
            label={item.label}
            icon={item.icon}
            collapsed={!sidebarOpen}
          />
        ))}
      </nav>

      {/* Collapse toggle — bottom of sidebar */}
      <div className="shrink-0 border-t border-sidebar-border p-2">
        <button
          onClick={() => dispatch(toggleSidebar())}
          aria-label={sidebarOpen ? 'Collapse sidebar' : 'Expand sidebar'}
          className={cn(
            'flex w-full items-center rounded-lg px-3 py-2 text-sm text-muted-foreground',
            'hover:bg-accent hover:text-accent-foreground transition-colors',
            !sidebarOpen && 'justify-center px-2',
          )}
        >
          {sidebarOpen ? (
            <>
              <ChevronLeft className="size-4 shrink-0" />
              <span className="ml-2">Collapse</span>
            </>
          ) : (
            <ChevronRight className="size-4 shrink-0" />
          )}
        </button>
      </div>
    </aside>
  );
}
