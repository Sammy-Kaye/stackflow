// LandingPage.tsx
// Route-level page component for the StackFlow marketing landing page.
//
// This page is publicly accessible at / and is wrapped in GuestRoute —
// authenticated users are redirected to /workflows before this renders.
//
// Composition order matches the Feature Brief:
//   LandingNavbar → LandingHero → LandingFeatures → LandingHowItWorks
//     → LandingPricing → LandingCta → LandingFooter
//
// The root element provides the base surface and text colour for the entire
// page, establishing the design system canvas before any section renders.

import { LandingNavbar } from '../components/LandingNavbar';
import { LandingHero } from '../components/LandingHero';
import { LandingFeatures } from '../components/LandingFeatures';
import { LandingHowItWorks } from '../components/LandingHowItWorks';
import { LandingPricing } from '../components/LandingPricing';
import { LandingCta } from '../components/LandingCta';
import { LandingFooter } from '../components/LandingFooter';

export function LandingPage() {
  return (
    <div
      className="min-h-screen"
      style={{
        backgroundColor: 'var(--sf-surface)',
        color: 'var(--sf-on-surface)',
        fontFamily: 'Inter, sans-serif',
      }}
    >
      <LandingNavbar />

      <main>
        <LandingHero />
        <LandingFeatures />
        <LandingHowItWorks />
        <LandingPricing />
        <LandingCta />
      </main>

      <LandingFooter />
    </div>
  );
}
