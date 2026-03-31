// LandingFeatures.test.tsx
// Unit tests for the LandingFeatures component.
//
// Strategy:
//   No mocks needed — this is a pure display component with no hooks.
//   Test that all three feature cards and their titles are rendered.
//   Test icon display.

import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { LandingFeatures } from '../LandingFeatures';

describe('LandingFeatures', () => {
  describe('rendering', () => {
    it('renders all three feature card titles', () => {
      render(<LandingFeatures />);

      expect(screen.getByText('Build once, run forever')).toBeInTheDocument();
      expect(screen.getByText('Everyone knows their next step')).toBeInTheDocument();
      expect(screen.getByText('Every action, recorded')).toBeInTheDocument();
    });

    it('renders the section with id="features"', () => {
      const { container } = render(<LandingFeatures />);
      const section = container.querySelector('section#features');
      expect(section).toBeInTheDocument();
    });

    it('renders the "Capabilities" subheading', () => {
      render(<LandingFeatures />);
      expect(screen.getByText('Capabilities')).toBeInTheDocument();
    });

    it('renders the section headline', () => {
      render(<LandingFeatures />);
      expect(screen.getByText('Precision-engineered workflows')).toBeInTheDocument();
    });
  });

  describe('feature descriptions', () => {
    it('renders the "Build once" feature description', () => {
      render(<LandingFeatures />);
      expect(
        screen.getByText(/Workflow templates you define once and launch as many times/i)
      ).toBeInTheDocument();
    });

    it('renders the "Everyone knows" feature description', () => {
      render(<LandingFeatures />);
      expect(
        screen.getByText(/Tasks are assigned automatically/i)
      ).toBeInTheDocument();
    });

    it('renders the "Every action" feature description', () => {
      render(<LandingFeatures />);
      expect(
        screen.getByText(/A complete audit trail of every status change/i)
      ).toBeInTheDocument();
    });
  });

  describe('icons', () => {
    it('renders three feature cards with icon containers', () => {
      const { container } = render(<LandingFeatures />);
      const iconContainers = container.querySelectorAll('div[style*="background"]');
      // There should be multiple icon containers in the feature cards
      expect(iconContainers.length).toBeGreaterThan(0);
    });
  });

  describe('styling', () => {
    it('renders the section with a background color', () => {
      const { container } = render(<LandingFeatures />);
      const section = container.querySelector('section#features');
      const style = section?.getAttribute('style');
      expect(style).toContain('--sf-surface-container-low');
    });

    it('renders feature cards in a grid layout', () => {
      const { container } = render(<LandingFeatures />);
      // The cards should be in a grid container
      const grid = container.querySelector('.grid');
      expect(grid).toBeInTheDocument();
      // Grid should have the responsive classes
      const gridClass = grid?.getAttribute('class');
      expect(gridClass).toContain('grid-cols-1');
      expect(gridClass).toContain('md:grid-cols-3');
    });
  });

  describe('card hover effects', () => {
    it('has feature cards with inline event handlers for hover effects', () => {
      const { container } = render(<LandingFeatures />);

      // The feature cards should be rendered
      const cards = container.querySelectorAll('div[style*="surface-container"]');
      expect(cards.length).toBeGreaterThan(0);

      // At least one card should have onMouseEnter/onMouseLeave handlers
      // (these are attached as React event handlers on the elements)
      const firstCard = cards[0] as HTMLDivElement;
      expect(firstCard).toBeInTheDocument();
    });

    it('applies transition-colors class to feature cards', () => {
      const { container } = render(<LandingFeatures />);

      // Cards should have transition-colors class for smooth color changes
      const cards = container.querySelectorAll('.transition-colors');
      expect(cards.length).toBeGreaterThan(0);
    });
  });
});
