// LandingHowItWorks.test.tsx
// Unit tests for the LandingHowItWorks component.
//
// Strategy:
//   No mocks needed — this is a pure display component with no hooks.
//   Test that all three steps and their titles are rendered.

import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { LandingHowItWorks } from '../LandingHowItWorks';

describe('LandingHowItWorks', () => {
  describe('rendering', () => {
    it('renders all three step titles', () => {
      render(<LandingHowItWorks />);

      expect(screen.getByText('Build')).toBeInTheDocument();
      expect(screen.getByText('Assign and launch')).toBeInTheDocument();
      expect(screen.getByText('Track and complete')).toBeInTheDocument();
    });

    it('renders the section headline', () => {
      render(<LandingHowItWorks />);
      expect(screen.getByText('Up and running in minutes')).toBeInTheDocument();
    });

    it('renders the section subheadline', () => {
      render(<LandingHowItWorks />);
      expect(
        screen.getByText(/The path from manual chaos to automated efficiency/i)
      ).toBeInTheDocument();
    });
  });

  describe('step descriptions', () => {
    it('renders the "Build" step description', () => {
      render(<LandingHowItWorks />);
      expect(
        screen.getByText(/Design your workflow once in the drag-and-drop builder/i)
      ).toBeInTheDocument();
    });

    it('renders the "Assign and launch" step description', () => {
      render(<LandingHowItWorks />);
      expect(
        screen.getByText(/Trigger a run for a client, project, or employee/i)
      ).toBeInTheDocument();
    });

    it('renders the "Track and complete" step description', () => {
      render(<LandingHowItWorks />);
      expect(
        screen.getByText(/Watch every step progress in real time/i)
      ).toBeInTheDocument();
    });
  });

  describe('step numbers', () => {
    it('displays step numbers 1, 2, 3', () => {
      render(<LandingHowItWorks />);

      const stepNumbers = ['1', '2', '3'];
      stepNumbers.forEach((number) => {
        // Each number appears as text content in a number circle
        const elements = screen.getAllByText(number);
        expect(elements.length).toBeGreaterThan(0);
      });
    });
  });

  describe('layout and styling', () => {
    it('renders the steps in a flex container', () => {
      const { container } = render(<LandingHowItWorks />);
      // The steps should be in a flex layout
      const stepsContainer = container.querySelector('.flex.flex-col.md\\:flex-row');
      expect(stepsContainer).toBeInTheDocument();
    });

    it('renders the connector line on desktop (hidden on mobile)', () => {
      const { container } = render(<LandingHowItWorks />);
      // The connector line is a div with hidden md:block classes
      const connectorLine = container.querySelector('.hidden.md\\:block');
      expect(connectorLine).toBeInTheDocument();
    });

    it('renders step circles with borders', () => {
      const { container } = render(<LandingHowItWorks />);
      // Each step should have a number circle with a border
      const circles = container.querySelectorAll('div[style*="border"]');
      expect(circles.length).toBeGreaterThan(0);
    });
  });

  describe('section styling', () => {
    it('applies the correct background color', () => {
      const { container } = render(<LandingHowItWorks />);
      const section = container.querySelector('section');
      const style = section?.getAttribute('style');
      expect(style).toContain('--sf-surface');
    });
  });
});
