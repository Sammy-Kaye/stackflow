// api-client.ts
// The single Axios instance for the entire application.
//
// WHY one instance: All API calls in the service layer import this client.
// When the base URL, auth headers, or timeout settings need to change,
// this is the only file that changes. No component or hook ever imports
// axios directly — they call their feature's service, which calls this.
//
// Base URL is read from VITE_API_URL at build time. In development this is
// http://localhost:5000 (set in .env.local). In production it is the deployed
// API URL injected by the Docker build.
//
// Request interceptor: reads the access token from the Redux store on every
// request and attaches it as Authorization: Bearer {token}. The store is
// imported directly (not via a hook) because interceptors run outside React.
// If no token is present the header is omitted — the endpoint handles the 401.

import axios from 'axios';
import { store } from '@/store/store';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL as string,
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
