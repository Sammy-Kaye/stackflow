// auth-service.ts
// Service layer for the auth module — the only file that knows the auth API exists.
//
// WHY a service layer: If the endpoint path, base URL, or request shape changes,
// this is the only file that needs updating. No hook or component imports
// apiClient directly — they call this service, which calls apiClient.
//
// devLogin — POST /api/auth/dev-login
//   Accepts no credentials. Returns a signed JWT with a hardcoded dev identity.
//   Only available when the API is running in Development environment.
//
// me — GET /api/auth/me
//   JWT required. Returns the decoded identity claims from the bearer token.

import { apiClient } from '@/modules/shared/infrastructure/api-client';
import type { DevLoginResponseDto, MeDto } from '../dtos/AuthDto';

export const authService = {
  devLogin: () =>
    apiClient.post<DevLoginResponseDto>('/api/auth/dev-login'),

  me: () =>
    apiClient.get<MeDto>('/api/auth/me'),
};
