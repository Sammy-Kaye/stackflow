// LandingHero.tsx
// Full-viewport-height hero section for the StackFlow landing page.
//
// Content:
//   - Headline: large Manrope display text
//   - Subheadline: Inter body text
//   - Primary CTA: teal gradient button, same useDevLogin wiring as navbar
//   - Dashboard mockup: a purely static div-based visual. No real app components
//     are imported here — that would couple the landing page to internal modules
//     and force rebuilds whenever those components change. The mockup uses
//     StackFlow design token CSS variables to match the real app's aesthetic.

import { Loader2 } from 'lucide-react';
import { useDevLogin } from '@/modules/auth/hooks/useDevLogin';

export function LandingHero() {
  const login = useDevLogin();

  const handleEnterApp = () => {
    // Error handling is centralised in useDevLogin's onError callback.
    login.mutate(undefined);
  };

  return (
    <section
      className="min-h-screen flex flex-col items-center justify-start pt-28 pb-20 px-6 text-center"
      style={{ backgroundColor: 'var(--sf-surface)' }}
    >
      {/* Text content */}
      <div className="max-w-4xl mx-auto space-y-6 mb-16">
        <h1
          className="text-5xl md:text-7xl font-extrabold tracking-tighter leading-[1.08]"
          style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif', letterSpacing: '-0.02em' }}
        >
          Workflows that think.{' '}
          <span style={{ color: 'var(--sf-primary)' }}>Teams that flow.</span>
        </h1>

        <p
          className="text-lg md:text-xl max-w-2xl mx-auto leading-relaxed"
          style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
        >
          Stop managing work in spreadsheets and email chains. StackFlow gives your team
          a living, adaptive process engine.
        </p>

        <div className="flex flex-col sm:flex-row items-center justify-center gap-4 pt-2">
          <button
            onClick={handleEnterApp}
            disabled={login.isPending}
            className="inline-flex items-center gap-2 px-8 py-4 rounded-lg text-base font-bold transition-all duration-150 active:scale-95 disabled:opacity-60 disabled:cursor-not-allowed"
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
            {login.isPending && <Loader2 className="size-5 animate-spin" />}
            Enter app
          </button>
        </div>
      </div>

      {/* Dashboard mockup — static visual only, no real component imports */}
      <div className="relative w-full max-w-5xl mx-auto px-4">
        {/* Ambient glow decorations */}
        <div
          className="absolute -top-16 -left-16 w-72 h-72 rounded-full pointer-events-none blur-[100px]"
          style={{ backgroundColor: 'color-mix(in srgb, var(--sf-primary) 5%, transparent)' }}
        />
        <div
          className="absolute -bottom-16 -right-16 w-72 h-72 rounded-full pointer-events-none blur-[100px]"
          style={{ backgroundColor: 'color-mix(in srgb, var(--sf-primary) 5%, transparent)' }}
        />

        {/* Mockup shell */}
        <div
          className="relative rounded-xl overflow-hidden"
          style={{
            backgroundColor: 'var(--sf-surface-container-high)',
            border: '1px solid color-mix(in srgb, var(--sf-outline-variant) 15%, transparent)',
            boxShadow: '0px 8px 32px rgba(229,226,225,0.04)',
          }}
        >
          {/* App chrome: top bar */}
          <div
            className="flex items-center gap-3 px-4 py-3"
            style={{ backgroundColor: 'var(--sf-surface-container-highest)' }}
          >
            {/* Fake traffic-light dots */}
            <div className="flex gap-1.5">
              <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'color-mix(in srgb, var(--sf-outline-variant) 60%, transparent)' }} />
              <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'color-mix(in srgb, var(--sf-outline-variant) 60%, transparent)' }} />
              <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'color-mix(in srgb, var(--sf-outline-variant) 60%, transparent)' }} />
            </div>
            <div
              className="flex-1 h-5 rounded"
              style={{ backgroundColor: 'color-mix(in srgb, var(--sf-outline-variant) 20%, transparent)', maxWidth: '240px' }}
            />
            <div className="ml-auto flex gap-2">
              <div className="w-6 h-6 rounded" style={{ backgroundColor: 'color-mix(in srgb, var(--sf-outline-variant) 20%, transparent)' }} />
              <div className="w-6 h-6 rounded" style={{ backgroundColor: 'color-mix(in srgb, var(--sf-outline-variant) 20%, transparent)' }} />
            </div>
          </div>

          {/* App body: sidebar + content */}
          <div className="flex" style={{ height: '340px' }}>
            {/* Sidebar stub */}
            <div
              className="flex flex-col gap-2 p-3 shrink-0"
              style={{ width: '180px', backgroundColor: 'var(--sf-surface-container-low)' }}
            >
              {/* Brand */}
              <div className="flex items-center gap-2 px-2 py-2 mb-2">
                <div
                  className="w-7 h-7 rounded-md flex items-center justify-center text-xs font-bold"
                  style={{ backgroundColor: 'var(--sf-primary)', color: 'var(--sf-on-primary)', fontFamily: 'Manrope, sans-serif' }}
                >
                  SF
                </div>
                <span
                  className="text-sm font-semibold"
                  style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif' }}
                >
                  StackFlow
                </span>
              </div>
              {/* Nav items */}
              {['Workflows', 'My Tasks', 'Active'].map((label, i) => (
                <div
                  key={label}
                  className="flex items-center gap-2 px-3 py-2 rounded-lg"
                  style={{
                    backgroundColor: i === 0
                      ? 'color-mix(in srgb, var(--sf-primary) 10%, transparent)'
                      : 'transparent',
                    color: i === 0 ? 'var(--sf-primary)' : 'var(--sf-on-surface-variant)',
                    fontFamily: 'Inter, sans-serif',
                    fontSize: '0.8rem',
                  }}
                >
                  <div
                    className="w-4 h-4 rounded"
                    style={{ backgroundColor: 'color-mix(in srgb, currentColor 30%, transparent)' }}
                  />
                  {label}
                </div>
              ))}
            </div>

            {/* Main content area */}
            <div
              className="flex-1 p-5 overflow-hidden"
              style={{ backgroundColor: 'var(--sf-surface-container)' }}
            >
              {/* Page header */}
              <div className="flex items-center justify-between mb-5">
                <div
                  className="h-5 w-28 rounded"
                  style={{ backgroundColor: 'color-mix(in srgb, var(--sf-on-surface) 15%, transparent)' }}
                />
                <div
                  className="h-7 w-24 rounded-lg"
                  style={{ background: 'linear-gradient(135deg, var(--sf-primary), var(--sf-primary-container))' }}
                />
              </div>

              {/* Workflow cards grid */}
              <div className="grid grid-cols-2 gap-3">
                {[
                  { name: 'Employee Onboarding', status: 'Active', steps: 8 },
                  { name: 'Purchase Approval', status: 'Active', steps: 5 },
                  { name: 'Client Offboarding', status: 'Active', steps: 6 },
                  { name: 'Vendor Review', status: 'Draft', steps: 4 },
                ].map((wf) => (
                  <div
                    key={wf.name}
                    className="p-4 rounded-lg space-y-3"
                    style={{ backgroundColor: 'var(--sf-surface-container-high)' }}
                  >
                    <div className="flex items-start justify-between">
                      <span
                        className="text-xs font-semibold leading-tight"
                        style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif', maxWidth: '80%' }}
                      >
                        {wf.name}
                      </span>
                      <span
                        className="text-xs px-1.5 py-0.5 rounded-full shrink-0"
                        style={{
                          backgroundColor: wf.status === 'Active'
                            ? 'color-mix(in srgb, var(--sf-primary) 15%, transparent)'
                            : 'color-mix(in srgb, var(--sf-outline-variant) 30%, transparent)',
                          color: wf.status === 'Active' ? 'var(--sf-primary)' : 'var(--sf-on-surface-variant)',
                          fontFamily: 'Inter, sans-serif',
                        }}
                      >
                        {wf.status}
                      </span>
                    </div>
                    <div
                      className="h-1.5 rounded-full overflow-hidden"
                      style={{ backgroundColor: 'color-mix(in srgb, var(--sf-outline-variant) 20%, transparent)' }}
                    >
                      <div
                        className="h-full rounded-full"
                        style={{
                          width: `${Math.round((wf.steps / 10) * 60 + 20)}%`,
                          background: 'linear-gradient(135deg, var(--sf-primary), var(--sf-primary-container))',
                        }}
                      />
                    </div>
                    <span
                      className="text-xs"
                      style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
                    >
                      {wf.steps} steps
                    </span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
