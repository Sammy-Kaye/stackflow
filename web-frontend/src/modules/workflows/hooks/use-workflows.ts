// use-workflows.ts
// React Query hooks for the workflows module.
//
// Query key convention:
//   ['workflows']       — the full list (useWorkflows)
//   ['workflows', id]   — a single workflow by id (useWorkflow)
//
// Mutation hooks invalidate the cache so the UI stays consistent without
// requiring a manual page refresh:
//   useCreateWorkflow — invalidates ['workflows'] on success
//   useUpdateWorkflow — invalidates ['workflows'] and ['workflows', id] on success
//   useDeleteWorkflow — invalidates ['workflows'] on success; shows Sonner toasts

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { isAxiosError } from 'axios';
import { toast } from 'sonner';
import { workflowService } from '../infrastructure/workflow-service';
import type { CreateWorkflowDto, UpdateWorkflowDto } from '../dtos/workflow-dtos';

// Central query key registry — consistent invalidation across the module.
export const workflowKeys = {
  all: ['workflows'] as const,
  byId: (id: string) => ['workflows', id] as const,
};

// useWorkflows — fetches the full workflow list including global templates.
// Query key: ['workflows']
export function useWorkflows() {
  return useQuery({
    queryKey: workflowKeys.all,
    queryFn: () => workflowService.getAll().then((res) => res.data),
  });
}

// useWorkflow — fetches a single workflow with its full task list.
// Query key: ['workflows', id]
// Disabled when id is undefined so callers can pass a potentially absent id.
export function useWorkflow(id: string | undefined) {
  return useQuery({
    queryKey: workflowKeys.byId(id ?? ''),
    queryFn: () => workflowService.getById(id!).then((res) => res.data),
    enabled: !!id,
  });
}

// useCreateWorkflow — creates a new workflow template.
// Invalidates ['workflows'] so the list page reflects the new entry immediately.
export function useCreateWorkflow() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (dto: CreateWorkflowDto) =>
      workflowService.create(dto).then((res) => res.data),

    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: workflowKeys.all });
    },
  });
}

// useUpdateWorkflow — replaces a workflow's header fields and task list.
// Invalidates both the list and the specific workflow's detail cache.
export function useUpdateWorkflow() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, dto }: { id: string; dto: UpdateWorkflowDto }) =>
      workflowService.update(id, dto).then((res) => res.data),

    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: workflowKeys.all });
      void queryClient.invalidateQueries({
        queryKey: workflowKeys.byId(variables.id),
      });
    },
  });
}

// useDeleteWorkflow — hard-deletes a workflow and all its task records.
// Shows a success toast on completion and an error toast when the backend
// returns 400 (e.g. the user tried to delete a global starter template).
export function useDeleteWorkflow() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => workflowService.remove(id),

    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: workflowKeys.all });
      toast.success('Workflow deleted');
    },

    onError: (error: unknown) => {
      // Surface the backend error message for 400 responses (e.g. global
      // template protection), fall back to a generic message for anything else.
      if (isAxiosError(error) && error.response?.status === 400) {
        const message: string =
          (error.response.data as { error?: string })?.error ??
          'Could not delete this workflow.';
        toast.error(message);
      } else {
        toast.error('Something went wrong. Please try again.');
      }
    },
  });
}
