// LandingCta.tsx
// Bottom call-to-action section for the StackFlow landing page.
//
// Centred card with headline, subtext, and an "Enter app" button.
// The card uses bg-surface-container-high for elevation above the page background.
// An ambient glow decoration (blur circle) adds depth behind the card per DESIGN.md.

import { Loader2 } from 'lucide-react';
import { useDevLogin } from '@/modules/auth/hooks/useDevLogin';

export function LandingCta() {
  const login = useDevLogin();

  const handleEnterApp = () => {
    // Error handling is centralised in useDevLogin's onError callback.
    login.mutate(undefined);
  };

  return (
    <section
      className="py-24 px-8"
      style={{ backgroundColor: 'var(--sf-surface)' }}
    >
      <div className="max-w-4xl mx-auto">
        <div
          className="relative p-16 rounded-2xl text-center space-y-6 overflow-hidden"
          style={{ backgroundColor: 'var(--sf-surface-container-high)' }}
        >
          {/* Ambient glow decoration */}
          <div
            className="absolute top-0 right-0 w-64 h-64 rounded-full pointer-events-none blur-[120px]"
            style={{ backgroundColor: 'color-mix(in srgb, var(--sf-primary) 10%, transparent)' }}
          />

          <h2
            className="relative text-4xl font-extrabold tracking-tight"
            style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif', letterSpacing: '-0.02em', zIndex: 1 }}
          >
            Ready to stop managing work manually?
          </h2>

          <p
            className="relative max-w-xl mx-auto"
            style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif', zIndex: 1 }}
          >
            Join teams that have made their workflows intelligent.
          </p>

          <div className="relative pt-2" style={{ zIndex: 1 }}>
            <button
              onClick={handleEnterApp}
              disabled={login.isPending}
              className="inline-flex items-center gap-2 px-12 py-4 rounded-lg text-lg font-bold transition-all duration-150 active:scale-95 disabled:opacity-60 disabled:cursor-not-allowed"
              style={{
                background: 'linear-gradient(135deg, var(--sf-primary), var(--sf-primary-container))',
                color: 'var(--sf-on-primary)',
                fontFamily: 'Manrope, sans-serif',
                boxShadow: '0 0 20px -5px var(--sf-primary)',
              }}
              onMouseEnter={(e) => {
                if (!login.isPending) {
                  (e.currentTarget as HTMLButtonElement).style.boxShadow =
                    '0 0 25px -3px var(--sf-primary)';
                }
              }}
              onMouseLeave={(e) => {
                (e.currentTarget as HTMLButtonElement).style.boxShadow =
                  '0 0 20px -5px var(--sf-primary)';
              }}
            >
              {login.isPending && <Loader2 className="size-5 animate-spin" />}
              Enter app
            </button>
          </div>
        </div>
      </div>
    </section>
  );
}
