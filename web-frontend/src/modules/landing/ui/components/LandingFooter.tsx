// LandingFooter.tsx
// The footer for the StackFlow marketing landing page.
//
// Layout (desktop): StackFlow wordmark on the left, two link columns in the
// centre, copyright line on the right.
// Layout (mobile): stacked vertically, copyright last.
//
// Uses the StackFlow design token CSS variables (--sf-*) defined in index.css.
// Background matches the base surface (#131313) — the footer recedes naturally
// without a hard border. A ghost border (outline-variant at 20% opacity) provides
// the single structural separator above the footer, as allowed by DESIGN.md for
// the one place a border is required structurally.

export function LandingFooter() {
  return (
    <footer
      className="w-full py-12 px-8"
      style={{
        backgroundColor: 'var(--sf-surface)',
        borderTop: '1px solid color-mix(in srgb, var(--sf-outline-variant) 20%, transparent)',
      }}
    >
      <div className="max-w-7xl mx-auto flex flex-col md:flex-row justify-between items-start md:items-center gap-8">
        {/* Wordmark */}
        <span
          className="text-xl font-bold tracking-tighter"
          style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif' }}
        >
          StackFlow
        </span>

        {/* Link columns */}
        <div className="flex gap-16">
          <div className="flex flex-col gap-3">
            <span
              className="text-xs font-semibold uppercase tracking-widest"
              style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif' }}
            >
              Product
            </span>
            <a
              href="#features"
              className="text-sm transition-colors duration-200"
              style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
              onMouseEnter={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface)')}
              onMouseLeave={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface-variant)')}
            >
              Features
            </a>
            <a
              href="#pricing"
              className="text-sm transition-colors duration-200"
              style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
              onMouseEnter={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface)')}
              onMouseLeave={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface-variant)')}
            >
              Pricing
            </a>
          </div>

          <div className="flex flex-col gap-3">
            <span
              className="text-xs font-semibold uppercase tracking-widest"
              style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif' }}
            >
              Support
            </span>
            <a
              href="#"
              className="text-sm transition-colors duration-200"
              style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
              onMouseEnter={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface)')}
              onMouseLeave={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface-variant)')}
            >
              Documentation
            </a>
            <a
              href="#"
              className="text-sm transition-colors duration-200"
              style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
              onMouseEnter={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface)')}
              onMouseLeave={(e) => (e.currentTarget.style.color = 'var(--sf-on-surface-variant)')}
            >
              Contact
            </a>
          </div>
        </div>

        {/* Copyright */}
        <p
          className="text-xs"
          style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
        >
          &copy; 2026 StackFlow. All rights reserved.
        </p>
      </div>
    </footer>
  );
}
