// LandingCta.test.tsx
// Unit tests for the LandingCta component.
//
// Strategy:
//   Mock useDevLogin to control isPending and mutate behavior.
//   Test the CTA button and spinner display.
//   Verify headline and subheadline text are present.

import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { LandingCta } from '../LandingCta';

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

describe('LandingCta', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('rendering', () => {
    it('renders the headline text', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingCta />);

      expect(screen.getByText(/Ready to stop managing work manually/i)).toBeInTheDocument();
    });

    it('renders the subheadline text', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingCta />);

      expect(screen.getByText(/Join teams that have made their workflows intelligent/i)).toBeInTheDocument();
    });

    it('renders the "Enter app" button', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingCta />);

      expect(screen.getByRole('button', { name: /Enter app/i })).toBeInTheDocument();
    });
  });

  describe('"Enter app" button', () => {
    it('calls login.mutate when clicked', () => {
      const mockMutate = vi.fn();
      const mockLogin = createMockLogin(false, mockMutate);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingCta />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      fireEvent.click(button);

      expect(mockMutate).toHaveBeenCalledWith(undefined);
    });

    it('shows a spinner when isPending is true', () => {
      const mockLogin = createMockLogin(true);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingCta />);

      // Look for the spinner icon (Loader2 component)
      const spinner = document.querySelector('svg.animate-spin');
      expect(spinner).toBeInTheDocument();
    });

    it('disables the button when isPending is true', () => {
      const mockLogin = createMockLogin(true);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingCta />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      expect(button).toBeDisabled();
    });

    it('enables the button when isPending is false', () => {
      const mockLogin = createMockLogin(false);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingCta />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      expect(button).not.toBeDisabled();
    });
  });

  describe('button hover effects', () => {
    it('applies hover shadow on mouseenter when not pending', () => {
      const mockLogin = createMockLogin(false);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingCta />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      fireEvent.mouseEnter(button);

      // After mouseenter, boxShadow should be set
      const style = (button as HTMLButtonElement).style;
      expect(style.boxShadow).toContain('25px');
    });

    it('restores shadow on mouseleave', () => {
      const mockLogin = createMockLogin(false);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingCta />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      fireEvent.mouseEnter(button);
      fireEvent.mouseLeave(button);

      // After mouseleave, boxShadow should be reset to initial
      const style = (button as HTMLButtonElement).style;
      expect(style.boxShadow).toContain('20px');
    });

    it('does not apply hover shadow when pending', () => {
      const mockLogin = createMockLogin(true);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(<LandingCta />);

      const button = screen.getByRole('button', { name: /Enter app/i });
      fireEvent.mouseEnter(button);

      // When pending, the hover handler should not apply the enhanced shadow
      const style = (button as HTMLButtonElement).style;
      expect(style.boxShadow).not.toContain('25px');
    });
  });

  describe('styling', () => {
    it('renders the card with correct background', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      const { container } = render(<LandingCta />);
      const card = container.querySelector('div[style*="surface-container-high"]');
      expect(card).toBeInTheDocument();
    });

    it('renders with relative positioning for z-index layering', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      const { container } = render(<LandingCta />);
      const card = container.querySelector('div.relative');
      expect(card).toBeInTheDocument();
    });
  });
});
