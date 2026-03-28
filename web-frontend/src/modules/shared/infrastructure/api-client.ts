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

import axios from 'axios';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL as string,
});
