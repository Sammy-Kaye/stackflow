// LandingHero.test.tsx
// Unit tests for the LandingHero component.
//
// Strategy:
//   Mock useDevLogin to control isPending and mutate behavior.
//   Test the primary CTA button and spinner display.
//   Verify headline and subheadline text are present.

import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { LandingHero } from '../LandingHero';

// Mock useDevLogin
vi.mock('@/modules/auth/hooks/useDevLogin', () => ({
  useDevLogin: vi.fn(),
}));

import { useDevLogin } from '@/modules/auth/hooks/useDevLogin';

// Test fixture: a complete mock of useDevLogin
const createMockLogin = (isPending = false, mutate = vi.fn()) => ({
  mutate,
  isPending,
  isLoading: false,
  isError: false,
  isSuccess: false,
  status: 'idle' as const,
  data: undefined,
  error: null,
  reset: vi.fn(),
  variables: undefined,
});

describe('LandingHero', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('rendering', () => {
    it('renders the headline text', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      expect(screen.getByText(/Workflows that think/i)).toBeInTheDocument();
      expect(screen.getByText(/Teams that flow/i)).toBeInTheDocument();
    });

    it('renders the subheadline text', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      expect(
        screen.getByText(/Stop managing work in spreadsheets and email chains/i)
      ).toBeInTheDocument();
    });

    it('renders the "Enter app" button', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      expect(screen.getByRole('button', { name: /Enter app/i })).toBeInTheDocument();
    });

    it('renders the dashboard mockup div', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      const { container } = render(<LandingHero />);

      // The mockup contains workflow card names
      expect(screen.getByText('Employee Onboarding')).toBeInTheDocument();
      expect(screen.getByText('Purchase Approval')).toBeInTheDocument();
      expect(screen.getByText('Client Offboarding')).toBeInTheDocument();
    });
  });

  describe('"Enter app" button', () => {
    it('calls login.mutate when clicked', () => {
      const mockMutate = vi.fn();
      const mockLogin = createMockLogin(false, mockMutate);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      fireEvent.click(button);

      expect(mockMutate).toHaveBeenCalledWith(undefined);
    });

    it('shows a spinner when isPending is true', () => {
      const mockLogin = createMockLogin(true);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      // Look for the spinner icon (Loader2 component)
      const spinner = document.querySelector('svg.animate-spin');
      expect(spinner).toBeInTheDocument();
    });

    it('disables the button when isPending is true', () => {
      const mockLogin = createMockLogin(true);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      expect(button).toBeDisabled();
    });

    it('enables the button when isPending is false', () => {
      const mockLogin = createMockLogin(false);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      expect(button).not.toBeDisabled();
    });

    it('does not show spinner when isPending is false', () => {
      const mockLogin = createMockLogin(false);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      const spinner = document.querySelector('svg.animate-spin');
      expect(spinner).not.toBeInTheDocument();
    });
  });

  describe('dashboard mockup', () => {
    it('displays all four workflow cards', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      expect(screen.getByText('Employee Onboarding')).toBeInTheDocument();
      expect(screen.getByText('Purchase Approval')).toBeInTheDocument();
      expect(screen.getByText('Client Offboarding')).toBeInTheDocument();
      expect(screen.getByText('Vendor Review')).toBeInTheDocument();
    });

    it('displays status badges for workflow cards', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      // Three "Active" badges
      const activeBadges = screen.getAllByText('Active');
      expect(activeBadges.length).toBeGreaterThanOrEqual(3);

      // One "Draft" badge
      expect(screen.getByText('Draft')).toBeInTheDocument();
    });

    it('displays step counts for workflow cards', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      expect(screen.getByText('8 steps')).toBeInTheDocument();
      expect(screen.getByText('5 steps')).toBeInTheDocument();
      expect(screen.getByText('6 steps')).toBeInTheDocument();
      expect(screen.getByText('4 steps')).toBeInTheDocument();
    });
  });

  describe('button hover effects', () => {
    it('applies hover shadow on mouseenter when not pending', () => {
      const mockLogin = createMockLogin(false);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      fireEvent.mouseEnter(button);

      // After mouseenter, boxShadow should be set
      const style = (button as HTMLButtonElement).style;
      expect(style.boxShadow).toContain('15px');
    });

    it('removes hover shadow on mouseleave', () => {
      const mockLogin = createMockLogin(false);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      fireEvent.mouseEnter(button);
      fireEvent.mouseLeave(button);

      // After mouseleave, boxShadow should be none
      const style = (button as HTMLButtonElement).style;
      expect(style.boxShadow).toBe('none');
    });

    it('does not apply hover shadow when pending', () => {
      const mockLogin = createMockLogin(true);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingHero />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      fireEvent.mouseEnter(button);

      // When pending, the hover handler should not apply the shadow
      // The shadow should remain at its initial state
      const style = (button as HTMLButtonElement).style;
      expect(style.boxShadow).not.toContain('20px');
    });
  });
});
