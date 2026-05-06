// WorkflowsListPage.tsx
// The main workflows template management page — rendered at /workflows.
//
// Layout:
//   Top bar: page title "Workflows" + "New workflow" primary action button
//   Grid: responsive card grid (1 col mobile, 2 col md, 3 col lg)
//   States: skeleton loader → real cards → empty state
//
// Empty state logic (from Feature 8 brief):
//   - Global template cards are ALWAYS shown.
//   - The empty state ("No workflows yet…") appears when there are zero
//     workspace-owned (non-global) workflows.
//   - So the empty state and global cards are shown together when the user
//     has no workflows of their own.
//
// Admin detection:
//   - The Delete button on cards is shown only to Admin users.
//   - Role is read from Redux auth state (never from React Query).
//
// Deletion flow:
//   - useDeleteWorkflow mutation lives here.
//   - WorkflowCard receives isDeleting so it can disable buttons while in flight.

import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Button } from '@/modules/shared/ui/components/button';
import { Skeleton } from '@/modules/shared/ui/components/skeleton';
import { useAppSelector } from '@/store/hooks';
import { selectAuth } from '@/store/authSlice';
import { useWorkflows, useDeleteWorkflow } from '../../hooks/use-workflows';
import { WorkflowCard } from '../components/WorkflowCard';

// Number of skeleton cards to show while the list is loading.
// Matches a typical first-load visual weight without a layout shift.
const SKELETON_COUNT = 6;

export function WorkflowsListPage() {
  const navigate = useNavigate();
  const { role } = useAppSelector(selectAuth);
  const isAdmin = role === 'Admin';

  const { data, isLoading, isError } = useWorkflows();
  const deleteMutation = useDeleteWorkflow();

  const workflows = data?.items ?? [];
  const hasWorkspaceWorkflows = workflows.some((w) => !w.isGlobal);
  const showEmptyState = !isLoading && !isError && !hasWorkspaceWorkflows;

  return (
    <div className="flex flex-col gap-6">
      {/* Page header — title + primary action */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-foreground">Workflows</h1>
        <Button onClick={() => navigate('/workflows/new')}>
          <Plus aria-hidden="true" />
          New workflow
        </Button>
      </div>

      {/* Loading state — skeleton grid */}
      {isLoading && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: SKELETON_COUNT }).map((_, i) => (
            <WorkflowCardSkeleton key={i} />
          ))}
        </div>
      )}

      {/* Error state */}
      {isError && (
        <div className="rounded-xl border border-destructive/30 bg-destructive/5 px-6 py-4 text-sm text-destructive">
          Failed to load workflows. Please refresh and try again.
        </div>
      )}

      {/* Loaded state — real workflow cards */}
      {!isLoading && !isError && workflows.length > 0 && (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {workflows.map((workflow) => (
            <WorkflowCard
              key={workflow.id}
              workflow={workflow}
              isAdmin={isAdmin}
              onDelete={(id) => deleteMutation.mutate(id)}
              isDeleting={
                deleteMutation.isPending &&
                deleteMutation.variables === workflow.id
              }
            />
          ))}
        </div>
      )}

      {/* Empty state — shown when the user has no workspace-owned workflows.
          Global template cards are shown above; this message sits below them
          (or alone if there are truly zero workflows). */}
      {showEmptyState && (
        <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-border py-16 text-center">
          <p className="text-base font-medium text-foreground">
            No workflows yet. Build your first one.
          </p>
          <p className="mt-1 text-sm text-muted-foreground">
            Workflow templates define the steps your team follows every time.
          </p>
          <Button className="mt-6" onClick={() => navigate('/workflows/new')}>
            <Plus aria-hidden="true" />
            Create workflow
          </Button>
        </div>
      )}
    </div>
  );
}

// WorkflowCardSkeleton — placeholder card shown during loading.
// Mirrors the WorkflowCard layout so there is no layout shift on data arrival.
function WorkflowCardSkeleton() {
  return (
    <div className="flex flex-col rounded-xl border border-border bg-card p-6 shadow-sm">
      {/* Badge row */}
      <div className="flex gap-2">
        <Skeleton className="h-5 w-14 rounded-full" />
      </div>

      {/* Title */}
      <Skeleton className="mt-3 h-5 w-3/4" />

      {/* Description lines */}
      <Skeleton className="mt-2 h-4 w-full" />
      <Skeleton className="mt-1 h-4 w-2/3" />

      {/* Meta row */}
      <Skeleton className="mt-4 h-4 w-1/2" />

      {/* Button row */}
      <div className="mt-6 flex gap-2">
        <Skeleton className="h-7 w-16 rounded-lg" />
        <Skeleton className="h-7 w-16 rounded-lg" />
      </div>
    </div>
  );
}
