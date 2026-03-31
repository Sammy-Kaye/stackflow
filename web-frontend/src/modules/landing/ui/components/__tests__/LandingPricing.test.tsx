// LandingPricing.test.tsx
// Unit tests for the LandingPricing component.
//
// Strategy:
//   Mock useDevLogin to control isPending and mutate behavior.
//   Each pricing CTA button gets its own useDevLogin instance, so test
//   that each button's spinner state is independent.
//   Test the MOST POPULAR badge is only on the Team tier.

import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { LandingPricing } from '../LandingPricing';

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

describe('LandingPricing', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('rendering', () => {
    it('renders the section with id="pricing"', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      const { container } = render(<LandingPricing />);
      const section = container.querySelector('section#pricing');
      expect(section).toBeInTheDocument();
    });

    it('renders the section headline', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);
      expect(screen.getByText('Scalable pricing for every team')).toBeInTheDocument();
    });

    it('renders the section subheadline', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);
      expect(
        screen.getByText(/Fair pricing that grows as your efficiency improves/i)
      ).toBeInTheDocument();
    });
  });

  describe('pricing tiers', () => {
    it('renders all three pricing tiers: Starter, Team, Pro', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      expect(screen.getByText('Starter')).toBeInTheDocument();
      expect(screen.getByText('Team')).toBeInTheDocument();
      expect(screen.getByText('Pro')).toBeInTheDocument();
    });

    it('displays correct prices: $0, $29, $79', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      expect(screen.getByText('$0')).toBeInTheDocument();
      expect(screen.getByText('$29')).toBeInTheDocument();
      expect(screen.getByText('$79')).toBeInTheDocument();
    });

    it('displays period "/mo" for all tiers', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      const periods = screen.getAllByText('/mo');
      expect(periods.length).toBe(3);
    });
  });

  describe('MOST POPULAR badge', () => {
    it('displays "MOST POPULAR" badge only on Team tier', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      const badge = screen.getByText('MOST POPULAR');
      expect(badge).toBeInTheDocument();

      // The badge should be a sibling of the Team pricing card
      const cardParent = badge.closest('div[style*="transform"]');
      expect(cardParent).toBeInTheDocument();
      expect(cardParent?.textContent).toContain('Team');
    });

    it('does not display badge on Starter or Pro tiers', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      // There should be exactly one badge
      const badges = screen.getAllByText('MOST POPULAR');
      expect(badges.length).toBe(1);
    });
  });

  describe('feature lists', () => {
    it('renders features for the Starter tier', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      expect(screen.getByText('3 workflows')).toBeInTheDocument();
      expect(screen.getByText('5 team members')).toBeInTheDocument();
      expect(screen.getByText('Basic templates')).toBeInTheDocument();
      expect(screen.getByText('Email notifications')).toBeInTheDocument();
    });

    it('renders features for the Team tier', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      expect(screen.getByText('Unlimited workflows')).toBeInTheDocument();
      expect(screen.getByText('25 team members')).toBeInTheDocument();
      expect(screen.getByText('All node types')).toBeInTheDocument();
      expect(screen.getByText('Audit trail')).toBeInTheDocument();
      expect(screen.getByText('Priority support')).toBeInTheDocument();
    });

    it('renders features for the Pro tier', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      expect(screen.getByText('Everything in Team')).toBeInTheDocument();
      expect(screen.getByText('Unlimited members')).toBeInTheDocument();
      expect(screen.getByText('Custom branding')).toBeInTheDocument();
      expect(screen.getByText('API access')).toBeInTheDocument();
      expect(screen.getByText('SLA')).toBeInTheDocument();
    });
  });

  describe('CTA buttons', () => {
    it('renders three CTA buttons, one for each tier', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      const buttons = screen.getAllByRole('button');
      expect(buttons.length).toBeGreaterThanOrEqual(3);
    });

    it('renders "Get started free" button for Starter tier', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      expect(screen.getByText('Get started free')).toBeInTheDocument();
    });

    it('renders "Start with Team" button for Team tier', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      expect(screen.getByText('Start with Team')).toBeInTheDocument();
    });

    it('renders "Start with Pro" button for Pro tier', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      render(<LandingPricing />);

      expect(screen.getByText('Start with Pro')).toBeInTheDocument();
    });
  });

  describe('button click behavior', () => {
    it('calls login.mutate when Starter button is clicked', () => {
      const mockMutate = vi.fn();
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin(false, mockMutate));

      render(<LandingPricing />);

      const starterButton = screen.getByText('Get started free');
      fireEvent.click(starterButton);

      expect(mockMutate).toHaveBeenCalledWith(undefined);
    });

    it('calls login.mutate when Team button is clicked', () => {
      const mockMutate = vi.fn();
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin(false, mockMutate));

      render(<LandingPricing />);

      const teamButton = screen.getByText('Start with Team');
      fireEvent.click(teamButton);

      expect(mockMutate).toHaveBeenCalledWith(undefined);
    });

    it('calls login.mutate when Pro button is clicked', () => {
      const mockMutate = vi.fn();
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin(false, mockMutate));

      render(<LandingPricing />);

      const proButton = screen.getByText('Start with Pro');
      fireEvent.click(proButton);

      expect(mockMutate).toHaveBeenCalledWith(undefined);
    });
  });

  describe('independent spinner states', () => {
    it('shows spinner only on the Starter button when it is pending', () => {
      // Create separate mock instances for each button
      const starterMock = createMockLogin(true);
      const teamMock = createMockLogin(false);
      const proMock = createMockLogin(false);

      // Mock useDevLogin to return different instances based on call count
      let callCount = 0;
      vi.mocked(useDevLogin).mockImplementation(() => {
        const mocks = [starterMock, teamMock, proMock];
        const result = mocks[callCount % 3];
        callCount++;
        return result;
      });

      render(<LandingPricing />);

      // Starter button should be disabled
      const starterButton = screen.getByText('Get started free') as HTMLButtonElement;
      expect(starterButton).toBeDisabled();

      // Team and Pro buttons should NOT be disabled
      const teamButton = screen.getByText('Start with Team') as HTMLButtonElement;
      const proButton = screen.getByText('Start with Pro') as HTMLButtonElement;
      expect(teamButton).not.toBeDisabled();
      expect(proButton).not.toBeDisabled();
    });

    it('shows spinner only on the Team button when it is pending', () => {
      const starterMock = createMockLogin(false);
      const teamMock = createMockLogin(true);
      const proMock = createMockLogin(false);

      let callCount = 0;
      vi.mocked(useDevLogin).mockImplementation(() => {
        const mocks = [starterMock, teamMock, proMock];
        const result = mocks[callCount % 3];
        callCount++;
        return result;
      });

      render(<LandingPricing />);

      // Team button should be disabled
      const teamButton = screen.getByText('Start with Team') as HTMLButtonElement;
      expect(teamButton).toBeDisabled();

      // Starter and Pro buttons should NOT be disabled
      const starterButton = screen.getByText('Get started free') as HTMLButtonElement;
      const proButton = screen.getByText('Start with Pro') as HTMLButtonElement;
      expect(starterButton).not.toBeDisabled();
      expect(proButton).not.toBeDisabled();
    });

    it('shows spinner only on the Pro button when it is pending', () => {
      const starterMock = createMockLogin(false);
      const teamMock = createMockLogin(false);
      const proMock = createMockLogin(true);

      let callCount = 0;
      vi.mocked(useDevLogin).mockImplementation(() => {
        const mocks = [starterMock, teamMock, proMock];
        const result = mocks[callCount % 3];
        callCount++;
        return result;
      });

      render(<LandingPricing />);

      // Pro button should be disabled
      const proButton = screen.getByText('Start with Pro') as HTMLButtonElement;
      expect(proButton).toBeDisabled();

      // Starter and Team buttons should NOT be disabled
      const starterButton = screen.getByText('Get started free') as HTMLButtonElement;
      const teamButton = screen.getByText('Start with Team') as HTMLButtonElement;
      expect(starterButton).not.toBeDisabled();
      expect(teamButton).not.toBeDisabled();
    });
  });

  describe('tier styling', () => {
    it('applies scale transform to Team tier', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      const { container } = render(<LandingPricing />);

      // The Team card should have a scale transform
      const cards = container.querySelectorAll('div[style*="transform"]');
      let foundTeamCard = false;
      cards.forEach((card) => {
        if (card.textContent?.includes('Team') && card.textContent?.includes('$29')) {
          foundTeamCard = true;
        }
      });
      expect(foundTeamCard).toBe(true);
    });

    it('applies outline to Team tier', () => {
      vi.mocked(useDevLogin).mockReturnValue(createMockLogin());

      const { container } = render(<LandingPricing />);

      // The Team card should have an outline
      const cards = container.querySelectorAll('div[style*="outline"]');
      let foundTeamCard = false;
      cards.forEach((card) => {
        if (card.textContent?.includes('Team') && card.textContent?.includes('$29')) {
          foundTeamCard = true;
        }
      });
      expect(foundTeamCard).toBe(true);
    });
  });
});
