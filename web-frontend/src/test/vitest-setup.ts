// vitest-setup.ts
// Global test setup — runs before every test file in the suite.
//
// Importing @testing-library/jest-dom here extends Vitest's expect with custom
// DOM matchers such as toBeInTheDocument(), toHaveTextContent(), and toBeDisabled().
// Without this import those matchers are undefined and tests that use them throw.

import '@testing-library/jest-dom';
