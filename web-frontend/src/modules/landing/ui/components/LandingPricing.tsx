// LandingPricing.tsx
// Three-tier pricing section for the StackFlow landing page.
// Section id="pricing" matches the navbar anchor links.
//
// Tiers:
//   Starter  — $0/mo  — outline CTA (ghost variant)
//   Team     — $29/mo — teal gradient CTA, "MOST POPULAR" badge, ring highlight, scale-105
//   Pro      — $79/mo — teal gradient CTA
//
// All CTA buttons call useDevLogin and navigate to /workflows on success.
// The teal gradient pattern and glow hover effect match all other primary CTAs.

import { Check, Loader2 } from 'lucide-react';
import { useDevLogin } from '@/modules/auth/hooks/useDevLogin';

// Each pricing CTA button gets its own useDevLogin instance so that only the
// clicked button shows a spinner — not all three simultaneously.
function PricingCtaButton({ label, highlighted }: { label: string; highlighted: boolean }) {
  const login = useDevLogin();
  return highlighted || label !== 'Get started free' ? (
    <button
      onClick={() => login.mutate(undefined)}
      disabled={login.isPending}
      className="w-full flex items-center justify-center gap-2 py-3 rounded-lg font-bold text-sm transition-all duration-150 active:scale-95 disabled:opacity-60 disabled:cursor-not-allowed"
      style={{
        background: 'linear-gradient(135deg, var(--sf-primary), var(--sf-primary-container))',
        color: 'var(--sf-on-primary)',
        fontFamily: 'Manrope, sans-serif',
        boxShadow: '0 0 15px -5px var(--sf-primary)',
      }}
      onMouseEnter={(e) => {
        if (!login.isPending)
          (e.currentTarget as HTMLButtonElement).style.boxShadow = '0 0 20px -3px var(--sf-primary)';
      }}
      onMouseLeave={(e) => {
        (e.currentTarget as HTMLButtonElement).style.boxShadow = '0 0 15px -5px var(--sf-primary)';
      }}
    >
      {login.isPending && <Loader2 className="size-4 animate-spin" />}
      {label}
    </button>
  ) : (
    <button
      onClick={() => login.mutate(undefined)}
      disabled={login.isPending}
      className="w-full flex items-center justify-center gap-2 py-3 rounded-lg font-bold text-sm transition-colors duration-150 active:scale-95 disabled:opacity-60 disabled:cursor-not-allowed"
      style={{
        backgroundColor: 'transparent',
        border: '1px solid color-mix(in srgb, var(--sf-outline-variant) 20%, transparent)',
        color: 'var(--sf-on-surface)',
        fontFamily: 'Manrope, sans-serif',
      }}
      onMouseEnter={(e) => {
        (e.currentTarget as HTMLButtonElement).style.backgroundColor = 'var(--sf-surface-bright)';
      }}
      onMouseLeave={(e) => {
        (e.currentTarget as HTMLButtonElement).style.backgroundColor = 'transparent';
      }}
    >
      {login.isPending && <Loader2 className="size-4 animate-spin" />}
      {label}
    </button>
  );
}

const TIERS = [
  {
    id: 'starter',
    name: 'Starter',
    price: '$0',
    period: '/mo',
    tagline: 'For individuals',
    features: [
      '3 workflows',
      '5 team members',
      'Basic templates',
      'Email notifications',
    ],
    cta: 'Get started free',
    highlighted: false,
  },
  {
    id: 'team',
    name: 'Team',
    price: '$29',
    period: '/mo',
    tagline: 'For growing teams',
    features: [
      'Unlimited workflows',
      '25 team members',
      'All node types',
      'Audit trail',
      'Priority support',
    ],
    cta: 'Start with Team',
    highlighted: true,
  },
  {
    id: 'pro',
    name: 'Pro',
    price: '$79',
    period: '/mo',
    tagline: 'For growing organizations',
    features: [
      'Everything in Team',
      'Unlimited members',
      'Custom branding',
      'API access',
      'SLA',
    ],
    cta: 'Start with Pro',
    highlighted: false,
  },
] as const;

export function LandingPricing() {

  return (
    <section
      id="pricing"
      className="py-32 px-8"
      style={{ backgroundColor: 'var(--sf-surface-container-low)' }}
    >
      <div className="max-w-7xl mx-auto">
        {/* Section header */}
        <div className="text-center mb-16">
          <h2
            className="text-4xl font-bold tracking-tight mb-4"
            style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif', letterSpacing: '-0.02em' }}
          >
            Scalable pricing for every team
          </h2>
          <p
            style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
          >
            Fair pricing that grows as your efficiency improves.
          </p>
        </div>

        {/* Tier cards — aligned to bottom so Team card appears taller */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 items-end">
          {TIERS.map((tier) => (
            <div
              key={tier.id}
              className="relative flex flex-col p-10 rounded-xl space-y-8 transition-transform"
              style={{
                backgroundColor: 'var(--sf-surface-container)',
                ...(tier.highlighted
                  ? {
                      transform: 'scale(1.05)',
                      outline: '2px solid color-mix(in srgb, var(--sf-primary) 50%, transparent)',
                      outlineOffset: '0px',
                    }
                  : {}),
              }}
            >
              {/* "MOST POPULAR" badge */}
              {tier.highlighted && (
                <div
                  className="absolute -top-4 left-1/2 -translate-x-1/2 px-4 py-1 rounded-full text-xs font-bold whitespace-nowrap"
                  style={{
                    backgroundColor: 'var(--sf-primary)',
                    color: 'var(--sf-on-primary)',
                    fontFamily: 'Manrope, sans-serif',
                  }}
                >
                  MOST POPULAR
                </div>
              )}

              {/* Price info */}
              <div className="space-y-2">
                <h3
                  className="text-xl font-bold"
                  style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif' }}
                >
                  {tier.name}
                </h3>
                <div className="flex items-baseline gap-1">
                  <span
                    className="text-4xl font-extrabold"
                    style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif' }}
                  >
                    {tier.price}
                  </span>
                  <span
                    className="text-sm"
                    style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
                  >
                    {tier.period}
                  </span>
                </div>
                <p
                  className="text-sm"
                  style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
                >
                  {tier.tagline}
                </p>
              </div>

              {/* Feature list */}
              <ul className="flex-1 space-y-3">
                {tier.features.map((feature) => (
                  <li
                    key={feature}
                    className="flex items-center gap-3 text-sm"
                    style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
                  >
                    <Check
                      className="size-4 shrink-0"
                      style={{ color: 'var(--sf-primary)' }}
                    />
                    {feature}
                  </li>
                ))}
              </ul>

              {/* CTA button — own useDevLogin instance via PricingCtaButton */}
              <PricingCtaButton label={tier.cta} highlighted={tier.highlighted} />
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
