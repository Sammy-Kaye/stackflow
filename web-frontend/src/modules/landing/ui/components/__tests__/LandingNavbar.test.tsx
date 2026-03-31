// LandingNavbar.test.tsx
// Unit tests for the LandingNavbar component.
//
// Strategy:
//   Mock useDevLogin to control the hook's isPending and mutate behavior.
//   Wrap renders in MemoryRouter for link support.
//   Test scroll listener by manually triggering scroll events.
//   Test hamburger menu open/close behavior.

import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { LandingNavbar } from '../LandingNavbar';

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

describe('LandingNavbar', () => {
  beforeEach(() => {
    // Reset mocks and scroll position before each test
    vi.clearAllMocks();
    Object.defineProperty(window, 'scrollY', {
      configurable: true,
      value: 0,
    });
  });

  afterEach(() => {
    // Clean up window properties and event listeners
    vi.clearAllMocks();
  });

  describe('rendering', () => {
    it('renders the StackFlow wordmark', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      expect(screen.getByText('StackFlow')).toBeInTheDocument();
    });

    it('renders nav links (Features, Pricing) on desktop', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      // Desktop links should be in the document
      const featureLinks = screen.getAllByText('Features');
      const pricingLinks = screen.getAllByText('Pricing');
      expect(featureLinks.length).toBeGreaterThan(0);
      expect(pricingLinks.length).toBeGreaterThan(0);
    });

    it('renders the "Enter app" button', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      expect(screen.getByRole('button', { name: /Enter app/i })).toBeInTheDocument();
    });

    it('renders the hamburger menu button on mobile', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      // The hamburger button should have aria-label "Open navigation menu"
      expect(screen.getByLabelText('Open navigation menu')).toBeInTheDocument();
    });
  });

  describe('"Enter app" button', () => {
    it('calls login.mutate when clicked', () => {
      const mockMutate = vi.fn();
      const mockLogin = createMockLogin(false, mockMutate);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      const button = screen.getByRole('button', { name: /Enter app/i });
      fireEvent.click(button);

      expect(mockMutate).toHaveBeenCalledWith(undefined);
    });

    it('shows a spinner when isPending is true', () => {
      const mockLogin = createMockLogin(true);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      // Look for the spinner icon (Loader2 component)
      const spinner = document.querySelector('svg.animate-spin');
      expect(spinner).toBeInTheDocument();
    });

    it('disables the button when isPending is true', () => {
      const mockLogin = createMockLogin(true);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      const button = screen.getByRole('button', { name: /Enter app/i });
      expect(button).toBeDisabled();
    });

    it('enables the button when isPending is false', () => {
      const mockLogin = createMockLogin(false);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      const button = screen.getByRole('button', { name: /Enter app/i });
      expect(button).not.toBeDisabled();
    });
  });

  describe('scroll behaviour', () => {
    it('has a transparent background at scroll position 0', () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      const { container } = render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      const nav = container.querySelector('nav');
      // At scrollY = 0, background should be transparent (initial state)
      const style = nav?.getAttribute('style');
      expect(style).toContain('transparent');
    });

    it('navbar is present and styled', async () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      const { container } = render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      // The navbar should exist and have transition styles applied
      const nav = container.querySelector('nav');
      expect(nav).toBeInTheDocument();
      const navClass = nav?.getAttribute('class');
      // Navbar should have transition and z-index classes
      expect(navClass).toContain('fixed');
      expect(navClass).toContain('transition-all');
    });
  });

  describe('mobile menu', () => {
    it('opens the mobile menu when hamburger is clicked', async () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      const hamburgerButton = screen.getByLabelText('Open navigation menu');
      fireEvent.click(hamburgerButton);

      // After clicking, the mobile menu should open and show the close button
      await waitFor(() => {
        expect(screen.getByLabelText('Close navigation menu')).toBeInTheDocument();
      });
    });

    it('shows nav links and CTA button in mobile overlay menu', async () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      const hamburgerButton = screen.getByLabelText('Open navigation menu');
      fireEvent.click(hamburgerButton);

      // Wait for the overlay to appear and check for nav links
      await waitFor(() => {
        // The overlay menu contains links with these hrefs
        const featureLink = screen.getAllByText('Features').find(
          (el) => el.closest('a')?.getAttribute('href') === '#features'
        );
        const pricingLink = screen.getAllByText('Pricing').find(
          (el) => el.closest('a')?.getAttribute('href') === '#pricing'
        );
        expect(featureLink).toBeInTheDocument();
        expect(pricingLink).toBeInTheDocument();
      });
    });

    it('closes mobile menu when close button is clicked', async () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      // Open the menu
      const hamburgerButton = screen.getByLabelText('Open navigation menu');
      fireEvent.click(hamburgerButton);

      // Wait for close button to appear
      await waitFor(() => {
        const closeButton = screen.getByLabelText('Close navigation menu');
        expect(closeButton).toBeInTheDocument();

        // Click it
        fireEvent.click(closeButton);
      });

      // After closing, the close button should disappear
      await waitFor(() => {
        expect(screen.queryByLabelText('Close navigation menu')).not.toBeInTheDocument();
      });
    });

    it('renders nav links in the mobile overlay when menu is open', async () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      // Open the menu
      const hamburgerButton = screen.getByLabelText('Open navigation menu');
      fireEvent.click(hamburgerButton);

      // Wait for the overlay to appear with nav links
      await waitFor(() => {
        const closeButton = screen.queryByLabelText('Close navigation menu');
        expect(closeButton).toBeInTheDocument();
        // The overlay nav links should be present
        const featureLinks = screen.getAllByText('Features');
        expect(featureLinks.length).toBeGreaterThan(1); // desktop + overlay
      });
    });

    it('closes mobile menu when "Enter app" button in overlay is clicked', async () => {
      const mockMutate = vi.fn();
      const mockLogin = createMockLogin(false, mockMutate);
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      // Open the menu
      const hamburgerButton = screen.getByLabelText('Open navigation menu');
      fireEvent.click(hamburgerButton);

      // Wait for overlay to appear and the button to be present
      await waitFor(() => {
        expect(screen.getByLabelText('Close navigation menu')).toBeInTheDocument();
      });

      // Find and click the overlay's "Enter app" button (both buttons have the same text)
      const enterAppButtons = screen.getAllByRole('button', { name: /Enter app/i });
      // The last one should be the overlay button
      fireEvent.click(enterAppButtons[enterAppButtons.length - 1]);

      // Verify mutate was called
      expect(mockMutate).toHaveBeenCalledWith(undefined);

      // Menu should close
      await waitFor(() => {
        expect(screen.queryByLabelText('Close navigation menu')).not.toBeInTheDocument();
      });
    });

    it('closes mobile menu when viewport resizes past md breakpoint', async () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      // Open the menu
      const hamburgerButton = screen.getByLabelText('Open navigation menu');
      fireEvent.click(hamburgerButton);

      // Verify menu is open
      await waitFor(() => {
        expect(screen.getByLabelText('Close navigation menu')).toBeInTheDocument();
      });

      // Simulate resize past md breakpoint (768px)
      Object.defineProperty(window, 'innerWidth', {
        configurable: true,
        value: 800,
      });
      fireEvent.resize(window);

      // Menu should close
      await waitFor(() => {
        expect(screen.queryByLabelText('Close navigation menu')).not.toBeInTheDocument();
      });
    });
  });

  describe('body scroll lock', () => {
    it('prevents body scroll when mobile menu is open', async () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      // Open the menu
      const hamburgerButton = screen.getByLabelText('Open navigation menu');
      fireEvent.click(hamburgerButton);

      // Wait for menu to open
      await waitFor(() => {
        expect(screen.getByLabelText('Close navigation menu')).toBeInTheDocument();
      });

      // Body overflow should be set to hidden
      expect(document.body.style.overflow).toBe('hidden');
    });

    it('restores body scroll when mobile menu is closed', async () => {
      const mockLogin = createMockLogin();
      vi.mocked(useDevLogin).mockReturnValue(mockLogin);

      render(
        <MemoryRouter>
          <LandingNavbar />
        </MemoryRouter>
      );

      // Open the menu
      const hamburgerButton = screen.getByLabelText('Open navigation menu');
      fireEvent.click(hamburgerButton);

      await waitFor(() => {
        expect(document.body.style.overflow).toBe('hidden');
      });

      // Close the menu
      const closeButton = screen.getByLabelText('Close navigation menu');
      fireEvent.click(closeButton);

      // Body overflow should be restored
      await waitFor(() => {
        expect(document.body.style.overflow).toBe('');
      });
    });
  });
});
