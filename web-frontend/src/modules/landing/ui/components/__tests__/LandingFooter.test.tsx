// LandingFooter.test.tsx
// Unit tests for the LandingFooter component.
//
// Strategy:
//   No mocks needed — this is a pure display component with no hooks.
//   Test that the wordmark, links, and copyright line are rendered.

import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { LandingFooter } from '../LandingFooter';

describe('LandingFooter', () => {
  describe('rendering', () => {
    it('renders the StackFlow wordmark', () => {
      render(<LandingFooter />);

      const wordmarks = screen.getAllByText('StackFlow');
      expect(wordmarks.length).toBeGreaterThan(0);
    });

    it('renders the copyright line', () => {
      render(<LandingFooter />);

      expect(screen.getByText(/© 2026 StackFlow. All rights reserved/i)).toBeInTheDocument();
    });

    it('renders as a footer element', () => {
      const { container } = render(<LandingFooter />);
      const footer = container.querySelector('footer');
      expect(footer).toBeInTheDocument();
    });
  });

  describe('Product section links', () => {
    it('renders "Product" section header', () => {
      render(<LandingFooter />);

      const headers = screen.getAllByText('Product');
      expect(headers.length).toBeGreaterThan(0);
    });

    it('renders "Features" link', () => {
      render(<LandingFooter />);

      const featureLinks = screen.getAllByText('Features');
      const footerFeatureLink = featureLinks.find(
        (el) => el.closest('a')?.getAttribute('href') === '#features'
      );
      expect(footerFeatureLink).toBeInTheDocument();
    });

    it('renders "Pricing" link', () => {
      render(<LandingFooter />);

      const pricingLinks = screen.getAllByText('Pricing');
      const footerPricingLink = pricingLinks.find(
        (el) => el.closest('a')?.getAttribute('href') === '#pricing'
      );
      expect(footerPricingLink).toBeInTheDocument();
    });
  });

  describe('Support section links', () => {
    it('renders "Support" section header', () => {
      render(<LandingFooter />);

      const headers = screen.getAllByText('Support');
      expect(headers.length).toBeGreaterThan(0);
    });

    it('renders "Documentation" link', () => {
      render(<LandingFooter />);

      expect(screen.getByText('Documentation')).toBeInTheDocument();
    });

    it('renders "Contact" link', () => {
      render(<LandingFooter />);

      expect(screen.getByText('Contact')).toBeInTheDocument();
    });
  });

  describe('link navigation', () => {
    it('renders Feature link with correct href', () => {
      render(<LandingFooter />);

      const featureLink = screen.getAllByText('Features').find(
        (el) => el.closest('a')
      )?.closest('a');
      expect(featureLink?.getAttribute('href')).toBe('#features');
    });

    it('renders Pricing link with correct href', () => {
      render(<LandingFooter />);

      const pricingLink = screen.getAllByText('Pricing').find(
        (el) => el.closest('a')
      )?.closest('a');
      expect(pricingLink?.getAttribute('href')).toBe('#pricing');
    });
  });

  describe('styling', () => {
    it('renders footer with correct background color', () => {
      const { container } = render(<LandingFooter />);
      const footer = container.querySelector('footer');
      const style = footer?.getAttribute('style');
      expect(style).toContain('--sf-surface');
    });

    it('renders footer with a border top', () => {
      const { container } = render(<LandingFooter />);
      const footer = container.querySelector('footer');
      const style = footer?.getAttribute('style');
      expect(style).toContain('border');
    });

    it('renders links with text color styling', () => {
      render(<LandingFooter />);

      const links = screen.getAllByRole('link');
      expect(links.length).toBeGreaterThan(0);

      // Each link should have style attributes with color
      links.forEach((link) => {
        const style = link.getAttribute('style');
        expect(style).toBeTruthy();
      });
    });
  });

  describe('layout', () => {
    it('renders footer with flex layout', () => {
      const { container } = render(<LandingFooter />);
      const contentDiv = container.querySelector('.flex');
      expect(contentDiv).toBeInTheDocument();
    });

    it('renders link columns with gap', () => {
      const { container } = render(<LandingFooter />);
      const linksContainer = container.querySelector('.flex.gap-16');
      expect(linksContainer).toBeInTheDocument();
    });
  });
});
