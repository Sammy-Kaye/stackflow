// api-client.ts
// The single Axios instance for the entire application.
//
// WHY one instance: All API calls in the service layer import this client.
// When the base URL, auth headers, or timeout settings need to change,
// this is the only file that changes. No component or hook ever imports
// axios directly — they call their feature's service, which calls this.
//
// Base URL strategy:
//   Development — empty string so all /api/* requests go to the same origin
//                 (:3000) and are forwarded to :5000 by the Vite proxy defined
//                 in vite.config.ts. This avoids CORS issues entirely.
//   Production  — VITE_API_URL is injected at build time (e.g. https://api.stackflow.app).
//
// Request interceptor: reads the access token from the Redux store on every
// request and attaches it as Authorization: Bearer {token}. The store is
// imported directly (not via a hook) because interceptors run outside React.
// If no token is present the header is omitted — the endpoint handles the 401.

import axios from 'axios';
import { store } from '@/store/store';

export const apiClient = axios.create({
  baseURL: import.meta.env.DEV ? '' : (import.meta.env.VITE_API_URL as string),
});

// Attach the bearer token from Redux auth state to every outgoing request.
// Reading store.getState() synchronously is safe here — the interceptor runs
// at request time, so it always picks up the latest token after login.
apiClient.interceptors.request.use((config) => {
  const { accessToken } = store.getState().auth;

  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }

  return config;
});
