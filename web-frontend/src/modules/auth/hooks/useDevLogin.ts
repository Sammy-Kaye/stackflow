// useDevLogin.ts
// Hook for the dev-only login flow.
//
// Sequence on click:
//   1. Call authService.devLogin() — POST /api/auth/dev-login
//   2. On success: dispatch setCredentials to populate Redux auth state,
//      then navigate to / (the app shell root).
//   3. On error: show a Sonner toast with the error message so the developer
//      knows immediately if the API is unreachable or the endpoint is missing.
//
// WHY useMutation instead of useQuery: devLogin is a write operation
// (it creates a token server-side). React Query's useMutation is the correct
// primitive for imperative calls triggered by user action, not for data fetching.
//
// This hook is only called from DevLoginPage. It has no query key because
// the returned token is stored in Redux, not in the React Query cache.

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
      navigate('/');
    },

    onError: (error: Error) => {
      toast.error(error.message ?? 'Dev login failed. Is the API running?');
    },
  });
};
