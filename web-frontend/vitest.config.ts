// vitest.config.ts
// Vitest configuration for the StackFlow frontend test suite.
//
// environment: jsdom — provides browser globals (window, document, localStorage)
// that React components expect. Tests run in Node but behave as if in a browser.
//
// setupFiles: runs vitest-setup.ts before every test file. That file imports
// @testing-library/jest-dom so its custom matchers (toBeInTheDocument, etc.)
// are available in every test without explicit imports.
//
// resolve.alias: mirrors the @ alias defined in vite.config.ts so imports like
// '@/store/authSlice' resolve correctly inside tests.

import path from 'path';
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/vitest-setup.ts'],
    globals: true,
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});
