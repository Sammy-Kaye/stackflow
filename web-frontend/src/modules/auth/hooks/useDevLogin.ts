// useDevLogin.ts
// Hook for the dev-only login flow.
//
// Sequence on click:
//   1. Call authService.devLogin() — POST /api/auth/dev-login
//   2. On success: dispatch setCredentials to populate Redux auth state,
//      then navigate directly to /workflows.
//   3. On error: show a fixed Sonner toast so the developer knows immediately
//      if the API is unreachable or the endpoint is missing.
//
// WHY useMutation instead of useQuery: devLogin is a write operation
// (it creates a token server-side). React Query's useMutation is the correct
// primitive for imperative calls triggered by user action, not for data fetching.
//
// Called from: LandingNavbar, LandingHero, LandingPricing, LandingCta.
// Has no query key because the returned token is stored in Redux, not in the
// React Query cache.

import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { authService } from '../infrastructure/auth-service';
import { setCredentials } from '@/store/authSlice';
import { useAppDispatch } from '@/store/hooks';

export const useDevLogin = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  return useMutation({
    mutationFn: () => authService.devLogin(),

    onSuccess: (response) => {
      // Populate the Redux auth store. The apiClient interceptor will pick up
      // the token immediately on the next request.
      dispatch(setCredentials(response.data));
      navigate('/workflows');
    },

    onError: (error: unknown) => {
      // Always show a friendly message — Axios error details are not useful on a
      // marketing page. Log the raw error so developers can inspect it in DevTools.
      console.error('[useDevLogin]', error);
      toast.error('Could not connect to the server. Is the API running?');
    },
  });
};
