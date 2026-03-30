// UnauthenticatedLayout.tsx
// Full-screen centered layout for unauthenticated pages.
//
// Used by auth pages (login, register, OTP, password reset) in Phase 2.
// In Phase 1 the DevLoginPage manages its own layout, so this component is
// scaffolded here but not yet wired into the router.
//
// When Phase 2 auth pages are built, they will use this as their layout wrapper
// via the router's layout route pattern — replacing DevLoginPage's inline layout.
//
// No sidebar, no top bar. The only content is the centered {children} card.

interface UnauthenticatedLayoutProps {
  children: React.ReactNode;
}

export function UnauthenticatedLayout({ children }: UnauthenticatedLayoutProps) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      {children}
    </div>
  );
}
