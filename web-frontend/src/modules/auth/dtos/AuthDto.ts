// AuthDto.ts
// API contract types for the auth module.
//
// These shapes mirror the API contract exactly as specified in the Feature Brief.
// If the backend changes its response shape, update these types — not the components.
//
// DevLoginResponseDto  — response from POST /api/auth/dev-login
// MeDto                — response from GET /api/auth/me

export interface DevLoginResponseDto {
  accessToken: string;
  expiresAt: string; // ISO 8601
  user: {
    id: string;        // UUID string
    email: string;
    role: string;
    workspaceId: string; // UUID string
  };
}

export interface MeDto {
  id: string;          // UUID string
  email: string;
  role: string;
  workspaceId: string; // UUID string
}
