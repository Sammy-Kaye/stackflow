// TopBar.tsx
// The horizontal top bar displayed across the authenticated application shell.
//
// Reads email and workspaceId from Redux auth state.
// The workspaceId is displayed as a workspace label placeholder — a workspace name
// API does not exist yet (Feature 8+). The literal ID string is shown until then.
//
// User area (right side):
//   - Circular text avatar: first character of email, uppercased
//   - Email address text
//   - Logout button: dispatches clearCredentials and navigates to /dev-login
//
// Logout does not make an API call — Phase 1 tokens are in-memory only.
// Clearing Redux state is sufficient; the ProtectedRoute guard handles the redirect.

import { LogOut } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import { selectAuth, clearCredentials } from '@/store/authSlice';
import { Button } from '@/modules/shared/ui/components/button';

export function TopBar() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { email, workspaceId } = useAppSelector(selectAuth);

  const handleLogout = () => {
    dispatch(clearCredentials());
    navigate('/dev-login');
  };

  // Derive the display initial from the email — fall back to '?' if email is null.
  const initial = email ? email.charAt(0).toUpperCase() : '?';

  return (
    <header className="flex h-14 shrink-0 items-center justify-between border-b border-border bg-background px-4">
      {/* Workspace label — left side */}
      <div className="flex items-center gap-2">
        <span className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
          Workspace
        </span>
        <span className="text-sm font-medium text-foreground truncate max-w-[200px]">
          {workspaceId ?? '—'}
        </span>
      </div>

      {/* User area — right side */}
      <div className="flex items-center gap-3">
        {/* Text initials avatar */}
        <div className="flex size-8 items-center justify-center rounded-full bg-primary text-primary-foreground text-xs font-semibold shrink-0">
          {initial}
        </div>

        {/* Email */}
        <span className="text-sm text-muted-foreground hidden sm:block">
          {email ?? ''}
        </span>

        {/* Logout button */}
        <Button
          variant="ghost"
          size="icon-sm"
          onClick={handleLogout}
          aria-label="Log out"
          title="Log out"
        >
          <LogOut className="size-4" />
        </Button>
      </div>
    </header>
  );
}
