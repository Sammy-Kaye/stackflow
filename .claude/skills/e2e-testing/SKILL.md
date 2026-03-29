---
name: e2e-testing
description: >
  Write and run Playwright E2E tests for StackFlow. Loaded once at session start
  by test-agent when writing E2E tests. Do not auto-load — load explicitly when
  the task specifically involves end-to-end or browser-level tests.
allowed-tools: Read, Write, Edit, Bash
---

<!--
  WHY THIS SKILL EXISTS
  ──────────────────────
  Unit and integration tests verify that individual layers work in isolation.
  E2E tests verify that the whole system — frontend, API, database — works
  together from the user's perspective.

  StackFlow has real users doing real things: creating workflows, completing
  tasks, approving requests via token links. E2E tests are the only automated
  check that proves the complete user journey works end to end.

  THE DUAL-MODE RELATIONSHIP WITH PLAYWRIGHT MCP:
  This skill covers TEST WRITING — producing .spec.ts files that live in
  the repo and run in CI. The Playwright MCP is for EXPLORATORY TESTING —
  Samuel manually drives the browser to investigate behaviour.

  Both use Playwright. Both matter. This skill is the write-the-file mode.

  WHEN TO ADD THIS SKILL:
  This skill is added before the Real Testing phase (Step 6 in the feature
  build loop) — not at project start. Add it when:
    - Playwright MCP is installed
    - Playwright browsers are installed (npx playwright install)
    - playwright.config.ts is created at project root
    - tests/e2e/helpers/auth.ts exists

  IF CLAUDE CODE CEASES TO EXIST:
  A developer can write E2E tests manually by following the patterns below.
  The auth helper, selector conventions, coverage map, and spec format are
  all defined here — no external knowledge required.
-->

# StackFlow — E2E Testing (Playwright)

---

## Setup & Prerequisites

```
Framework:    Playwright with TypeScript
Test root:    tests/e2e/
Base URL:     http://localhost:3000
Backend API:  http://localhost:5000
Config:       playwright.config.ts (project root)
```

**Both the frontend and backend must be running before E2E tests execute.**
Use `docker compose up -d` to start all services, then run tests.

---

## Installation (First Time Only)

```bash
# Install Playwright browsers — run once after installing the package
npx playwright install

# Verify installation
npx playwright --version
```

---

## playwright.config.ts — Standard Configuration

```typescript
// playwright.config.ts — project root
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: false,          // Sequential — StackFlow tests share DB state
  retries: process.env.CI ? 2 : 0,
  workers: 1,                    // One worker — prevents test interference
  reporter: [['html'], ['list']],
  use: {
    baseURL: 'http://localhost:3000',
    trace: 'on-first-retry',     // Capture trace on first retry for debugging
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
  // Start dev server before tests if not already running
  // webServer: { command: 'docker compose up -d', reuseExistingServer: true },
});
```

---

## Auth Helper — Always Use This, Never Repeat Login Logic

Every test that requires authentication must use this helper.
Never repeat the login flow inline in a test.

```typescript
// tests/e2e/helpers/auth.ts
import { Page } from '@playwright/test';

export type TestRole = 'admin' | 'member';

const testCredentials: Record<TestRole, { email: string; password: string }> = {
  admin: {
    email: 'admin@stackflow.test',
    password: 'Admin1234!',
  },
  member: {
    email: 'member@stackflow.test',
    password: 'Member1234!',
  },
};

export async function loginAs(page: Page, role: TestRole): Promise<void> {
  await page.goto('/login');
  await page.getByTestId('email-input').fill(testCredentials[role].email);
  await page.getByTestId('password-input').fill(testCredentials[role].password);
  await page.getByTestId('login-submit').click();
  await page.waitForURL('/app/dashboard');
}

export async function logout(page: Page): Promise<void> {
  await page.getByTestId('user-menu').click();
  await page.getByTestId('logout-button').click();
  await page.waitForURL('/login');
}
```

**Why a helper:**
Login involves multiple steps across pages. If the login flow changes,
you update one file — not every test that needs authentication.

---

## Selector Convention — Always `data-testid`

```typescript
// ✅ CORRECT — stable, survives CSS changes and refactors
page.getByTestId('create-workflow-btn')
page.getByTestId('workflow-name-input')
page.getByTestId('delete-confirm-dialog')
page.getByTestId('task-status-badge')

// ❌ WRONG — brittle, breaks when styles change
page.locator('.btn-primary')
page.locator('.workflow-card:first-child')

// ❌ WRONG — breaks when copy changes
page.getByText('Create Workflow')
page.getByText('Delete')
```

**Every interactive element and every testable output must have a `data-testid` attribute.**
When writing E2E tests, if a `data-testid` is missing from a component, add it to
the component first, then reference it in the test.

### `data-testid` Naming Convention

```
{entity}-{element-type}-{optional-qualifier}

Examples:
  workflow-card               ← a single workflow card
  workflow-card-title         ← title inside a workflow card
  create-workflow-btn         ← the create button
  workflow-name-input         ← name field in the create form
  delete-workflow-dialog      ← confirmation dialog
  delete-workflow-confirm     ← confirm button inside dialog
  task-status-badge           ← status badge on a task
  task-status-badge-completed ← badge when status is Completed
```

---

## Test File Structure

```
tests/e2e/
├── helpers/
│   └── auth.ts              ← Shared auth helper — never duplicate login logic
├── auth.spec.ts             ← All authentication flows
├── workflows.spec.ts        ← Workflow CRUD + builder interactions
├── tasks.spec.ts            ← Task assignment, completion, decline
├── approvals.spec.ts        ← Approval node flows (Phase 2)
├── tokens.spec.ts           ← External task token flows
├── notifications.spec.ts    ← In-app notification flows (Phase 2)
├── guards.spec.ts           ← Route guard and permission tests
└── regression/
    └── {bug-slug}.spec.ts   ← One file per reported regression
```

---

## Standard Spec File Pattern

```typescript
// tests/e2e/workflows.spec.ts
import { test, expect } from '@playwright/test';
import { loginAs } from './helpers/auth';

// Group related tests in a describe block
test.describe('Workflow CRUD', () => {

  // Use beforeEach for setup that every test in the block requires
  test.beforeEach(async ({ page }) => {
    await loginAs(page, 'admin');
  });

  test('creates a new workflow and shows it in the template list', async ({ page }) => {
    // Arrange — navigate to the right page
    await page.goto('/app/workflows');

    // Act — perform the user action
    await page.getByTestId('create-workflow-btn').click();
    await page.getByTestId('workflow-name-input').fill('Onboarding Flow');
    await page.getByTestId('workflow-description-input').fill('New hire process');
    await page.getByTestId('create-workflow-submit').click();

    // Assert — verify the outcome
    await expect(page.getByTestId('workflow-card').filter({ hasText: 'Onboarding Flow' }))
      .toBeVisible();
  });

  test('shows confirmation dialog before deleting a workflow', async ({ page }) => {
    await page.goto('/app/workflows');

    // Trigger the delete action
    await page.getByTestId('workflow-card').first().getByTestId('workflow-menu').click();
    await page.getByTestId('delete-workflow-option').click();

    // Dialog must appear — delete has NOT happened yet
    await expect(page.getByTestId('delete-workflow-dialog')).toBeVisible();
    await expect(page.getByTestId('delete-workflow-dialog'))
      .toContainText('cannot be undone');
  });

  test('deletes a workflow after confirmation', async ({ page }) => {
    await page.goto('/app/workflows');
    const workflowName = await page.getByTestId('workflow-card').first()
      .getByTestId('workflow-card-title').textContent();

    await page.getByTestId('workflow-card').first().getByTestId('workflow-menu').click();
    await page.getByTestId('delete-workflow-option').click();
    await page.getByTestId('delete-workflow-confirm').click();

    // Verify it's gone
    await expect(page.getByText(workflowName!)).not.toBeVisible();
  });
});
```

---

## Coverage Map — What to Test Per Feature

Build E2E tests to cover these flows. Not every scenario needs E2E coverage —
focus on the user journeys that cross multiple layers.

### Auth flows (`auth.spec.ts`)
```
□ Email+password login → redirects to /app/dashboard
□ Invalid credentials → error toast shown, stay on /login
□ Forgot password form → submits, success message shown
□ OTP entry → correct code → authenticated and redirected
□ Google OAuth button → visible and clickable on login page
□ Unauthenticated access to /app/dashboard → redirected to /login
```

### Workflow CRUD (`workflows.spec.ts`)
```
□ Create workflow → appears in template list
□ Edit workflow name → updated in list
□ Delete workflow → confirmation dialog shown first
□ Delete workflow → confirmed → removed from list
□ Admin role can access workflow management
□ Member role cannot access /app/admin routes
```

### Task execution (`tasks.spec.ts`)
```
□ Assign task → appears in assignee's My Tasks view
□ Complete task → status badge updates to Completed
□ Decline task → reason field appears → required
□ Overdue task → amber overdue badge visible
□ Task with no due date → no overdue indicator
```

### External task tokens (`tokens.spec.ts`)
```
□ GET /complete/:valid-token → page loads without authentication
□ GET /complete/:expired-token → shows "link has expired" message
□ GET /complete/:used-token → shows "already completed" message
□ External token page → submits completion notes → success state shown
```

### Route guards (`guards.spec.ts`)
```
□ /app/dashboard → /login redirect when unauthenticated (no stored token)
□ /app/admin → /app/dashboard redirect for Member role
□ /complete/:token → accessible with no auth headers (public route)
□ After logout → /app/dashboard → redirected back to /login
```

---

## Running Tests

```bash
# Run all E2E tests
npx playwright test

# Run a specific file
npx playwright test tests/e2e/workflows.spec.ts

# Run a specific test by name
npx playwright test --grep "creates a new workflow"

# Run with visible browser (debug mode)
npx playwright test --headed

# Run in debug mode (step through)
npx playwright test --debug

# Show HTML test report after run
npx playwright show-report

# Show trace for a failed test
npx playwright show-trace test-results/trace.zip
```

---

## Regression Test Format

When Samuel reports a bug from real testing, write a regression test that
would have caught it. This test lives permanently in `tests/e2e/regression/`.

```typescript
// tests/e2e/regression/task-decline-required-reason.spec.ts

// Regression: declining a task without a reason did not show validation error
// Reported: 2025-04-10 — Samuel (Real Tester)
// Root cause: Zod schema marked reason as optional; should be required on Declined status

import { test, expect } from '@playwright/test';
import { loginAs } from '../helpers/auth';

test('regression: declining a task without a reason shows a validation error', async ({ page }) => {
  await loginAs(page, 'member');
  await page.goto('/app/tasks');

  // Open the task action menu
  await page.getByTestId('task-card').first().getByTestId('task-action-menu').click();
  await page.getByTestId('decline-task-option').click();

  // Try to submit without entering a reason
  await page.getByTestId('decline-task-submit').click();

  // Validation error must appear — this was the bug
  await expect(page.getByTestId('decline-reason-error'))
    .toContainText('Reason is required');

  // Task must NOT be declined without a reason
  await expect(page.getByTestId('task-card').first().getByTestId('task-status-badge'))
    .not.toContainText('Declined');
});
```

**Regression test rules:**
- One file per bug — named after the bug, not the feature
- Comment at top: what broke, when reported, root cause
- Test name starts with `regression:` — makes them identifiable in reports
- Reproduces the exact user steps that triggered the original bug
- Verifies the bug is fixed AND the state before the bug is also verified

---

## Playwright MCP vs Spec Files — When to Use Which

| Situation | Use |
|---|---|
| Samuel exploring a new feature manually | Playwright MCP (browser automation in session) |
| Verifying a specific user journey works | Playwright MCP (ad-hoc check) |
| Writing a test that runs in CI forever | This skill — write a `.spec.ts` file |
| Regression test after a bug fix | This skill — `tests/e2e/regression/` |
| Debugging why a page looks wrong | Playwright MCP (`--headed` + pause) |

---

## What You Must Never Do

- Use CSS class selectors or text content selectors — always `data-testid`
- Repeat login logic inline — always import and use `loginAs()` from the auth helper
- Write a test against a page that does not have `data-testid` attributes — add them first
- Write tests that depend on test execution order — each test must be independently runnable
- Hard-code user credentials anywhere except `helpers/auth.ts`
- Test implementation details — test what the user sees and does, not internal state
- Leave a test that fails locally unchecked — fix or skip with an explanation before committing
- Write E2E tests for things that unit tests already cover well — E2E is for user journeys
