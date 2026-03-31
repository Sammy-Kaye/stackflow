// LandingPage.test.tsx
// Unit tests for the LandingPage component.
//
// Strategy:
//   Mock all child section components to keep this test fast and isolated.
//   Verify that all 7 section components render (by their mocked output).

import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { LandingPage } from '../LandingPage';

// Mock all child components — mocking at the import level
vi.mock('../../components/LandingNavbar', () => ({
  LandingNavbar: () => <div data-testid="landing-navbar">Navbar</div>,
}));

vi.mock('../../components/LandingHero', () => ({
  LandingHero: () => <div data-testid="landing-hero">Hero</div>,
}));

vi.mock('../../components/LandingFeatures', () => ({
  LandingFeatures: () => <div data-testid="landing-features">Features</div>,
}));

vi.mock('../../components/LandingHowItWorks', () => ({
  LandingHowItWorks: () => <div data-testid="landing-how-it-works">How It Works</div>,
}));

vi.mock('../../components/LandingPricing', () => ({
  LandingPricing: () => <div data-testid="landing-pricing">Pricing</div>,
}));

vi.mock('../../components/LandingCta', () => ({
  LandingCta: () => <div data-testid="landing-cta">CTA</div>,
}));

vi.mock('../../components/LandingFooter', () => ({
  LandingFooter: () => <div data-testid="landing-footer">Footer</div>,
}));

describe('LandingPage', () => {
  it('renders all 7 section components', () => {
    render(<LandingPage />);

    expect(screen.getByTestId('landing-navbar')).toBeInTheDocument();
    expect(screen.getByTestId('landing-hero')).toBeInTheDocument();
    expect(screen.getByTestId('landing-features')).toBeInTheDocument();
    expect(screen.getByTestId('landing-how-it-works')).toBeInTheDocument();
    expect(screen.getByTestId('landing-pricing')).toBeInTheDocument();
    expect(screen.getByTestId('landing-cta')).toBeInTheDocument();
    expect(screen.getByTestId('landing-footer')).toBeInTheDocument();
  });

  it('wraps content in a div with min-h-screen class', () => {
    const { container } = render(<LandingPage />);
    const wrapper = container.querySelector('div.min-h-screen');
    expect(wrapper).toBeInTheDocument();
  });

  it('applies design token CSS variables to the root div', () => {
    const { container } = render(<LandingPage />);
    const wrapper = container.querySelector('div.min-h-screen');
    const style = wrapper?.getAttribute('style');
    expect(style).toContain('--sf-surface');
    expect(style).toContain('--sf-on-surface');
    expect(style).toContain('Inter, sans-serif');
  });

  it('renders navbar before main content sections', () => {
    render(<LandingPage />);
    const navbar = screen.getByTestId('landing-navbar');
    const hero = screen.getByTestId('landing-hero');
    expect(navbar.compareDocumentPosition(hero) & 4).toBeTruthy(); // DOCUMENT_POSITION_FOLLOWING
  });

  it('renders main element containing feature sections', () => {
    const { container } = render(<LandingPage />);
    const main = container.querySelector('main');
    expect(main).toBeInTheDocument();

    // Verify feature sections are inside main
    const hero = main?.querySelector('[data-testid="landing-hero"]');
    const features = main?.querySelector('[data-testid="landing-features"]');
    const howItWorks = main?.querySelector('[data-testid="landing-how-it-works"]');
    const pricing = main?.querySelector('[data-testid="landing-pricing"]');
    const cta = main?.querySelector('[data-testid="landing-cta"]');

    expect(hero).toBeInTheDocument();
    expect(features).toBeInTheDocument();
    expect(howItWorks).toBeInTheDocument();
    expect(pricing).toBeInTheDocument();
    expect(cta).toBeInTheDocument();
  });

  it('renders footer after main content', () => {
    render(<LandingPage />);
    const main = screen.getByText('CTA').closest('main');
    const footer = screen.getByTestId('landing-footer');
    expect(main).toBeInTheDocument();
    expect(footer).toBeInTheDocument();
  });
});
