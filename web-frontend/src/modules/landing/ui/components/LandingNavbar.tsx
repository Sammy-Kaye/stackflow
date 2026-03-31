// LandingNavbar.tsx
// Fixed top navigation bar for the StackFlow marketing landing page.
//
// Behaviour:
//   - Transparent at scroll position 0.
//   - At scroll > 60px: bg-background/80 with backdrop blur and a ghost border below.
//   - "Enter app" button calls useDevLogin. Shows a Loader2 spinner while in-flight.
//     On error: Sonner toast "Could not connect to the server. Is the API running?"
//   - Mobile (< md): nav links hidden, hamburger icon shown.
//   - Hamburger opens a full-screen overlay with nav links and "Enter app" button.
//     Closing: click any link, click the button, or click the X icon.

import { useState, useEffect } from 'react';
import { Menu, X, Loader2 } from 'lucide-react';
import { useDevLogin } from '@/modules/auth/hooks/useDevLogin';

export function LandingNavbar() {
  const [scrolled, setScrolled] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const login = useDevLogin();

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 60);
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  // Close mobile menu on viewport resize past md breakpoint
  useEffect(() => {
    const onResize = () => {
      if (window.innerWidth >= 768) setMenuOpen(false);
    };
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize);
  }, []);

  // Prevent body scroll while mobile menu is open
  useEffect(() => {
    document.body.style.overflow = menuOpen ? 'hidden' : '';
    return () => { document.body.style.overflow = ''; };
  }, [menuOpen]);

  const handleEnterApp = () => {
    // Error handling is centralised in useDevLogin's onError callback —
    // no need to duplicate the toast here.
    login.mutate(undefined);
    setMenuOpen(false);
  };

  const navLinkStyle: React.CSSProperties = {
    color: 'var(--sf-on-surface-variant)',
    fontFamily: 'Manrope, sans-serif',
    fontSize: '0.875rem',
    fontWeight: 500,
    letterSpacing: '-0.01em',
    transition: 'color 200ms',
    textDecoration: 'none',
  };

  return (
    <>
      <nav
        className="fixed top-0 left-0 right-0 z-50 transition-all duration-300"
        style={
          scrolled
            ? {
                backgroundColor: 'color-mix(in srgb, var(--sf-surface) 80%, transparent)',
                backdropFilter: 'blur(8px)',
                WebkitBackdropFilter: 'blur(8px)',
                borderBottom: '1px solid color-mix(in srgb, var(--sf-outline-variant) 20%, transparent)',
              }
            : { backgroundColor: 'transparent' }
        }
      >
        <div className="max-w-7xl mx-auto flex items-center justify-between px-6 md:px-8 py-4">
          {/* Wordmark */}
          <span
            className="text-xl font-bold tracking-tighter select-none"
            style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif' }}
          >
            StackFlow
          </span>

          {/* Desktop nav links */}
          <div className="hidden md:flex items-center gap-8">
            <a
              href="#features"
              style={navLinkStyle}
              onMouseEnter={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface)')}
              onMouseLeave={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface-variant)')}
            >
              Features
            </a>
            <a
              href="#pricing"
              style={navLinkStyle}
              onMouseEnter={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface)')}
              onMouseLeave={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface-variant)')}
            >
              Pricing
            </a>
          </div>

          {/* Desktop CTA + mobile hamburger */}
          <div className="flex items-center gap-4">
            {/* Enter app — desktop only */}
            <button
              onClick={handleEnterApp}
              disabled={login.isPending}
              className="hidden md:inline-flex items-center gap-2 px-5 py-2 rounded-lg text-sm font-semibold transition-all duration-150 active:scale-95 disabled:opacity-60 disabled:cursor-not-allowed"
              style={{
                background: 'linear-gradient(135deg, var(--sf-primary), var(--sf-primary-container))',
                color: 'var(--sf-on-primary)',
                fontFamily: 'Manrope, sans-serif',
              }}
              onMouseEnter={(e) => {
                if (!login.isPending) {
                  (e.currentTarget as HTMLButtonElement).style.boxShadow =
                    '0 0 15px -5px var(--sf-primary)';
                }
              }}
              onMouseLeave={(e) => {
                (e.currentTarget as HTMLButtonElement).style.boxShadow = 'none';
              }}
            >
              {login.isPending && <Loader2 className="size-4 animate-spin" />}
              Enter app
            </button>

            {/* Hamburger — mobile only */}
            <button
              onClick={() => setMenuOpen(true)}
              aria-label="Open navigation menu"
              className="md:hidden flex items-center justify-center p-2 rounded-lg transition-colors"
              style={{ color: 'var(--sf-on-surface-variant)' }}
            >
              <Menu className="size-5" />
            </button>
          </div>
        </div>
      </nav>

      {/* Mobile full-screen overlay */}
      {menuOpen && (
        <div
          className="fixed inset-0 z-50 flex flex-col px-6 py-4"
          style={{ backgroundColor: 'var(--sf-surface)' }}
        >
          {/* Overlay header */}
          <div className="flex items-center justify-between">
            <span
              className="text-xl font-bold tracking-tighter"
              style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif' }}
            >
              StackFlow
            </span>
            <button
              onClick={() => setMenuOpen(false)}
              aria-label="Close navigation menu"
              className="flex items-center justify-center p-2 rounded-lg transition-colors"
              style={{ color: 'var(--sf-on-surface-variant)' }}
            >
              <X className="size-5" />
            </button>
          </div>

          {/* Overlay nav links */}
          <nav className="flex flex-col gap-6 mt-12">
            <a
              href="#features"
              onClick={() => setMenuOpen(false)}
              className="text-2xl font-semibold transition-colors duration-200"
              style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif', textDecoration: 'none' }}
            >
              Features
            </a>
            <a
              href="#pricing"
              onClick={() => setMenuOpen(false)}
              className="text-2xl font-semibold transition-colors duration-200"
              style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif', textDecoration: 'none' }}
            >
              Pricing
            </a>
          </nav>

          {/* Overlay CTA */}
          <div className="mt-auto pb-8">
            <button
              onClick={handleEnterApp}
              disabled={login.isPending}
              className="w-full flex items-center justify-center gap-2 px-5 py-4 rounded-lg text-base font-semibold transition-all duration-150 active:scale-95 disabled:opacity-60 disabled:cursor-not-allowed"
              style={{
                background: 'linear-gradient(135deg, var(--sf-primary), var(--sf-primary-container))',
                color: 'var(--sf-on-primary)',
                fontFamily: 'Manrope, sans-serif',
              }}
            >
              {login.isPending && <Loader2 className="size-5 animate-spin" />}
              Enter app
            </button>
          </div>
        </div>
      )}
    </>
  );
}
