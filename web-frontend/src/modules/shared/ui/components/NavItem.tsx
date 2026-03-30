// NavItem.tsx
// A single navigation entry in the sidebar.
//
// Props:
//   to        — the route path this item links to
//   label     — the human-readable label shown next to the icon
//   icon      — a lucide-react icon element
//   collapsed — when true, renders icon-only with a tooltip; when false, renders icon + label
//
// Active state: NavLink from react-router-dom applies the `isActive` class when the
// current URL matches the `to` path. We use this to apply a highlighted background.
//
// Collapsed tooltip: uses the shared shadcn/ui Tooltip component (base-nova style,
// wrapping @base-ui/react) so the user still knows which item they are hovering
// when the sidebar is icon-only.
//
// The NavLink is passed directly via the `render` prop on TooltipTrigger so no
// extra wrapper element is inserted into the DOM.

import { NavLink } from 'react-router-dom';
import { Tooltip, TooltipContent, TooltipTrigger } from './tooltip';
import { cn } from '@/modules/shared/lib/utils';

interface NavItemProps {
  to: string;
  label: string;
  icon: React.ReactNode;
  collapsed: boolean;
}

export function NavItem({ to, label, icon, collapsed }: NavItemProps) {
  const linkClasses = ({ isActive }: { isActive: boolean }) =>
    cn(
      'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
      'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
      isActive && 'bg-accent text-accent-foreground',
      collapsed && 'justify-center px-2.5',
    );

  // When collapsed, wrap in a tooltip so the label is still discoverable on hover.
  // When expanded the label is always visible so the tooltip is unnecessary.
  if (collapsed) {
    return (
      <Tooltip>
        <TooltipTrigger
          render={
            <NavLink to={to} className={linkClasses}>
              <span className="shrink-0">{icon}</span>
            </NavLink>
          }
        />
        <TooltipContent side="right" sideOffset={8}>
          {label}
        </TooltipContent>
      </Tooltip>
    );
  }

  return (
    <NavLink to={to} className={linkClasses}>
      <span className="shrink-0">{icon}</span>
      <span>{label}</span>
    </NavLink>
  );
}
